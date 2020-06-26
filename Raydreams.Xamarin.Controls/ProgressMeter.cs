using System;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Linq;

namespace Raydreams.Xamarin.Controls
{
	/// <summary>Rendering Mode of the meter</summary>
	public enum MeterStyle
	{
        /// <summary>A single sweep line to show percent closed.</summary>
		Progress = 0,
		/// <summary>Really a Pie chart of two sweeps lines.</summary>
		Pie = 1, 
	}

	/// <summary>Which Label Position</summary>
	public enum LabelPosition
	{
		/// <summary>The primary label position</summary>
		Prime = 0,
		/// <summary>The sub label position.</summary>
		Sub = 1,
	}

	/// <summary>Profress meter is an activity ring with 2 modes, showing either a percent in Pie mode or up to 100% in Progress mode.</summary>
	public partial class ProgressMeter : ContentView
	{
		/// <summary>border fudge factor that insets the graph slightly more</summary>
		public static readonly float BorderFactor = 1.2F;

		/// <summary>stock colors</summary>
		private static SKColor _disabled = new SKColor( 128, 128, 128 );
		private static SKColor _default2 = new SKColor( 0, 0, 255 );
		private static SKColor _test = new SKColor( 255, 0, 0 );
		private static SKColor _orange = new SKColor( 255, 165, 0 );

		/// <summary>default font scales are 100% max</summary>
		private float _primeFontScale = 100.0F;
		private float _subFontScale = 100.0F;

		private float _arcLineThickness = 1.0F;

		#region [ Constructors ]

		/// <summary>Default constructor</summary>
		public ProgressMeter() : this( MeterStyle.Progress )
		{
		}

		/// <summary>Explicit mode</summary>
		public ProgressMeter( MeterStyle mode )
		{
			this.Mode = mode;

			// set the single child view and event handlers
			this.MasterCanvas = new SKCanvasView();
			this.Content = this.MasterCanvas;
			this.MasterCanvas.PaintSurface += OnCanvasViewPaintSurface;

			this.PrimeLabel = String.Empty;
			this.SubLabel = String.Empty;
		}

		#endregion [ Constructors ]

		#region [ Properties ]

		/// <summary>the top level canvas to draw on</summary>
		public SKCanvasView MasterCanvas { get; set; }

		/// <summary>What style to draw the control in</summary>
		public MeterStyle Mode { get; set; }

		/// <summary>Percent adjustment of the actual arc line thickness</summary>
		public float ArcLineThickness
		{
			get { return this._arcLineThickness; }
			set
			{
				if (value < 0.02F)
					this._arcLineThickness = 0.02F;
				else if (value > 10.0F)
					this._arcLineThickness = 10.0F;
				else
					this._arcLineThickness = value;
			}
		}

		/// <summary>Whether to Draw the layout helper grid</summary>
		public bool LayoutGrid { get; set; }

		/// <summary>Color to show the the background of the ring</summary>
		public Color DisabledColor
		{
			get { return (Color)GetValue( DisabledColorProperty ); }
			set { SetValue( DisabledColorProperty, value ); }
		}

		/// <summary>Primary Arc Color</summary>
		public Color ArcColor1
		{
			get { return (Color)GetValue( ArcColor1Property ); }
			set { SetValue( ArcColor1Property, value ); }
		}

		/// <summary>Secondary Arc Color</summary>
		public Color ArcColor2
		{
			get { return (Color)GetValue( ArcColor2Property ); }
			set { SetValue( ArcColor2Property, value ); }
		}

		/// <summary>The angle in degrees where the first sweep ends and second begins</summary>
		public float Value1
		{
			get { return (float)GetValue( Value1Property ); }
			set { SetValue( Value1Property, value ); }
		}

		/// <summary>The angle in degrees where the first sweep ends and second begins</summary>
		public float Value2
		{
			get { return (float)GetValue( Value2Property ); }
			set { SetValue( Value2Property, value ); }
		}

