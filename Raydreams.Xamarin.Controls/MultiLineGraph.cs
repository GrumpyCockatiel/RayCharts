using System;
using System.Collections.Generic;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace Raydreams.Xamarin.Controls
{
	/// <summary></summary>
	public partial class MultiLineGraph : ContentView
	{
		private static SKColor _gridColor = new SKColor( 255, 0, 0 );
		private static SKColor _white = new SKColor( 255, 255, 255 );
		private float _xFontScale = 100.0F;
		private float _yFontScale = 100.0F;
		private float _xbreath = 5.0F; // should be equal to the largest series stroke width
		private float _ybreath = 5.0F;

		/// <summary></summary>
		public MultiLineGraph()
		{
			// set the single child view and event handlers
			this.MasterCanvas = new SKCanvasView();
			this.Content = this.MasterCanvas;
			this.MasterCanvas.PaintSurface += OnCanvasViewPaintSurface;

			//InitializeComponent();

			// set default values
			this.XMin = 0;
			this.YMin = 0;
			this.XMax = 10;
			this.YMax = 10;
			this.TitleHeight = 20;
			this.XLabelHeight = 20;
			this.YLabelWidth = 20;
			this.RightMarginWidth = 5;
		}

		#region [ Bindable Properties ]

		/// <summary>the top level canvas to draw on</summary>
		public SKCanvasView MasterCanvas { get; set; }

		/// <summary>Logical graph data</summary>
		public List<LineSeries> Series
		{
			get { return (List<LineSeries>)GetValue( SeriesProperty ); }
			set { SetValue( SeriesProperty, value ); }
		}

		/// <summary>Logical graph data</summary>
		public List<LineLabel> XLabels
		{
			get { return (List<LineLabel>)GetValue(XLabelsProperty); }
			set { SetValue(XLabelsProperty, value); }
		}

		/// <summary>Logical graph data</summary>
		public List<LineLabel> YLabels
		{
			get { return (List<LineLabel>)GetValue(YLabelsProperty); }
			set { SetValue(YLabelsProperty, value); }
		}

		/// <summary>Color to paint the axis</summary>
		public Color AxisColor
		{
			get { return (Color)GetValue( AxisColorProperty ); }
			set { SetValue( AxisColorProperty, value ); }
		}

		/// <summary>Color to paint the gridlines</summary>
		public Color GridlineColor
		{
			get { return (Color)GetValue( GridlineColorProperty ); }
			set { SetValue( GridlineColorProperty, value ); }
		}

		/// <summary>Color to paint the Axis Labels</summary>
		public Color LabelColor
		{
			get { return (Color)GetValue( XLabelColorProperty ); }
			set { SetValue( XLabelColorProperty, value ); }
		}

		/// <summary></summary>
		public float YMin
		{
			get { return (float)GetValue( YMinProperty ); }
			set { SetValue( YMinProperty, value ); }
		}

		/// <summary></summary>
		public float YMax
		{
			get { return (float)GetValue( YMaxProperty ); }
			set { SetValue( YMaxProperty, value ); }
		}

		/// <summary></summary>
		public float XMin
		{
			get { return (float)GetValue( XMinProperty ); }
			set { SetValue( XMinProperty, value ); }
		}

		/// <summary></summary>
		public float XMax
		{
			get { return (float)GetValue( XMaxProperty ); }
			set { SetValue( XMaxProperty, value ); }
		}

		/// <summary>What font family to use for the X Labels</summary>
		public string LabelFontFamily
		{
			get { return (string)GetValue( LabelFontFamilyProperty ); }
			set { SetValue( LabelFontFamilyProperty, value ); }
		}

		/// <summary>Draw the layout helper grid</summary>
		public bool LayoutGrid { get; set; }

		/// <summary>The area below the graph for X axies labels</summary>
		public float XLabelHeight { get; set; }

		/// <summary>The height of the area about the graph</summary>
		public float TitleHeight { get; set; }

		/// <summary>The left margin width for writing Y axis labels</summary>
		public float YLabelWidth { get; set; }

		/// <summary>The right margin space</summary>
		public float RightMarginWidth { get; set; }

		/// <summary>Scales the label front from a % of 100</summary>
		public float XLabelFontScale
		{
			get { return this._xFontScale; }
			set
			{
				this._xFontScale = (value > 0.0F && value < 100.0F) ? value : 100.0F;
			}
		}

		/// <summary>Scales the label front from a % of 100</summary>
		public float YLabelFontScale
		{
			get { return this._yFontScale; }
			set
			{
				this._yFontScale = (value > 0.0F && value < 100.0F) ? value : 100.0F;
			}
		}

		#endregion [ Bindable Properties ]

		#region [ Calculated Properties ]

		/// <summary>The origin/center of the graph canvas itself</summary>
		protected SKPoint Origin { get; set; }

		/// <summary>The entire graph - lines and labels</summary>
		protected SKRect ControlFrame { get; set; }

		/// <summary>Only the graph itself from the origin and axes</summary>
		protected SKRect GraphFrame { get; set; }

		/// <summary></summary>
		internal LabelMetrics XLabelMetrics { get; set; }

		/// <summary></summary>
		internal LabelMetrics YLabelMetrics { get; set; }

		#endregion [ Calculated Properties ]

		#region [ Static Properties ]

		/// <summary></summary>
		public static readonly BindableProperty SeriesProperty = BindableProperty.Create(
			nameof( Series ), typeof( List<LineSeries> ), typeof( MultiLineGraph ), null, BindingMode.TwoWay, null, OnSeriesPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty XLabelsProperty = BindableProperty.Create(
			nameof(XLabels), typeof(List<LineLabel>), typeof(MultiLineGraph), null, BindingMode.TwoWay, null, OnXLabelsPropertyChanged);

		/// <summary></summary>
		public static readonly BindableProperty YLabelsProperty = BindableProperty.Create(
			nameof(YLabels), typeof(List<LineLabel>), typeof(MultiLineGraph), null, BindingMode.TwoWay, null, OnYLabelsPropertyChanged);

		/// <summary></summary>
		public static readonly BindableProperty AxisColorProperty = BindableProperty.Create(
			nameof( AxisColor ), typeof( Color ), typeof( MultiLineGraph ), Color.White, BindingMode.TwoWay, null, OnAxisColorPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty GridlineColorProperty = BindableProperty.Create(
			nameof( GridlineColor ), typeof( Color ), typeof( MultiLineGraph ), Color.White, BindingMode.TwoWay, null, OnGridlineColorPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty XLabelColorProperty = BindableProperty.Create(
			nameof( LabelColor ), typeof( Color ), typeof( MultiLineGraph ), Color.Black, BindingMode.TwoWay, null, OnLabelColorPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty XMinProperty = BindableProperty.Create(
			nameof( XMin ), typeof( float ), typeof( MultiLineGraph ), 0.0F, BindingMode.TwoWay, null, OnXMinPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty YMinProperty = BindableProperty.Create(
			nameof( YMin ), typeof( float ), typeof( MultiLineGraph ), 0.0F, BindingMode.TwoWay, null, OnYMinPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty XMaxProperty = BindableProperty.Create(
			nameof( XMax ), typeof( float ), typeof( MultiLineGraph ), 1.0F, BindingMode.TwoWay, null, OnXMaxPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty YMaxProperty = BindableProperty.Create(
			nameof( YMax ), typeof( float ), typeof( MultiLineGraph ), 1.0F, BindingMode.TwoWay, null, OnYMaxPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty LabelFontFamilyProperty = BindableProperty.Create(
			nameof( LabelFontFamily ), typeof( string ), typeof( MultiLineGraph ), String.Empty, BindingMode.TwoWay, null, OnLabelFontFamilyChanged );

		#endregion [ Static Properties ]

		#region [ Property Handlers ]

		/// <summary></summary>
		public static void OnSeriesPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.Series = (List<LineSeries>)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnXLabelsPropertyChanged(BindableObject sender, object oldValue, object newValue)
		{
			var control = sender as MultiLineGraph;
			control.XLabels = (List<LineLabel>)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnYLabelsPropertyChanged(BindableObject sender, object oldValue, object newValue)
		{
			var control = sender as MultiLineGraph;
			control.YLabels = (List<LineLabel>)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnAxisColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.AxisColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnGridlineColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.GridlineColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnLabelColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.LabelColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnXMinPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.XMin = (float)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnYMinPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.YMin = (float)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnXMaxPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.XMax = (float)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnYMaxPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.YMax = (float)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnLabelFontFamilyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as MultiLineGraph;
			control.LabelFontFamily = newValue.ToString();
			control.MasterCanvas.InvalidateSurface();
		}

		#endregion [ Property Handlers ]

		/// <summary>Main paint method</summary>
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

			// draw everything
			this.DrawGridlines( info, canvas );
			this.DrawAxis( info, canvas );
			this.DrawGraph( info, canvas );
			this.DrawXLabels(info, canvas);
			this.DrawYLabels( info, canvas );
		}

		/// <summary>Draw the layout grid</summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void DrawLayout( SKImageInfo info, SKCanvas canvas )
		{
			using ( SKPaint debugPaint = new SKPaint() { Color = _gridColor, StrokeWidth = 1, Style = SKPaintStyle.Stroke } )
			{
				// frame the entire canvas
				canvas.DrawRect( this.GraphFrame, debugPaint );
			}
		}

		/// <summary></summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void DrawGraph( SKImageInfo info, SKCanvas canvas )
		{
			if ( this.Series == null )
				return;

			float xf = ( this.GraphFrame.Width / (this.XMax - this.XMin) );
			float yf = ( this.GraphFrame.Height / (this.YMax - this.YMin) );

			// iterate each line series
			foreach ( LineSeries ls in this.Series )
			{
				using ( SKPaint axisPaint = new SKPaint() { Color = ls.LineColor, StrokeWidth = ls.StrokeWidth, Style = SKPaintStyle.Stroke, IsAntialias = true } )
				{
					// normalize the points to the graph frame
					SKPoint[] tps = new SKPoint[ls.Values.Length];

					// calculate all the points
					for ( int i = 0; i < ls.Values.Length; ++i )
					{
						tps[i] = new SKPoint( this.GraphFrame.Left + ((ls.Values[i].X - this.XMin) * xf), this.TitleHeight + this.GraphFrame.Height - ( (ls.Values[i].Y - this.YMin) * yf) );
					}

					if ( tps.Length < 2 )
						continue;

					// draw the lines
					for(int i = 0; i < tps.Length-1; ++i )
					{
						canvas.DrawLine( tps[i], tps[i+1], axisPaint );
					}

					// draw the dots
					using ( SKPaint ptPaint = new SKPaint() { Color = ls.LineColor, StrokeWidth = ls.StrokeWidth * 5.0F, StrokeCap = SKStrokeCap.Round, IsAntialias = true } )
					{
						for ( int i = 0; i < tps.Length; ++i )
						{
							canvas.DrawPoint( tps[i], ptPaint );
						}
					}
				}
			}
		}

		/// <summary>Draws the grid axis</summary>
		private void DrawAxis( SKImageInfo info, SKCanvas canvas, bool vertical = true, bool horizontal = true )
		{
			using ( SKPaint axisPaint = new SKPaint() { Color = this.AxisColor.ToSKColor(), StrokeWidth = 2.0F, Style = SKPaintStyle.Stroke } )
			{
				if ( vertical )
					canvas.DrawLine( this.GraphFrame.Left, this.GraphFrame.Top, this.GraphFrame.Left, this.GraphFrame.Bottom, axisPaint );

				if ( horizontal )
					canvas.DrawLine( this.GraphFrame.Left, this.GraphFrame.Bottom, this.GraphFrame.Right, this.GraphFrame.Bottom, axisPaint );
			}
		}

		/// <summary>Calculate major anchor points</summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void CalcFrame( SKImageInfo info, SKCanvas canvas )
		{
			// center of the drawing area
			this.Origin = new SKPoint( info.Width / 2.0F, info.Height / 2.0F );

			// calc the outer frame
			this.ControlFrame = new SKRect( 0, 0, info.Width - 1, info.Height - 1 );

			this.GraphFrame = new SKRect( this.ControlFrame.Left + this.YLabelWidth, this.ControlFrame.Top + this.TitleHeight,
				this.ControlFrame.Right - this.RightMarginWidth, this.ControlFrame.Bottom - this.XLabelHeight );

			// calc the label metrics
			this.XLabelMetrics = this.CalculateXLabels( this.XLabelHeight - this._xbreath );
			this.YLabelMetrics = this.CalculateYLabels( this.YLabelWidth - this._ybreath );
		}

		/// <summary></summary>
		private void DrawGridlines( SKImageInfo info, SKCanvas canvas )
		{
			float xf = (this.GraphFrame.Width / (this.XMax - this.XMin));
			float yf = (this.GraphFrame.Height / (this.YMax - this.YMin));

			// no data
			if ( this.XLabels != null && this.XLabels.Count > 0 )
			{
				foreach ( LineLabel i in this.XLabels )
				{
					// skip the ends
					if ( !i.Line || i.Location <= this.XMin || i.Location >= this.XMax )
						continue;

					float x = this.GraphFrame.Left + ((i.Location - this.XMin) * xf);

					using ( SKPaint axisPaint = new SKPaint() { Color = this.GridlineColor.ToSKColor(), StrokeWidth = 1.0F, Style = SKPaintStyle.Stroke } )
					{
						canvas.DrawLine( x, this.GraphFrame.Top, x, this.GraphFrame.Bottom, axisPaint );
					}
				}
			}

			// no data
			if ( this.YLabels == null || this.YLabels.Count < 0 )
				return;

			foreach ( LineLabel i in this.YLabels )
			{
				// skip the ends
				if ( !i.Line || i.Location <= this.YMin || i.Location >= this.YMax )
					continue;

				float y = this.GraphFrame.Bottom - ((i.Location - this.YMin) * yf);

				using ( SKPaint axisPaint = new SKPaint() { Color = this.GridlineColor.ToSKColor(), StrokeWidth = 1.0F, Style = SKPaintStyle.Stroke } )
				{
					canvas.DrawLine( this.GraphFrame.Left, y, this.GraphFrame.Right, y, axisPaint );
				}
			}

		}

		/// <summary></summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void DrawXLabels(SKImageInfo info, SKCanvas canvas)
		{
			// no data
			if (this.XLabels == null || this.XLabels.Count < 0)
				return;

			// x transform factor
			float xf = (this.GraphFrame.Width / (this.XMax - this.XMin));

			using (SKPaint textPaint = new SKPaint() { Color = this.LabelColor.ToSKColor(), TextSize = this.XLabelMetrics.FontSize })
			{
				textPaint.TextAlign = SKTextAlign.Center;
				textPaint.IsAntialias = true;
				textPaint.IsStroke = false;
				textPaint.Typeface = SKTypeface.FromFamilyName(this.LabelFontFamily);

				foreach (LineLabel i in this.XLabels)
				{
					if ( String.IsNullOrWhiteSpace(i.Text) )
						continue;

					float x = this.GraphFrame.Left + ( (i.Location - this.XMin ) * xf);

					canvas.DrawText(i.Text, x, this.GraphFrame.Bottom + this.XLabelMetrics.Height - this.XLabelMetrics.Descent + this._xbreath, textPaint);
				}
			}
		}

		/// <summary></summary>
		/// <param name="info"></param>
		/// <param name="canvas"></param>
		private void DrawYLabels( SKImageInfo info, SKCanvas canvas )
		{
			// no data
			if ( this.YLabels == null || this.YLabels.Count < 0 )
				return;

			// y scale factor
			float yf = (this.GraphFrame.Height / (this.YMax - this.YMin));

			using ( SKPaint textPaint = new SKPaint() { Color = this.LabelColor.ToSKColor(), TextSize = this.YLabelMetrics.FontSize } )
			{
				textPaint.TextAlign = SKTextAlign.Right;
				textPaint.IsAntialias = true;
				textPaint.IsStroke = false;
				textPaint.Typeface = SKTypeface.FromFamilyName( this.LabelFontFamily );

				foreach ( LineLabel i in this.YLabels )
				{
					if ( String.IsNullOrWhiteSpace( i.Text ) )
						continue;

					float y = this.GraphFrame.Bottom - ( (i.Location - this.YMin) * yf);

					SKRect textBounds = new SKRect();
					textPaint.MeasureText( i.Text, ref textBounds );

					//SKRect rect = new SKRect(x - (textBounds.Width / 2.0F), this.GraphFrame.Bottom, (x + textBounds.Width / 2.0F), this.ControlFrame.Bottom);

					canvas.DrawText( i.Text, this.GraphFrame.Left - this._ybreath, y + textBounds.Height/2, textPaint );
				}
			}
		}

		/// <summary>Calculates the needed font size for the X labels</summary>
		/// <returns></returns>
		internal LabelMetrics CalculateXLabels(float labelHeight, float startFontSize = 100.0F)
		{
			float minScale = 1000.0F;
			float maxHeight = 0.0F;
			float fontSize = startFontSize;
			float desc = 0.0F;

			// scale only based on height
			using (SKPaint textPaint = new SKPaint() { TextSize = fontSize, IsAntialias = true, IsStroke = false })
			{
				textPaint.Typeface = SKTypeface.FromFamilyName(this.LabelFontFamily);

				SKFontMetrics metrics;
				textPaint.GetFontMetrics(out metrics);

				// we only care about the height this time which we get from Descent - Ascent
				float scaley = labelHeight / (metrics.Descent - metrics.Ascent);

				if (scaley < minScale)
					minScale = scaley;

				fontSize = textPaint.TextSize * minScale * (this.XLabelFontScale / 100.0F);
			}

			// use the new font size
			using (SKPaint textPaint = new SKPaint() { TextSize = fontSize, IsAntialias = true, IsStroke = false })
			{
				textPaint.Typeface = SKTypeface.FromFamilyName(this.LabelFontFamily);

				SKFontMetrics metrics;
				textPaint.GetFontMetrics(out metrics);

				if (metrics.Descent > desc)
					desc = metrics.Descent;

				// sets the distance from Ascent to Descent
				if (metrics.Descent - metrics.Ascent > maxHeight)
					maxHeight = metrics.Descent - metrics.Ascent;

			}

			return new LabelMetrics(fontSize, maxHeight, desc);
		}

		/// <summary>Calculates the needed font size for the Y labels</summary>
		/// <returns></returns>
		internal LabelMetrics CalculateYLabels( float labelWidth, float startFontSize = 100.0F )
		{
			float minScale = 1000.0F;
			float maxHeight = 0.0F;
			float fontSize = startFontSize;
			float desc = 0.0F;

			// no data
			if ( this.YLabels == null || this.YLabels.Count < 0 )
				return new LabelMetrics( fontSize, maxHeight, desc );

			// scale only based on width
			using ( SKPaint textPaint = new SKPaint() { TextSize = fontSize, IsAntialias = true, IsStroke = false } )
			{
				textPaint.Typeface = SKTypeface.FromFamilyName( this.LabelFontFamily );

				foreach (LineLabel entry in this.YLabels)
				{
					if ( String.IsNullOrWhiteSpace( entry.Text ) )
						continue;

					SKRect textBounds = new SKRect();
					textPaint.MeasureText( entry.Text, ref textBounds );

					float scale = labelWidth / textBounds.Width;

					if ( scale < minScale )
						minScale = scale;
				}

				// adjust the font
				fontSize = textPaint.TextSize * minScale * (this.YLabelFontScale / 100.0F);
			}

			// use the new font size
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

	}
}
