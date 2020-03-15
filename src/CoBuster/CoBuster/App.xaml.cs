using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CoBuster.Services;
using CoBuster.Views;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter.Distribute;

namespace CoBuster
{
	public partial class App : Application
	{

		public App()
		{
			InitializeComponent();

			DependencyService.Register<MockDataStore>();
			MainPage = new AppShell();
		}

		protected override void OnStart()
		{
			AppCenter.Start("ios=d02b23db-b2db-4a6d-8e61-ca9ded94a146;" +
							"android=509c7cef-2067-4421-91a9-920e9c7099fc",
				  typeof(Analytics), typeof(Crashes), typeof(Distribute));
		}

		protected override void OnSleep()
		{
		}

		protected override void OnResume()
		{
		}
	}
}