		/// <summary>The text to show in the primary label</summary>
		public string PrimeLabel
		{
			get { return (string)GetValue( PrimeLabelProperty ); }
			set { SetValue( PrimeLabelProperty, value ); }
		}

		/// <summary>The text to show in the secondary label</summary>
		public string SubLabel
		{
			get { return (string)GetValue( SubLabelProperty ); }
			set { SetValue( SubLabelProperty, value ); }
		}

		/// <summary>Primary Label Text Color</summary>
		public Color PrimaryTextColor
		{
			get { return (Color)GetValue( PrimaryTextColorProperty ); }
			set { SetValue( PrimaryTextColorProperty, value ); }
		}

		/// <summary>Sub label text color</summary>
		public Color SubTextColor
		{
			get { return (Color)GetValue( SecondaryTextColorProperty ); }
			set { SetValue( SecondaryTextColorProperty, value ); }
		}

		/// <summary>What font family to use for the X Labels</summary>
		public string LabelFontFamily
		{
			get { return (string)GetValue( LabelFontFamilyProperty ); }
			set { SetValue( LabelFontFamilyProperty, value ); }
		}

		/// <summary>What font attributes to use for the primary label</summary>
		public FontAttributes PrimeLabelAttributes { get; set; }

		/// <summary>What font attributes to use for the primary label</summary>
		public FontAttributes SubLabelAttributes { get; set; }

		/// <summary>Scales the label front from a % of 100</summary>
		public float PrimeLabelFontScale
		{
			get { return this._primeFontScale; }
			set
			{
				this._primeFontScale = ( value > 0.0F && value <= 100.0F ) ? value : 100.0F;
			}
		}

		/// <summary>Scales the label front from a % of 100</summary>
		public float SubLabelFontScale
		{
			get { return this._subFontScale; }
			set
			{
				this._subFontScale = ( value > 0.0F && value <= 100.0F ) ? value : 100.0F;
			}
		}

		#endregion [ Properties ]

		#region [ Calculated Properties ]

		/// <summary>The width of the arc stroke</summary>
		public float ArcLineWidth { get; set; }

		/// <summary>The top left of the control drawing area</summary>
		protected SKPoint TopLeft { get; set; }

		/// <summary>The outter square in which the circle is inscribed centered</summary>
		protected SKRect OuterRect { get; set; }

		/// <summary>The inner square inscribed inside the ring</summary>
		protected SKRect InscribedRect { get; set; }

		/// <summary>The calculated max width of the prime label area</summary>
		protected float MaxPrimeLabelWidth { get; set; }

		/// <summary>The calculated max width of the prime label area</summary>
		protected float MaxSubLabelWidth { get; set; }

		/// <summary>The origin/center of the graph canvas itself</summary>
		protected SKPoint Origin { get; set; }

		/// <summary>The center x line top to bottom</summary>
		protected float VerticalCenter
		{
			get { return this.Origin.X; }
		}

		/// <summary>Calculated metrics about the primary label</summary>
		internal LabelMetrics PrimeLabelMetrics { get; set; }

		/// <summary>Calculated metrics about the secondary label</summary>
		internal LabelMetrics SubLabelMetrics { get; set; }

		#endregion [ Calculated Properties ]

		#region [ Static Properties ]

