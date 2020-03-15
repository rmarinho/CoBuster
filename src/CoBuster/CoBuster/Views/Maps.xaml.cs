using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
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
			Analytics.TrackEvent($"Visiting Maps Page");

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
					var userPosition = new Position(location.Latitude, location.Longitude);
					map.MoveToRegion(MapSpan.FromCenterAndRadius(userPosition, Distance.FromMeters(100)));
					Analytics.TrackEvent($"GotLocation Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
				}
			}
			catch (FeatureNotSupportedException fnsEx)
			{
				Crashes.TrackError(fnsEx);
				// Handle not supported on device exception
			}
			catch (FeatureNotEnabledException fneEx)
			{
				Crashes.TrackError(fneEx);
				// Handle not enabled on device exception
			}
			catch (PermissionException pEx)
			{
				Crashes.TrackError(pEx);
				// Handle permission exception
			}
			catch (Exception ex)
			{
				Crashes.TrackError(ex);
				// Unable to get location
			}
		}
	}
}
