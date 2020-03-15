using System;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace CoBuster.ViewModels
{
	public class CameraViewModel : BaseViewModel
	{
		VitalSignsProcessing _vitalSignsProcessing;

		public Command StartMeasuringCommand { get; set; }
		public CameraViewModel()
		{
			Title = "Check Vital Signs";
			Instructions = "Please cover your phone camera with your index finger. Wait around for 40 seconds to get result";
			StartMeasuringCommand = new Command(() => StarMeasuring());
		}

		void StarMeasuring()
		{
			_vitalSignsProcessing = new VitalSignsProcessing();
			CameraVisible = true;
			InstructionsVisible = false;
			Analytics.TrackEvent("Star measuring o2");
		}

		internal void PreviewCallback(byte[] data, Size size)
		{
			if (_vitalSignsProcessing == null)
				return;

			//we need to handle errors here
			var o2 = _vitalSignsProcessing.O2Processsing(data, size);

			if (o2 > 0)
			{
				Instructions = $"Your results for O2 saturation is {o2}";
				CameraVisible = false;
				InstructionsVisible = true;
				Analytics.TrackEvent($"Measuring success o2 results {o2}");
			}
			
			_vitalSignsProcessing = null;
		}

		string instructions = string.Empty;
		public string Instructions
		{
			get { return instructions; }
			set { SetProperty(ref instructions, value); }
		}


		bool _instructionsVisible = true;
		public bool InstructionsVisible
		{
			get { return _instructionsVisible; }
			set { SetProperty(ref _instructionsVisible, value); }
		}

		bool _cameraVisible = false;
		public bool CameraVisible
		{
			get { return _cameraVisible; }
			set { SetProperty(ref _cameraVisible, value); }
		}
	}
}
