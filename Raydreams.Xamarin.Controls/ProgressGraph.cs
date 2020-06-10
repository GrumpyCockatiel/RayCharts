using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Linq;

namespace Raydreams.Xamarin.Controls
{
	/// <summary>A Bar chart with 2 bars on top of each other to so a threshold.</summary>
	public partial class ProgressGraph : ContentView
	{
		private static SKColor _gridColor = new SKColor( 255, 0, 0 );
		private static SKColor _white = new SKColor( 255, 255, 255 );

		private float _xFontScale = 100.0F;
		private float _xbreath = 5.0F; // should be equal to the largest series stroke width

		/// <summary>Internal class only used to associate logical and physical data</summary>
		protected class ColumnEntry
		{
			/// <summary>The bar rect</summary>
			public SKRect Frame { get; set; }

			/// <summary>The logical data</summary>
			public GraphEntry Entry { get; set; }

			/// <summary>The label rect</summary>
			public SKRect LabelFrame { get; set; }
		}

		#region [ Constructors ]

		/// <summary>Constructor</summary>
		public ProgressGraph()
		{
			// set the single child view and event handlers
			this.MasterCanvas = new SKCanvasView();
			this.Content = this.MasterCanvas;
			this.MasterCanvas.PaintSurface += OnCanvasViewPaintSurface;

			//InitializeComponent();
		}

		#endregion [ Constructors ]

		#region [ Bindable Properties ]

		/// <summary>the top level canvas to draw on</summary>
		public SKCanvasView MasterCanvas { get; set; }

		/// <summary>Logical graph data</summary>
		public List<GraphEntry> Entries
		{
			get { return (List<GraphEntry>)GetValue( EntriesProperty ); }
			set { SetValue( EntriesProperty, value ); }
		}

		/// <summary>Color to paint the axis</summary>
		public Color AxisColor
		{
			get { return (Color)GetValue( AxisColorProperty ); }
			set { SetValue( AxisColorProperty, value ); }
		}

		/// <summary>Color to paint X Axis Labels</summary>
		public Color XLabelColor
		{
			get { return (Color)GetValue( XLabelColorProperty ); }
			set { SetValue( XLabelColorProperty, value ); }
		}

		/// <summary>Thickness of the bars between 1-100%</summary>
		public float BarThickness
		{
			get { return (float)GetValue( BarThicknessProperty ); }
			set
			{
				if ( value < 0 )
					value = 1.0F;

				if ( value > 100.0F )
					value = 100.0F;

				SetValue( BarThicknessProperty, value );
			}
		}

		/// <summary>Scales the label front from a % of 100</summary>
		public float XLabelFontScale
		{
			get { return this._xFontScale; }
			set
			{
				this._xFontScale = (value > 0.0F && value < 100.0F) ? value : 100.0F;
			}
		}

		/// <summary>What font family to use for the X Labels</summary>
		public string XLabelFontFamily
		{
			get { return (string)GetValue( XLabelFontFamilyProperty ); }
			set { SetValue( XLabelFontFamilyProperty, value ); }
		}

		/// <summary>Draw horizontal scale lines</summary>
		public bool HorizontalScale { get; set; }

		/// <summary>Draw the horizontal Axis</summary>
		public bool HorizontalAxis { get; set; }

		/// <summary>Draw the horizontal Axis</summary>
		public bool VerticalAxis { get; set; }

		/// <summary>Draw the layout helper grid</summary>
		public bool LayoutGrid { get; set; }

		#endregion [ Bindable Properties ]

		#region [ Calculated Properties ]

		/// <summary>The origin/center of the graph canvas itself</summary>
		protected SKPoint Origin { get; set; }

		/// <summary>Logical data associated with their physical rects</summary>
		protected ColumnEntry[] Frames { get; set; }

		/// <summary>Only the axis lines and bars themselves</summary>
		protected SKRect BarFrame { get; set; }

		/// <summary>The entire graph - bars and labels</summary>
		protected SKRect GraphFrame { get; set; }

		/// <summary></summary>
		internal LabelMetrics XLabelMetrics { get; set; }

		#endregion [ Calculated Properties ]

		#region [ Static Properties ]

