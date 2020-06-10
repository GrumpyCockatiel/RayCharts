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

			this.meterChartView.PrimeLabel = "Limy Áhm 333";
			this.meterChartView.SubLabel = "subby label";
		}
	}
}
