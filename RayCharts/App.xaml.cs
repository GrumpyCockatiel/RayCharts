﻿using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using RayCharts.Views;

namespace RayCharts
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