		/// <summary></summary>
		public static readonly BindableProperty EntriesProperty = BindableProperty.Create(
			nameof( Entries ), typeof( List<GraphEntry> ), typeof( ProgressGraph ), null, BindingMode.TwoWay, null, OnEntriesPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty AxisColorProperty = BindableProperty.Create(
			nameof( AxisColor ), typeof( Color ), typeof( ProgressGraph ), Color.White, BindingMode.TwoWay, null, OnAxisColorPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty XLabelColorProperty = BindableProperty.Create(
			nameof( XLabelColor ), typeof( Color ), typeof( ProgressGraph ), Color.White, BindingMode.TwoWay, null, OnXLabelColorPropertyChanged );

		/// <summary></summary>
		public static readonly BindableProperty BarThicknessProperty = BindableProperty.Create(
			nameof( BarThickness ), typeof( float ), typeof( ProgressGraph ), 50.0F, BindingMode.TwoWay, null, OnBarThicknessChanged );

		/// <summary></summary>
		public static readonly BindableProperty XLabelFontFamilyProperty = BindableProperty.Create(
			nameof( XLabelFontFamily ), typeof( string ), typeof( ProgressGraph ), String.Empty, BindingMode.TwoWay, null, OnXLabelFontFamilyChanged );

		#endregion [ Static Properties ]

		#region [ Property Handlers ]

		/// <summary></summary>
		public static void OnEntriesPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressGraph;
			control.Entries = (List<GraphEntry>)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnAxisColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressGraph;
			control.AxisColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnXLabelColorPropertyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressGraph;
			control.XLabelColor = (Color)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnBarThicknessChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressGraph;
			control.BarThickness = (float)newValue;
			control.MasterCanvas.InvalidateSurface();
		}

		/// <summary></summary>
		public static void OnXLabelFontFamilyChanged( BindableObject sender, object oldValue, object newValue )
		{
			var control = sender as ProgressGraph;
			control.XLabelFontFamily = newValue.ToString();
			control.MasterCanvas.InvalidateSurface();
		}

		#endregion [ Property Handlers ]

		#region [ Methods ]

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
			this.DrawGraph( info, canvas );
			this.DrawAxis( info, canvas, this.VerticalAxis, this.HorizontalAxis );
			this.DrawXLabels( info, canvas );
		}

