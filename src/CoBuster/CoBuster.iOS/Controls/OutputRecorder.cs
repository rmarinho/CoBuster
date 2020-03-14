using AVFoundation;
using CoreMedia;
using CoreVideo;
using System.Runtime.InteropServices;

namespace CoBuster.iOS.Controls
{
	class OutputRecorder : AVCaptureVideoDataOutputSampleBufferDelegate
	{
		FormsCameraView _formsCameraView;
		public OutputRecorder(FormsCameraView formsCameraView)
		{
			_formsCameraView = formsCameraView;
		}
		public override void DidOutputSampleBuffer(AVCaptureOutput captureOutput, CMSampleBuffer sampleBuffer, AVCaptureConnection connection)
		{
			using (var pixelBuffer = sampleBuffer.GetImageBuffer() as CVPixelBuffer)
			{
				// Lock the base address
				pixelBuffer.Lock(CVPixelBufferLock.ReadOnly);
				// Get the number of bytes per row for the pixel buffer
				var baseAddress = pixelBuffer.BaseAddress;
				int bytesPerRow = (int)pixelBuffer.BytesPerRow;
				int width = (int)pixelBuffer.Width;
				int height = (int)pixelBuffer.Height;


				byte[] managedArray = new byte[width * height];
				Marshal.Copy(baseAddress, managedArray, 0, width * height);
				pixelBuffer.Unlock(CVPixelBufferLock.ReadOnly);

				_formsCameraView.RaiseFrameAvailable(managedArray);
			}
		}
	}
}