		/// <summary></summary>
		//public static readonly BindableProperty ArcLineWidthProperty = BindableProperty.Create(
		//	nameof( ArcLineWidth ), typeof( float ), typeof( ProgressMeter ), 1.0F, BindingMode.TwoWay, null, OnArcLineWidthPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty DisabledColorProperty = BindableProperty.Create(
			nameof( DisabledColor ), typeof( Color ), typeof( ProgressMeter ), Color.LightGray, BindingMode.TwoWay, null, OnDisabledColorPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty ArcColor1Property = BindableProperty.Create(
			nameof( ArcColor1 ), typeof( Color ), typeof( ProgressMeter ), Color.LightGray, BindingMode.TwoWay, null, OnArcColor1PropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty ArcColor2Property = BindableProperty.Create(
			nameof( ArcColor2 ), typeof( Color ), typeof( ProgressMeter ), Color.LightGray, BindingMode.TwoWay, null, OnArcColor2PropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty PrimaryTextColorProperty = BindableProperty.Create(
			nameof( PrimaryTextColor ), typeof( Color ), typeof( ProgressMeter ), Color.Black, BindingMode.TwoWay, null, OnPrimaryTextColorChanged );

		/// <summary></summary>
		public static readonly BindableProperty SecondaryTextColorProperty = BindableProperty.Create(
			nameof( SubTextColor ), typeof( Color ), typeof( ProgressMeter ), Color.Black, BindingMode.TwoWay, null, OnSecondaryTextColorChanged );

		/// <summary>the angles</summary>
		public static readonly BindableProperty Value1Property = BindableProperty.Create(
			nameof( Value1 ), typeof( float ), typeof( ProgressMeter ), 0.0F, BindingMode.TwoWay, null, OnValue1Changed );

		/// <summary>2nd angle</summary>
		public static readonly BindableProperty Value2Property = BindableProperty.Create(
			nameof( Value2 ), typeof( float ), typeof( ProgressMeter ), 360.0F, BindingMode.TwoWay, null, OnValue2Changed );

		/// <summary></summary>
		public static readonly BindableProperty PrimeLabelProperty = BindableProperty.Create(
			nameof( PrimeLabel ), typeof( string ), typeof( ProgressMeter ), String.Empty, BindingMode.TwoWay, null, OnPrimeLabelChanged );

		/// <summary></summary>
		public static readonly BindableProperty SubLabelProperty = BindableProperty.Create(
			nameof( SubLabel ), typeof( string ), typeof( ProgressMeter ), String.Empty, BindingMode.TwoWay, null, OnSubLabelChanged );

		/// <summary></summary>
		public static readonly BindableProperty LabelFontFamilyProperty = BindableProperty.Create(
			nameof( LabelFontFamily ), typeof( string ), typeof( ProgressMeter ), String.Empty, BindingMode.TwoWay, null, OnLabelFontFamilyChanged );

		#endregion [ Static Properties ]

		#region [ Property Handlers ]

		/// <summary></summary>
		//public static void OnArcLineWidthPropertyChanged( BindableObject sender, object oldValue, object newValue )
		//{
		//	var control = sender as ProgressMeter;
		//	control.ArcLineWidth = Convert.ToSingle( newValue );
		//	control.MasterCanvas.InvalidateSurface();
		//}

		/// <summary></summary>
		public static void OnSubLabelChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.SubLabel = newValue.ToString();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnPrimeLabelChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.PrimeLabel = newValue.ToString();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnDisabledColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.DisabledColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnArcColor1PropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.ArcColor1 = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnArcColor2PropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.ArcColor2 = (Color)newValue;
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnPrimaryTextColorChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.PrimaryTextColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnSecondaryTextColorChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.SubTextColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnValue1Changed( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.Value1 = Convert.ToSingle( newValue );
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnValue2Changed( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.Value2 = Convert.ToSingle( newValue );
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnLabelFontFamilyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.LabelFontFamily = newValue.ToString();
			control.MasterCanvas.InvalidateSurface();
		}

		#endregion [ Property Handlers ]

		#region [ Methods ]

		/// <summary>The main canvas paint handler</summary>
		private void OnCanvasViewPaintSurface( object sender, SKPaintSurfaceEventArgs args )
		{
			SKImageInfo info = args.Info;
			SKCanvas canvas = args.Surface.Canvas;

			// clear the canvas
			canvas.Clear();

			// do major calculations
			this.CalcFrame( info, canvas );

			// draw the layout helper grid
			if ( this.LayoutGrid )
				this.DrawLayout( info, canvas );

			if ( this.Mode == MeterStyle.Progress )
				this.PaintProgress( info, canvas );
			else
				this.PaintPie( info, canvas, false );

			this.DrawLabels( info, canvas );
		}

		/// <summary>Calculate major anchor points</summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void CalcFrame( SKImageInfo info, SKCanvas canvas )
		{
			// the vertical center line
			float vertCenter = Convert.ToSingle( info.Width / 2.0 );

			// length to use for outer square - shortest of vertical vs horizontal
			int edgeLen = new int[] { info.Width, info.Height }.Min();

			// 1/2 the distance
			float halfEdge = edgeLen / 2.0F;

			// set the arc line thickness
			this.ArcLineWidth = ( edgeLen / 20.0F ) * this.ArcLineThickness;

			// inset border of the graph frame based on the arc line width
			float border = this.ArcLineWidth * BorderFactor;

			// center of the drawing area
			this.Origin = new SKPoint( vertCenter, info.Height / 2.0F );

			// the square to draw the ring inside of
			this.OuterRect = new SKRect( this.Origin.X - halfEdge + border, this.Origin.Y - halfEdge + border, this.Origin.X + halfEdge - 1 - border, this.Origin.Y + halfEdge - 1 - border );

			// the square inscribed inside the circle
			this.InscribedRect = QuickCalc.InscribeSquare( this.Origin, halfEdge - border );

            // calculate the font for the labels, the same width is used for both which could cause issues
			this.PrimeLabelMetrics = this.CalculateLabels( this.PrimeLabel, this.PrimeLabelAttributes, this.PrimeLabelFontScale );
			this.SubLabelMetrics = this.CalculateLabels( this.SubLabel, this.SubLabelAttributes, this.SubLabelFontScale );
		}

		/// <summary>Draw a progress meter</summary>
		/// <remarks>
        /// Value 1 is the part and Value 2 is the total possible
        /// Value 1 can exceed value 2 for mutiple revolutions
		/// </remarks>
		private void PaintProgress(SKImageInfo info, SKCanvas canvas)
		{
			// set up the gradient colors
			SKColor[] colors = new SKColor[2] { this.ArcColor1.ToSKColor(), this.ArcColor2.ToSKColor() };

			// sweep is in DEG -> normalize the sweep angle between 0 and 360
			//float sweep = QuickCalc.Revolution( ( this.Value1 / this.Value2 ) * 360.0F );
			//sweep = ( sweep > 360.0 ) ? 360.0F : sweep;
			float sweep = ( this.Value1 % this.Value2 / this.Value2 ) * 360.0F;
			
			// we have to roate the drawing canvas 90 degrees CCW
			canvas.RotateDegrees( -90, info.Width / 2, info.Height / 2 );

			// no value
			if (this.Value1 <= 0.0)
            {
				// draw background ring
				using (SKPath path = new SKPath())
				{
					using (SKPaint bkgPaint = new SKPaint() { Color = _disabled, StrokeWidth = this.ArcLineWidth, Style = SKPaintStyle.Stroke, IsAntialias = true })
					{
						path.AddArc( this.OuterRect, 0, 360.0F );
						canvas.DrawPath( path, bkgPaint );
					}
				}
			}
			// less than 1 revolution
			else if (this.Value1 < this.Value2)
			{
				// draw background ring
				using (SKPath path = new SKPath())
				{
					using (SKPaint bkgPaint = new SKPaint() { Color = _disabled, StrokeWidth = this.ArcLineWidth, Style = SKPaintStyle.Stroke, IsAntialias = true })
					{
						path.AddArc( this.OuterRect, 0, 360.0F );
						canvas.DrawPath( path, bkgPaint );
					}
				}

				// draw the partial arc
				using (SKPath path = new SKPath())
				{
					using (SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ArcColor1.ToSKColor() })
					{
						arcPaint.StrokeWidth = this.ArcLineWidth;
						arcPaint.StrokeCap = SKStrokeCap.Butt;
						arcPaint.IsAntialias = true;
						arcPaint.Shader = SKShader.CreateSweepGradient( this.Origin, colors, new Single[] { 0, 1 }, SKShaderTileMode.Clamp, 0, sweep );

						// create an arc to sweep along
						path.AddArc( this.OuterRect, 0, sweep );
						canvas.DrawPath( path, arcPaint );
					}
				}
			}
			else // 1 or more revolution
            {
				// draw background ring
				using (SKPath path = new SKPath())
				{
					using ( SKPaint bkgPaint = new SKPaint() { Color = this.ArcColor1.ToSKColor(), StrokeWidth = this.ArcLineWidth } )
					{
						bkgPaint.Style = SKPaintStyle.Stroke;
						bkgPaint.IsAntialias = true;

						path.AddArc( this.OuterRect, 0, 360.0F );
						canvas.DrawPath( path, bkgPaint );
					}
				}

				// rotate the canvas by the sweep angle so we always start at 0
				canvas.RotateDegrees( sweep-180, info.Width / 2, info.Height / 2 );

				// draw the partial gradiant arc
				using (SKPath path = new SKPath())
				{
					using (SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke })
					{
						arcPaint.Color = this.ArcColor2.ToSKColor();
						arcPaint.StrokeWidth = this.ArcLineWidth;
						arcPaint.StrokeCap = SKStrokeCap.Butt;
						arcPaint.IsAntialias = true;

						// sweep gradient uses start angle to end angle
						arcPaint.Shader = SKShader.CreateSweepGradient( this.Origin, colors, new Single[] { 0, 1 }, SKShaderTileMode.Clamp, 0, 180 );

						// create an arc to sweep along - uses start angle and then how many degrees to rotate from start
						path.AddArc( this.OuterRect, 0, 180 );
						canvas.DrawPath( path, arcPaint );
					}
				}

				canvas.RotateDegrees( -(sweep - 180), info.Width / 2, info.Height / 2 );
			}

			// calc pts for the trailing handle
			Tuple<float, float> pt1 = QuickCalc.Transform( QuickCalc.Deg2Rad( sweep + (this.ArcLineWidth * 0.075F) ), this.OuterRect.Width / 2.0F );
			Tuple<float, float> pt2 = QuickCalc.Transform( QuickCalc.Deg2Rad( sweep ), this.OuterRect.Width / 2.0F );

			// draw the trailing point with shadow
			using (SKPaint handlePaint = new SKPaint() { Color = this.BackgroundColor.ToSKColor() })
			{
				handlePaint.StrokeCap = SKStrokeCap.Round;
				handlePaint.StrokeWidth = this.ArcLineWidth * 2;
				handlePaint.IsAntialias = true;

				// shadow
				canvas.DrawPoint( pt1.Item1 + info.Rect.MidX, info.Rect.MidY + pt1.Item2, handlePaint );

				// change color
				handlePaint.Color = this.ArcColor2.ToSKColor();

				// handle
				canvas.DrawPoint( pt2.Item1 + info.Rect.MidX, info.Rect.MidY + pt2.Item2, handlePaint );
			}

			// rotate it all back
			canvas.RotateDegrees( 90, info.Width / 2, info.Height / 2 );
        }

		/// <summary>Draw a pie meter</summary>
		private void PaintPie( SKImageInfo info, SKCanvas canvas, bool handles = false )
		{
			float arcWidth = ( info.Width / 15.0F ) * 1.0F;

			// always draw a background ring
			using ( SKPath path = new SKPath() )
			{
				path.AddArc( this.OuterRect, 0, 360 );
				canvas.DrawPath( path, new SKPaint() { Color = _disabled, StrokeWidth = this.ArcLineWidth, Style = SKPaintStyle.Stroke, IsAntialias = true } );
			}

			// calc the sum
			float total = this.Value1 + this.Value2;

			// bail if both are 0
			if ( total <= 0.0 )
				return;

			// calc the sweep angle
			float sweep = (this.Value1 / total) * 360.0F;

			// calc the angles, 0 is to the right, start from overhead
			float rewardSweep = (sweep > 360.0) ? 360.0F : sweep;
			float bonusStartAngle = QuickCalc.Revolution( 270.0F + rewardSweep );

			// draw arc 1
			using ( SKPath path = new SKPath() )
			{
				using ( SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ArcColor1.ToSKColor(), StrokeWidth = this.ArcLineWidth, IsAntialias = true } )
				{
					path.AddArc( this.OuterRect, 270, rewardSweep );
					canvas.DrawPath( path, arcPaint );
				}
			}

			// draw arc 2
			using ( SKPath path = new SKPath() )
			{
				using ( SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ArcColor2.ToSKColor(), StrokeWidth = this.ArcLineWidth, IsAntialias = true } )
				{
					path.AddArc( this.OuterRect, bonusStartAngle, 360 - rewardSweep );
					canvas.DrawPath( path, arcPaint );
				}
			}