        /// <summary></summary>
        /// <param name="info"></param>
        /// <param name="canvas"></param>
        private void DrawGraph(SKImageInfo info, SKCanvas canvas)
        {
            // max height of any bar
            float height = this.BarFrame.Bottom - this.BarFrame.Top;

            // find the max value
            float maxValue = this.Frames.Max(e => e.Entry.Value);
            float maxThreshold = this.Frames.Max(e => e.Entry.Threshold);
            float max = new float[] { maxValue, maxThreshold }.Max();

            // draw the horizontal scale
            if (this.HorizontalScale)
            {
                // draw horizontal line every
                float yline = this.BarFrame.Bottom;
                float n = 10F;

                using (SKPaint pen = new SKPaint() { Color = this.AxisColor.ToSKColor(), StrokeWidth = 1.0F, Style = SKPaintStyle.Stroke })
                {
                    while (yline > 0)
                    {
                        canvas.DrawLine(this.BarFrame.Left, yline, this.BarFrame.Right, yline, pen);
                        yline -= height / n;
                    }
                }
            }

            // draw each bar
            foreach (ColumnEntry entry in this.Frames)
            {
                // the bar drawing frame
                SKRect rect = entry.Frame;

                // calc margin from thickness - use denominator from [3,15]
                float denom = 3.0F + ((15.0F - 3.0F) * (this.BarThickness / 100.0F));
                float margin = rect.Width / denom;

                // calculate threshold & value percentage of height
                float pt = (entry.Entry.Threshold / max) * height;
                float pv = (entry.Entry.Value / max) * height;

                // calculate threshold rect and value rect
                SKRect bart = new SKRect(rect.Left + margin, rect.Bottom - pt, rect.Right - margin, rect.Bottom);
                SKRect barv = new SKRect(rect.Left + margin, rect.Bottom - pv, rect.Right - margin, rect.Bottom);

                // draw threshold bar
                //canvas.DrawRect(bart, new SKPaint() { Color = entry.Entry.BackgroundColor, StrokeWidth = 0, Style = SKPaintStyle.StrokeAndFill });

                // epmty entries are ignored
                if (entry.Entry == null)
                    continue;

                // no data
                if (entry.Entry.IsEmpty)
                {
                    float rad = 10.0F;
                    using (SKPaint ptPaint = new SKPaint() { Color = this.AxisColor.ToSKColor(), StrokeWidth = rad, StrokeCap = SKStrokeCap.Round })
                    {
                        canvas.DrawPoint(rect.MidX, rect.Bottom - rad, ptPaint);
                    }
                }
                // reached goal then paint threshold on top of value
                else if (entry.Entry.Value >= entry.Entry.Threshold)
                {
                    using (SKPaint barPaint = new SKPaint() { Color = entry.Entry.ValueColor, StrokeWidth = 0, Style = SKPaintStyle.StrokeAndFill })
                    {
                        canvas.DrawRect(barv, barPaint);

                        barPaint.Color = entry.Entry.ThresholdColor;
                        canvas.DrawRect(bart, barPaint);
                    }

                    //// draw threshold line
                    //if (entry.Entry.Value > entry.Entry.Threshold)
                    //    canvas.DrawLine(bart.Left, bart.Top, bart.Right, bart.Top,
                    //    new SKPaint() { Color = entry.Entry.BackgroundColor, StrokeWidth = 3.0F, Style = SKPaintStyle.Stroke,
                    //        PathEffect = SKPathEffect.CreateDash(new float[] { 10, 10 }, 20)
                    //    });
                }
                else
                {
                    using (SKPaint barPaint = new SKPaint() { Color = entry.Entry.ThresholdColor, StrokeWidth = 0, Style = SKPaintStyle.StrokeAndFill })
                    {
                        canvas.DrawRect(bart, barPaint);

                        barPaint.Color = entry.Entry.ValueColor;
                        canvas.DrawRect(barv, barPaint);
                    }
                }

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
			this.GraphFrame = new SKRect( 0, 0, info.Width - 1, info.Height - 1 );

			// calculate the number of frames and their width
			int frameCnt = (this.Entries == null || this.Entries.Count < 2) ? 1 : this.Entries.Count;
			float frameWidth = Convert.ToSingle( this.GraphFrame.Width ) / frameCnt;

			// calc the dimensions on the X Label area - space needed to render all the text
			this.XLabelMetrics = (this.Entries != null) ? this.CalculateXLabels( frameWidth ) :
				new LabelMetrics( 100.0F, 0.0F, 0.0F );

			// calc the inner frame subtracting the max X label font height and the breath area
			this.BarFrame = new SKRect( this.GraphFrame.Left, this.GraphFrame.Top, this.GraphFrame.Right,
				this.GraphFrame.Bottom - this.XLabelMetrics.Height - this._xbreath );

			// calc each bars drawing rect
			this.Frames = new ColumnEntry[frameCnt];
			float left = this.BarFrame.Left;

			// iterate all the frames since entries may be null
			for ( int i = 0; i < this.Frames.Length; ++i )
			{
				this.Frames[i] = new ColumnEntry()
				{
					Frame = new SKRect( left, this.BarFrame.Top, left + frameWidth, this.BarFrame.Bottom ),
					Entry = (this.Entries == null || this.Entries.Count < 1) ? new GraphEntry() : this.Entries[i],
					LabelFrame = new SKRect( left, this.BarFrame.Bottom + this._xbreath, left + frameWidth, this.BarFrame.Bottom + this.XLabelMetrics.Height + this._xbreath )
				};

				left += frameWidth;
			}

		}

		/// <summary>Draws the grid axis</summary>
		private void DrawAxis( SKImageInfo info, SKCanvas canvas, bool vertical = true, bool horizontal = true )
		{
			using ( SKPaint axisPaint = new SKPaint() { Color = this.AxisColor.ToSKColor(), StrokeWidth = 2.0F, Style = SKPaintStyle.Stroke } )
			{
				if ( vertical )
					canvas.DrawLine( this.BarFrame.Left, this.BarFrame.Top, this.BarFrame.Top, this.BarFrame.Bottom, axisPaint );

				if ( horizontal )
					canvas.DrawLine( this.BarFrame.Left, this.BarFrame.Bottom, this.BarFrame.Right, this.BarFrame.Bottom, axisPaint );
			}
		}

		/// <summary>Draws the X axis labels</summary>
		private void DrawXLabels( SKImageInfo info, SKCanvas canvas )
		{
			// no data
			if ( this.Entries == null || this.Entries.Count < 0 )
				return;

			using ( SKPaint textPaint = new SKPaint() { Color = this.XLabelColor.ToSKColor(), TextSize = this.XLabelMetrics.FontSize } )
			{
				textPaint.TextAlign = SKTextAlign.Center;
				textPaint.IsAntialias = true;
				textPaint.IsStroke = false;
				textPaint.Typeface = SKTypeface.FromFamilyName( this.XLabelFontFamily );

				foreach ( ColumnEntry entry in this.Frames )
				{
					if ( entry.Entry == null || String.IsNullOrWhiteSpace( entry.Entry.Label ) )
						continue;

					SKRect rect = entry.LabelFrame;
					canvas.DrawText( entry.Entry.Label, rect.Left + (rect.Width / 2.0F), rect.Bottom - this.XLabelMetrics.Descent, textPaint );
				}
			}
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

				// frame bar area
				canvas.DrawRect( this.BarFrame, debugPaint );

				foreach ( ColumnEntry entry in this.Frames )
				{
					canvas.DrawRect( entry.Frame, debugPaint );
					canvas.DrawRect( entry.LabelFrame, debugPaint );
				}
			}
		}

