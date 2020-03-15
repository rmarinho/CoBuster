using System;
using System.ComponentModel;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CoBuster.Views
{
	[DesignTimeVisible(false)]
	public partial class AboutPage : ContentPage
	{
		public AboutPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Analytics.TrackEvent($"Visiting About Page");
		}
	}
}