			if ( !handles )
				return;

			// draw handles
			using ( SKPaint handlePaint = new SKPaint() { Color = this.ArcColor1.ToSKColor(), StrokeCap = SKStrokeCap.Round, StrokeWidth = this.ArcLineWidth * 2, IsAntialias = true } )
			{
				canvas.DrawPoint( this.VerticalCenter, this.OuterRect.Top, handlePaint );

				// where's the other handle?
				float rad = QuickCalc.Deg2Rad( QuickCalc.Revolution( 360 - rewardSweep + 90 ) );
				Tuple<float, float> pt = QuickCalc.Transform( rad, this.OuterRect.Width / 2.0F );

				handlePaint.Color = this.ArcColor2.ToSKColor();
				canvas.DrawPoint( pt.Item1 + info.Rect.MidX, info.Rect.MidY - pt.Item2, handlePaint );
			}
			
		}

		/// <summary>Draws the X axis labels</summary>
		private void DrawLabels( SKImageInfo info, SKCanvas canvas )
		{
			// draw prime label
			if ( !String.IsNullOrWhiteSpace(this.PrimeLabel) )
			{
				using ( SKPaint textPaint = new SKPaint() { Color = this.PrimaryTextColor.ToSKColor() } )
				{
					// set the font family and style
					this.SetFont( textPaint, this.PrimeLabelMetrics.FontSize, this.PrimeLabelAttributes );

					textPaint.TextAlign = SKTextAlign.Center;

					SKRect rect = new SKRect(this.Origin.X - this.MaxPrimeLabelWidth / 2.0F, this.Origin.Y - (this.PrimeLabelMetrics.Height / 2.0F),
                        this.Origin.X + this.MaxPrimeLabelWidth / 2.0F, this.Origin.Y + (this.PrimeLabelMetrics.Height / 2.0F) );

					// show the label rect
                    if ( this.LayoutGrid )
						canvas.DrawRect( rect, new SKPaint() { Color = _orange, StrokeWidth = 1, Style = SKPaintStyle.Stroke } );

                    // draw the text centered on X
					canvas.DrawText( this.PrimeLabel, rect.MidX, rect.Bottom - this.PrimeLabelMetrics.Descent, textPaint );
				}
			}

			// draw sub label
			if ( !String.IsNullOrWhiteSpace( this.SubLabel ) )
			{
				using ( SKPaint textPaint = new SKPaint() { Color = this.SubTextColor.ToSKColor() } )
				{
					textPaint.TextAlign = SKTextAlign.Center;

					// set the font family and style
					this.SetFont( textPaint, this.SubLabelMetrics.FontSize, this.SubLabelAttributes );

					// set below the prime label
					SKRect rect = new SKRect( this.Origin.X - this.MaxPrimeLabelWidth / 2.0F, this.Origin.Y + (this.PrimeLabelMetrics.Height / 2.0F),
                        this.Origin.X + this.MaxPrimeLabelWidth / 2.0F, this.Origin.Y + (this.PrimeLabelMetrics.Height / 2.0F) + this.SubLabelMetrics.Height );

                    if ( this.LayoutGrid )
						canvas.DrawRect( rect, new SKPaint() { Color = _orange, StrokeWidth = 1, Style = SKPaintStyle.Stroke } );

                    canvas.DrawText( this.SubLabel, rect.MidX, rect.Bottom - this.SubLabelMetrics.Descent, textPaint );
				}
			}

		}

