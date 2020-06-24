using System;
using System.Collections.Generic;
using Xamarin.Forms;
using SkiaSharp;
using Raydreams.Xamarin.Controls;

namespace RayCharts.Views
{
    public partial class Controls2Page : ContentPage
    {
        public Controls2Page()
        {
            InitializeComponent();
        }

		protected override void OnAppearing()
		{
			base.OnAppearing();

			//this.pieChartView.ArcLineWidth = Convert.ToSingle( this.pieChartView.Width / 15.0F );

			//this.meterChartView.PrimeLabel = "Limy Áhm 222";
			//this.meterChartView.SubLabel = "subby label";

			//this.pieChartView.PrimeLabel = "1ÁyE";
			//this.pieChartView.PrimeLabelFontScale = 50.0F;
			int x = 5;
		}
	}
}
