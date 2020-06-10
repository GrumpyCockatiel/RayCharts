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

		/// <summary>default font sizes</summary>
		private double _primeFontSize = 10.0;
		private double _secFontSize = 10.0;

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

			//this.InitializeComponent();

			this.PrimeLabel = String.Empty;
			this.SubLabel = String.Empty;
		}

		#endregion [ Constructors ]

		#region [ Properties ]

		/// <summary>the top level canvas to draw on</summary>
		public SKCanvasView MasterCanvas { get; set; }

		/// <summary>What style to draw the control in</summary>
		public MeterStyle Mode { get; set; }

		/// <summary>The width of the arc stroke</summary>
		public float ArcLineWidth { get; set; }

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

		/// <summary></summary>
		public double PrimeLabelFontSize
        {
            get { return this._primeFontSize; }
            set
            {
                this._primeFontSize = (value < 1.0) ? 10.0 : value;
            }
        }

        /// <summary></summary>
        public double SubLabelFontSize
		{
			get { return this._secFontSize; }
			set
			{
				this._secFontSize = (value < 1.0) ? 10.0 : value;
			}
		}

		///// <summary>Scales the label front from a % of 100</summary>
		//public float PrimeLabelFontScale
		//{
		//	get { return this._primeFontScale; }
		//	set
		//	{
		//		this._primeFontScale = (value > 0.0F && value < 100.0F) ? value : 100.0F;
		//	}
		//}

		///// <summary>Scales the label front from a % of 100</summary>
		//public float SubLabelFontScale
		//{
		//	get { return this._secFontScale; }
		//	set
		//	{
		//		this._secFontScale = (value > 0.0F && value < 100.0F) ? value : 100.0F;
		//	}
		//}

		#endregion [ Properties ]

		#region [ Calculated Properties ]

		/// <summary>The top left of the control drawing area</summary>
		protected SKPoint TopLeft { get; set; }

		/// <summary>The square in which the circle is inscribed centered</summary>
		protected SKRect GraphFrame { get; set; }

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

		/// <summary></summary>
		public static readonly BindableProperty Value1Property = BindableProperty.Create(
			nameof( Value1 ), typeof( float ), typeof( ProgressMeter ), 0.0F, BindingMode.TwoWay, null, OnValue1Changed );

		/// <summary></summary>
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
		public static void OnSubLabelChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.SubLabel = newValue.ToString();
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnPrimeLabelChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.PrimeLabel = newValue.ToString();
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnDisabledColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.DisabledColor = (Color)newValue;
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnArcColor1PropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.ArcColor1 = (Color)newValue;
			//control.meter.InvalidateSurface();
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
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnSecondaryTextColorChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.SubTextColor = (Color)newValue;
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnValue1Changed( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.Value1 = Convert.ToSingle( newValue );
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnValue2Changed( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.Value2 = Convert.ToSingle( newValue );
			//control.meter.InvalidateSurface();
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnLabelFontFamilyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressMeter;
			control.LabelFontFamily = newValue.ToString();
			//control.meter.InvalidateSurface();
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
				this.PaintPie( info, canvas );

			this.DrawLabels( info, canvas );
		}

		/// <summary>Calculate major anchor points</summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void CalcFrame( SKImageInfo info, SKCanvas canvas )
		{
			// the vertical center line
			float vertCenter = Convert.ToSingle( info.Width / 2.0 );

			// shortest of vertical vs horizontal
			int edgeLen = new int[] { info.Width, info.Height }.Min();

			// 1/2 the distance
			float halfEdge = edgeLen / 2.0F;

			// inset border of the graph frame based on the arc line width
			float border = this.ArcLineWidth * BorderFactor;

			// center of the drawing area
			this.Origin = new SKPoint( vertCenter, info.Height / 2.0F );

			// the square to draw the ring inside of
			this.GraphFrame = new SKRect( this.Origin.X - halfEdge + border, this.Origin.Y - halfEdge + border, this.Origin.X + halfEdge - 1 - border, this.Origin.Y + halfEdge - 1 - border );

            // calculate the font for the labels, the same width is used for both which could cause issues
			this.PrimeLabelMetrics = this.CalculateLabels( this.PrimeLabel, (float)this.PrimeLabelFontSize );
			this.SubLabelMetrics = this.CalculateLabels( this.SubLabel, (float)this.SubLabelFontSize );
		}

		/// <summary>Draw a progress meter</summary>
		/// <remarks>Value 2 is the total in Progress</remarks>
		private void PaintProgress(SKImageInfo info, SKCanvas canvas )
		{
			// always draw a background ring
			using ( SKPath path = new SKPath() )
			{
				path.AddArc( this.GraphFrame, 0, 360 );
				canvas.DrawPath( path, new SKPaint() { Color = _disabled, StrokeWidth = this.ArcLineWidth, Style = SKPaintStyle.Stroke, IsAntialias = true } );
			}

			// if 0 no need to continue
			if ( this.Value2 <= 0.0 )
			{
				// draw starting handle
				using ( SKPaint handlePaint = new SKPaint() { Color = this.ArcColor1.ToSKColor(), StrokeCap = SKStrokeCap.Round, StrokeWidth = this.ArcLineWidth * 2, IsAntialias = true } )
				{
					canvas.DrawPoint( this.VerticalCenter, this.GraphFrame.Top, handlePaint );
				}
				return;
			}

			// calc the sweep angle
			float sweep = (this.Value1 / this.Value2) * 360.0F;

			// calc the angles, 0 is to the right, start from overhead
			sweep = (sweep > 360.0) ? 360.0F : sweep;

			// draw the arc
			using ( SKPath path = new SKPath() )
			{
				using ( SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ArcColor1.ToSKColor(), StrokeWidth = this.ArcLineWidth, StrokeCap = SKStrokeCap.Round, IsAntialias = true } )
				{
					path.AddArc( this.GraphFrame, 270, sweep );
					canvas.DrawPath( path, arcPaint );
				}
			}

			// draw starting handle
			using ( SKPaint handlePaint = new SKPaint() { Color = this.ArcColor1.ToSKColor(), StrokeCap = SKStrokeCap.Round, StrokeWidth = this.ArcLineWidth * 2, IsAntialias = true } )
			{
				canvas.DrawPoint( this.VerticalCenter, this.GraphFrame.Top, handlePaint );
			}

			// where's the other handle?
			float rad = QuickCalc.Deg2Rad( QuickCalc.Revolution( 360 - sweep + 90 ) );
			Tuple<float, float> pt = QuickCalc.Transform( rad, this.GraphFrame.Width / 2.0F );

			// draw trailing point
			using ( SKPaint handlePaint = new SKPaint() { Color = this.ArcColor2.ToSKColor(), StrokeCap = SKStrokeCap.Round, StrokeWidth = this.ArcLineWidth * 2, IsAntialias = true } )
			{
				canvas.DrawPoint( pt.Item1 + info.Rect.MidX, info.Rect.MidY - pt.Item2, handlePaint );
			}

		}

		/// <summary>Draw a pie meter</summary>
		private void PaintPie( SKImageInfo info, SKCanvas canvas, bool handles = false )
		{
			// always draw a background ring
			using ( SKPath path = new SKPath() )
			{
				path.AddArc( this.GraphFrame, 0, 360 );
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
					path.AddArc( this.GraphFrame, 270, rewardSweep );
					canvas.DrawPath( path, arcPaint );
				}
			}

			// draw arc 2
			using ( SKPath path = new SKPath() )
			{
				using ( SKPaint arcPaint = new SKPaint { Style = SKPaintStyle.Stroke, Color = this.ArcColor2.ToSKColor(), StrokeWidth = this.ArcLineWidth, IsAntialias = true } )
				{
					path.AddArc( this.GraphFrame, bonusStartAngle, 360 - rewardSweep );
					canvas.DrawPath( path, arcPaint );
				}
			}

			if ( !handles )
				return;

			// draw handles
			using ( SKPaint handlePaint = new SKPaint() { Color = this.ArcColor1.ToSKColor(), StrokeCap = SKStrokeCap.Round, StrokeWidth = this.ArcLineWidth * 2, IsAntialias = true } )
			{
				canvas.DrawPoint( this.VerticalCenter, this.GraphFrame.Top, handlePaint );

				// where's the other handle?
				float rad = QuickCalc.Deg2Rad( QuickCalc.Revolution( 360 - rewardSweep + 90 ) );
				Tuple<float, float> pt = QuickCalc.Transform( rad, this.GraphFrame.Width / 2.0F );

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
			canvas.DrawRect( this.GraphFrame, debugPaint );

			// frame the entire canvas
			canvas.DrawRect(0,0,info.Width-1,info.Height-1, new SKPaint() { Color = _test, StrokeWidth = 1, Style = SKPaintStyle.Stroke } );

			// draw cross hairs
			canvas.DrawLine( this.Origin.X, 0, this.Origin.X, info.Height, debugPaint );
			canvas.DrawLine( 0, this.Origin.Y, info.Width, this.Origin.Y, debugPaint );
		}

		/// <summary>Calculates the needed font size for the X labels</summary>
		/// <returns></returns>
		/// <remarks>Right now only calculates the prime label text box</remarks>
		internal LabelMetrics CalculateLabels( string label, float startFontSize = 250.0F )
		{
			float minScale = 1000.0F;
			float maxHeight = 0.0F;
			float fontSize = startFontSize;
			float desc = 0.0F;

			// set the max label width to some default
			this.MaxPrimeLabelWidth = this.GraphFrame.Width - (this.ArcLineWidth * BorderFactor * 2.0F);

			// scale only based on height
			using ( SKPaint textPaint = new SKPaint() )
			{
				// set the font
				this.SetFont( textPaint, fontSize, this.PrimeLabelAttributes );
				
				// measure it
				SKRect textBounds = new SKRect();
				textPaint.MeasureText( label, ref textBounds );

                // get the font metrics which includes max height
				SKFontMetrics metrics;
				textPaint.GetFontMetrics( out metrics );

				float chord = Convert.ToSingle( QuickCalc.CalcChord(this.GraphFrame.Width / 2.0F, (metrics.Descent - metrics.Ascent) / 2.0F ) );
				this.MaxPrimeLabelWidth = chord - (this.ArcLineWidth * BorderFactor * 2.0F);

				// we only care the width that fits in the arc
				// float scaley = (this.GraphFrame.Height / 4.0F) / (metrics.Descent - metrics.Ascent);
				if ( textBounds.Width > this.MaxPrimeLabelWidth )
                {
					float scalex = this.MaxPrimeLabelWidth / textBounds.Width;
					minScale = new float[] { minScale, scalex }.Min();
					fontSize = textPaint.TextSize * minScale;
				}
			}

			// use the new font size to re-measure
			using ( SKPaint textPaint = new SKPaint() { TextSize = fontSize, IsAntialias = true, IsStroke = false } )
			{
				textPaint.Typeface = SKTypeface.FromFamilyName( this.LabelFontFamily );

				SKFontMetrics metrics;
				textPaint.GetFontMetrics( out metrics );

				if ( metrics.Descent > desc )
					desc = metrics.Descent;

				// sets the distance from Ascent to Descent
				if ( metrics.Descent - metrics.Ascent > maxHeight )
					maxHeight = metrics.Descent - metrics.Ascent;

			}

			return new LabelMetrics( fontSize, maxHeight, desc );
		}

		/// <summary>Sets up the SK Font attributes</summary>
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