		/// <summary>Draws the test layout area</summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void DrawLayout( SKImageInfo info, SKCanvas canvas )
		{
			SKPaint debugPaint = new SKPaint() { Color = _test, StrokeWidth = 1, Style = SKPaintStyle.Stroke };

			// draw origin and control area frame
			canvas.DrawPoint( this.Origin, new SKPaint() { Color = _test, StrokeWidth = 20, Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round } );
			canvas.DrawRect( this.OuterRect, debugPaint );
			canvas.DrawRect( this.InscribedRect, debugPaint );

			// frame the entire canvas
			canvas.DrawRect(0,0,info.Width-1,info.Height-1, new SKPaint() { Color = _test, StrokeWidth = 1, Style = SKPaintStyle.Stroke } );

			// draw cross hairs
			canvas.DrawLine( this.Origin.X, 0, this.Origin.X, info.Height, debugPaint );
			canvas.DrawLine( 0, this.Origin.Y, info.Width, this.Origin.Y, debugPaint );
		}

		/// <summary>Calculates the needed font size for the X labels</summary>
		/// <returns></returns>
		/// <remarks>Right now only calculates the prime label text box</remarks>
		internal LabelMetrics CalculateLabels( string label, FontAttributes attr, float fontScale = 100.0F )
		{
			// if the label is empty
			if (String.IsNullOrEmpty( label ))
				return LabelMetrics.Default();

			if (fontScale < 1)
				fontScale = 1.0F;

			float maxHeight = 0.0F;
			float fontSize = 250.0F;
			float desc = 0.0F;

			// set the max label width to some default
			// use the inscribed square width
			//this.MaxPrimeLabelWidth = this.OuterRect.Width - (this.ArcLineWidth * BorderFactor * 2.0F);
			this.MaxPrimeLabelWidth = this.InscribedRect.Width;

			// determine the ideal font
			using (SKPaint textPaint = new SKPaint())
			{
				// init the font with all its properties
				this.SetFont( textPaint, fontSize, attr );

				// find the min font size for the width
				LabelMetrics widthMet = QuickCalc.CalcSizeForWidth( textPaint, this.MaxPrimeLabelWidth, label );
				textPaint.TextSize = widthMet.FontSize;

				// get the NEW font metrics which includes max height
				SKFontMetrics metrics;
				textPaint.GetFontMetrics( out metrics );

				// set the calculated values thus far
				desc = metrics.Descent;
				maxHeight = metrics.Descent - metrics.Ascent;

				// for fun calculate the width of the circle at the top of the text
				//float chord = Convert.ToSingle( QuickCalc.CalcChord( this.OuterRect.Width / 2.0F, maxHeight / 2.0F ) );
				//this.MaxPrimeLabelWidth = chord - ( this.ArcLineWidth * BorderFactor * 2.0F );

				// now check its not too tall
				if ( maxHeight > this.InscribedRect.Height )
                {
					// scale the font further based on height
					float scale = this.InscribedRect.Height / maxHeight;
					textPaint.TextSize = textPaint.TextSize * scale;
				}

				// now scale by the relative user set scale
				if ( fontScale < 100.0F )
					textPaint.TextSize = textPaint.TextSize * ( fontScale / 100.0F );

                // remeasure after user set scaling
                textPaint.GetFontMetrics( out metrics );
				fontSize = textPaint.TextSize;
				desc = metrics.Descent;
				maxHeight = metrics.Descent - metrics.Ascent;
			}

			return new LabelMetrics( fontSize, maxHeight, desc );
		}

