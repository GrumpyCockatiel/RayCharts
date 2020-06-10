using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SkiaSharp;
using Raydreams.Xamarin.Controls;

namespace RayCharts.Views
{
    public partial class Controls1Page : ContentPage
    {
		SKColor blue = new SKColor( 0, 0, 255 );
		SKColor gray = new SKColor( 100, 100, 100 );
		SKColor ltgray = new SKColor( 200, 200, 200 );
		SKColor green = new SKColor( 0, 255, 0 );
		SKColor red = new SKColor( 255, 0, 0 );
		SKColor rewards = new SKColor( 128, 195, 67 );
		SKColor bonus = new SKColor( 9, 120, 86 );
		SKColor white = new SKColor( 255, 255, 255 );
		SKColor lightWhite = new SKColor( 255, 255, 255, 128 );
		SKColor error = SKColor.Parse( "DC722B" );
		SKColor yellow = new SKColor( 240, 238, 118 );

		public Controls1Page()
        {
            InitializeComponent();
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();

			this.myChartView.Entries = new List<GraphEntry>();
			this.myChartView.Entries.Add( new GraphEntry( 8, 10 ) { Label = "jan", ThresholdColor = gray, ValueColor = yellow } );
			this.myChartView.Entries.Add( new GraphEntry( 15, 9 ) { Label = "feb", ThresholdColor = rewards, ValueColor = bonus } );
			this.myChartView.Entries.Add( new GraphEntry( 3, 11 ) { Label = "mar", ThresholdColor = gray, ValueColor = yellow } );
			this.myChartView.Entries.Add( new GraphEntry( 10, 10 ) { Label = "apr", ThresholdColor = rewards, ValueColor = bonus } );
			//this.dcuChartView.Entries.Add( new GraphEntry() { Label = "Apr" } );
			this.myChartView.Entries.Add( new GraphEntry( 0, 7 ) { Label = "may", ThresholdColor = gray, ValueColor = yellow } );
			this.myChartView.Entries.Add( new GraphEntry( 17, 15 ) { Label = "jun", ThresholdColor = rewards, ValueColor = bonus } );

			this.myChartView.AxisColor = Color.LightYellow;
			this.myChartView.XLabelColor = Color.White;

			LineSeries l1 = new LineSeries()
			{
				LineColor = green,
				StrokeWidth = 2.5F,
				Values = new SKPoint[] { new SKPoint(1,0.0F),
				new SKPoint( 2, 0.98F ), new SKPoint( 3, 1.05F ),
				new SKPoint( 4, 5.5F), new SKPoint(5,6.11F), new SKPoint(6,6.55F) }
			};

			LineSeries l2 = new LineSeries()
			{
				LineColor = green,
				StrokeWidth = 2.5F,
				Values = new SKPoint[] { new SKPoint(1,0.10F),
				new SKPoint( 2, 0.98F ), new SKPoint( 3, 1.05F ),
				new SKPoint( 4, 1.25F), new SKPoint(5,1.97F), new SKPoint(6,2.02F) }
			};

			List<LineSeries> s = new List<LineSeries>();
			s.Add( l2 );

			this.lineChartView.Series = s;
			this.lineChartView.LabelColor = Color.White;
			this.lineChartView.AxisColor = Color.FromHex( "DC722B" );
			this.lineChartView.GridlineColor = Color.FromRgba( 255, 255, 255, 128 );
			this.lineChartView.XLabelHeight = 50;
			this.lineChartView.YLabelWidth = 100;
			this.lineChartView.TitleHeight = 50;

			this.lineChartView.XLabels = new List<LineLabel>( new[] { new LineLabel( 1, "jan" ), new LineLabel( 2, "feb" ), new LineLabel( 3, "mar" ), new LineLabel( 4, "apr" ), new LineLabel( 5, "may" ), new LineLabel( 6, "jun" ) } );
			this.lineChartView.YLabels = new List<LineLabel>( new[] { new LineLabel( 0, "$0.00" ), new LineLabel( 10, "$10.00" ), new LineLabel( 5, "$5.00" ), new LineLabel( 2.5F, "$2.50" ), new LineLabel( 7.5F, "$7.50" ) } );
		}
	}
}