		/// <summary>Minimum font that will fit in the label width area</summary>
		/// <returns></returns>
		internal LabelMetrics CalculateXLabels( float labelWidth, float startFontSize = 100.0F )
		{
			float minScale = 1000.0F;
			float maxHeight = 0.0F;
			float fontSize = startFontSize;
			float desc = 0.0F;

			using ( SKPaint textPaint = new SKPaint() { TextSize = fontSize, IsAntialias = true, IsStroke = false } )
			{
				textPaint.Typeface = SKTypeface.FromFamilyName( this.XLabelFontFamily );

				foreach ( GraphEntry entry in this.Entries )
				{
					if ( entry == null || String.IsNullOrWhiteSpace( entry.Label ) )
						continue;

					SKRect textBounds = new SKRect();
					textPaint.MeasureText( entry.Label, ref textBounds );

					float scale = labelWidth / textBounds.Width;

					if ( scale < minScale )
						minScale = scale;
				}

				// reduce the font size
				fontSize = textPaint.TextSize * minScale * (this.XLabelFontScale / 100.0F);
				textPaint.TextSize = fontSize;

				// calculate the font metrics for each label to figure out the max height needed
				foreach ( GraphEntry entry in this.Entries )
				{
					if ( entry == null || String.IsNullOrWhiteSpace( entry.Label ) )
						continue;

					SKFontMetrics metrics;
					textPaint.GetFontMetrics( out metrics );

					if ( metrics.Descent > desc )
						desc = metrics.Descent;

					// sets the distance from Ascent to Descent
					if ( metrics.Descent - metrics.Ascent > maxHeight )
						maxHeight = metrics.Descent - metrics.Ascent;
				}
			}

			return new LabelMetrics( fontSize, maxHeight, desc );
		}

		#endregion [ Methods ]

		/// <summary>Tries to determine to what scale the font should be to</summary>
		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="rect"></param>
		/// <returns>Returns a tuple of the new fontSize to fit the rect as well as the height of the calulated font rect</returns>
		//public static float CalculateFontForRect( string text, SKPaint font, SKRect rect )
		//{
		//	SKRect textBounds = new SKRect();
		//	font.MeasureText( text, ref textBounds );

		//	SKFontMetrics metrics;
		//	font.GetFontMetrics( out metrics );

		//	//float scale = new float[] { rect.Height / textBounds.Height, rect.Width / textBounds.Width }.Min();
		//	float scale = new float[] { rect.Height / (metrics.Descent - metrics.Ascent), rect.Width / textBounds.Width }.Min();

		//	float textSize = (font.TextSize * scale > 0) ? (font.TextSize * scale) : 0.1F;

		//	return textSize;
		//}
	}

}
