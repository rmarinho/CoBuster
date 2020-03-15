using System;

using Xamarin.Forms;

namespace CoBuster.ViewModels
{
	public class ProfileViewModel : BaseViewModel
	{
		public ProfileViewModel()
		{
			Title = "Profile";
			GoToAboutPage = new Command(() => {

			});
		}

		public Command GoToAboutPage { get; set; }
	}
}

