using System;
using System.Collections.Generic;

using Xamarin.Forms;
using static CoBuster.Controls.CameraView;

namespace CoBuster.Views
{
	public partial class CameraPage : ContentPage
	{
		public CameraPage()
		{
			InitializeComponent();

			cameraView.CameraOption = CameraOptions.Default;
            cameraView.FlashMode = CameraFlashMode.On;
            cameraView.MediaCaptured += (_, e) =>
            {
                switch (cameraView.CaptureOptions)
                {
                    default:
                    case CameraCaptureOptions.Default:
                    case CameraCaptureOptions.Photo:
                        //testMediaElement.IsVisible = false;
                        //testImage.IsVisible = true;
                        //testImage.Source = e.Image;
                        buttonShot.Text = "Shot";
                        break;
                    case CameraCaptureOptions.Video:
                        //testImage.IsVisible = false;
                        //testMediaElement.IsVisible = true;
                        //testMediaElement.Source = e.Video;
                        buttonShot.Text = "Start record";
                        break;
                }
            };

            cameraView.OnAvailable += (_, available) =>
            {
                if (available)
                {
                }
                buttonShot.IsEnabled = available;
            };
        }



        void buttonShot_Clicked(System.Object sender, System.EventArgs e)
        {
            cameraView.Shutter();
        }
    }
}
