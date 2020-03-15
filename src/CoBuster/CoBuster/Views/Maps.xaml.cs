using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace CoBuster.Views
{
	public partial class Maps : ContentPage
	{
		public Maps()
		{
			InitializeComponent();
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await GoTouserLocation();
		}

		async void ImageButton_Clicked(System.Object sender, System.EventArgs e)
		{
			await GoTouserLocation();
		}

		async Task GoTouserLocation()
		{
			try
			{
				var location = await Geolocation.GetLastKnownLocationAsync();

				if (location != null)
				{
					var userPosition = new Xamarin.Forms.Maps.Position(location.Latitude, location.Longitude);
					map.MoveToRegion(MapSpan.FromCenterAndRadius(userPosition, Distance.FromMeters(100)));
					Console.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
				}
			}
			catch (FeatureNotSupportedException fnsEx)
			{
				// Handle not supported on device exception
			}
			catch (FeatureNotEnabledException fneEx)
			{
				// Handle not enabled on device exception
			}
			catch (PermissionException pEx)
			{
				// Handle permission exception
			}
			catch (Exception ex)
			{
				// Unable to get location
			}
		}
	}
}