		/// <summary>Sets up the SK Font attributes including family and bold</summary>
		/// <param name="textPaint"></param>
		internal void SetFont( SKPaint textPaint, float fontSize, FontAttributes weight )
		{
			textPaint.TextSize = fontSize;
			textPaint.IsAntialias = true;
			textPaint.IsStroke = false;

			if ( (weight & FontAttributes.Bold) == FontAttributes.Bold )
				textPaint.Typeface = SKTypeface.FromFamilyName( this.LabelFontFamily, SKFontStyle.Bold );
			else
				textPaint.Typeface = SKTypeface.FromFamilyName( this.LabelFontFamily, SKFontStyle.Normal );

			// FromFile
		}

		#endregion [ Methods ]

	}
}


///// <summary>Draw a progress meter</summary>
///// <remarks>Value 2 is the total in Progress</remarks>
//private void PaintProgressold(SKImageInfo info, SKCanvas canvas)
//{
//	// always draw a background ring
//	using (SKPath path = new SKPath())
//	{
//		path.AddArc( this.OuterRect, 0, 360 );
//		canvas.DrawPath( path, new SKPaint() { Color = _disabled, StrokeWidth = this.ArcLineWidth, Style = SKPaintStyle.Stroke, IsAntialias = true } );
//	}

//	// if 0 no need to continue
//	if (this.Value2 <= 0.0)
//	{
//		// draw starting handle
//		using (SKPaint handlePaint = new SKPaint() { Color = this.ArcColor1.ToSKColor() })
//		{
//			handlePaint.StrokeCap = SKStrokeCap.Round;
//			handlePaint.StrokeWidth = this.ArcLineWidth * 2;
//			handlePaint.IsAntialias = true;

