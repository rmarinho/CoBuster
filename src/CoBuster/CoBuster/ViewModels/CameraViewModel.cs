using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Analytics;
using Xamarin.Forms;

namespace CoBuster.ViewModels
{
	public class CameraViewModel : BaseViewModel
	{
		VitalSignsProcessing vitalSignsProcessingHR;
		VitalSignsProcessing vitalSignsProcessingO2;

		public Command StartMeasuringCommand { get; set; }

		public CameraViewModel()
		{
			Title = "Check Vital Signs";
			Instructions = "Please cover your phone camera with your index finger. Wait around for 40 seconds to get result";
			StartMeasuringCommand = new Command(() => StarMeasuring());
		}

		void StarMeasuring()
		{
			vitalSignsProcessingHR = new VitalSignsProcessing();
			vitalSignsProcessingO2 = new VitalSignsProcessing();
			CameraVisible = true;
			InstructionsVisible = false;
			Analytics.TrackEvent("Star measurings");
		}

		internal void PreviewCallback(byte[] data, Size size)
		{
			if (size.Width <= 0 || size.Height <= 0 || data.Length <= 1)
				return;
			MeasureO2(data, size);
		}

		void MeasureBeats(byte[] data, Size size)
		{
			if (vitalSignsProcessingHR == null)
				return;
			//we need to handle errors here
			var beats = vitalSignsProcessingHR.HRProcesssing(data, size);

			if (beats > 0)
			{
				Instructions = $"Your results for heart rate is {beats}";
				CameraVisible = false;
				InstructionsVisible = true;
				Analytics.TrackEvent($"Measuring success beats",
						new Dictionary<string, string>() { { "Beats", beats.ToString() } });
				vitalSignsProcessingHR = null;
			}
		}

		void MeasureO2(byte[] data, Size size)
		{
			if (vitalSignsProcessingO2 == null)
				return;
			//we need to handle errors here
			var o2 = vitalSignsProcessingO2.O2Processsing(data, size);

			if (o2 > 0)
			{
				Instructions = $"Your results for O2 saturation is {o2}";
				CameraVisible = false;
				InstructionsVisible = true;
				Analytics.TrackEvent($"Measuring success o2 results",
						new Dictionary<string, string>() { { "o2", o2.ToString() } });
				vitalSignsProcessingO2 = null;
			}
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
