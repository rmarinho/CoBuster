using System;
using System.Collections.Generic;
using CoBuster.ViewModels;
using Xamarin.Forms;

namespace CoBuster.Views
{
	public partial class Profile : ContentPage
	{
		ProfileViewModel viewModel;

		public Profile()
		{
			InitializeComponent();
			BindingContext = viewModel = new ProfileViewModel();
		}
	}
}