//			canvas.DrawPoint( this.VerticalCenter, this.OuterRect.Top, handlePaint );
//		}
//		return;
//	}

//	// set up the gradient colors
//	//SKColor[] colors = new SKColor[2] { this.ArcColor1.ToSKColor(), this.ArcColor2.ToSKColor() };
//	SKColor[] colors = new SKColor[2] { SKColor.Parse( "FF0000" ), SKColor.Parse( "0000FF" ) };

//	// calc the sweep angle - calc the angles, 0 is to the right, start from overhead
//	float sweep = ( this.Value1 / this.Value2 ) * 360.0F;
//	sweep = ( sweep > 360.0 ) ? 360.0F : sweep;

//	// calculate the end angle in deg from top
//	float endAngle = QuickCalc.Deg2Rad( QuickCalc.Revolution( 360 - sweep + 90 ) );

//	// draw the arc
//	using (SKPath path = new SKPath())
//	{
//		using (SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ArcColor1.ToSKColor() })
//		{
//			arcPaint.StrokeWidth = this.ArcLineWidth;
//			arcPaint.StrokeCap = SKStrokeCap.Round;
//			arcPaint.IsAntialias = true;
//			//arcPaint.Shader = SKShader.CreateSweepGradient( this.Origin, colors, new Single[] {0, 1}, SKShaderTileMode.Clamp, -90, 270 );
//			//arcPaint.Shader = SKShader.CreateSweepGradient( this.Origin, colors );

//			// create an arc to sweep along
//			path.AddArc( this.OuterRect, 270, sweep );
//			canvas.DrawPath( path, arcPaint );
//		}
//	}

//	// draw starting handle
//	using (SKPaint handlePaint = new SKPaint() { Color = this.ArcColor1.ToSKColor() })
//	{
//		handlePaint.StrokeCap = SKStrokeCap.Round;
//		handlePaint.StrokeWidth = this.ArcLineWidth * 2;
//		handlePaint.IsAntialias = true;

//		canvas.DrawPoint( this.VerticalCenter, this.OuterRect.Top, handlePaint );
//	}

//	// calc the other handle in caretesian points
//	Tuple<float, float> pt = QuickCalc.Transform( endAngle, this.OuterRect.Width / 2.0F );

//	// draw trailing point
//	using (SKPaint handlePaint = new SKPaint() { Color = this.ArcColor2.ToSKColor() })
//	{
//		handlePaint.StrokeCap = SKStrokeCap.Round;
//		handlePaint.StrokeWidth = this.ArcLineWidth * 2;
//		handlePaint.IsAntialias = true;

//		canvas.DrawPoint( pt.Item1 + info.Rect.MidX, info.Rect.MidY - pt.Item2, handlePaint );
//	}
//}
