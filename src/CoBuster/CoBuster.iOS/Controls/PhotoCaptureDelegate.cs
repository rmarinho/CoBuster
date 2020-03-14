using System;

using Foundation;
using AVFoundation;
using CoreMedia;

namespace CoBuster.iOS.Controls
{
	class PhotoCaptureDelegate : NSObject, IAVCapturePhotoCaptureDelegate
	{
		public Action<NSData, NSError> OnFinishCapture;
		public Action WillCapturePhotoAnimation;

		NSData photoData;

		[Export("captureOutput:willCapturePhotoForResolvedSettings:")]
		public void WillCapturePhoto(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings) => WillCapturePhotoAnimation();

		[Export("captureOutput:didFinishProcessingPhotoSampleBuffer:previewPhotoSampleBuffer:resolvedSettings:bracketSettings:error:")]
		public void DidFinishProcessingPhoto(AVCapturePhotoOutput captureOutput, CMSampleBuffer photoSampleBuffer, CMSampleBuffer previewPhotoSampleBuffer, AVCaptureResolvedPhotoSettings resolvedSettings, AVCaptureBracketedStillImageSettings bracketSettings, NSError error)
		{
			if (photoSampleBuffer != null)
				photoData = AVCapturePhotoOutput.GetJpegPhotoDataRepresentation(photoSampleBuffer, previewPhotoSampleBuffer);
			else
				Console.WriteLine($"Error capturing photo: {error.LocalizedDescription}");
		}

		[Export("captureOutput:didFinishCaptureForResolvedSettings:error:")]
		public void DidFinishCapture(AVCapturePhotoOutput captureOutput, AVCaptureResolvedPhotoSettings resolvedSettings, NSError error)
			=> OnFinishCapture(photoData, error);
	}
}
