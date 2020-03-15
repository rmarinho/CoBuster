using System;
using System.ComponentModel;

#if __ANDROID_29__
 using AndroidX.Fragment.App;
#else
using Android.Support.V4.App;
#endif

using Android.Content;
using Android.Views;
using Android.Widget;
using AView = Android.Views.View;

using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android.FastRenderers;
using Xamarin.Forms.Platform.Android;
using CoBuster.Controls;
using Xamarin.Forms;
using static CoBuster.Controls.CameraView;
using CoBuster.Droid.Controls;

[assembly: ExportRenderer(typeof(CameraView), typeof(CameraViewRenderer))]
namespace CoBuster.Droid.Controls
{
    public class CameraViewRenderer : FrameLayout, IVisualElementRenderer, IViewRenderer
    {
        int? _defaultLabelFor;
        bool _disposed;
        CameraView _element;
        VisualElementTracker _visualElementTracker;
        VisualElementRenderer _visualElementRenderer;
        FragmentManager _fragmentManager;

        FragmentManager FragmentManager => _fragmentManager ?? (_fragmentManager = Context.GetFragmentManager());

        CameraFragment _camerafragment;

        public CameraViewRenderer(Context context) : base(context)
        {
            _visualElementRenderer = new VisualElementRenderer(this);
        }

        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        public event EventHandler<PropertyChangedEventArgs> ElementPropertyChanged;

        async void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ElementPropertyChanged?.Invoke(this, e);

            switch (e.PropertyName)
            {
                case nameof(CameraView.CameraOptions):
                    await _camerafragment.RetrieveCameraDevice();
                    break;
                case nameof(CameraView.CaptureOptions):
                    _camerafragment.UpdateCaptureOptions();
                    await _camerafragment.RetrieveCameraDevice();
                    break;
                case nameof(CameraView.FlashMode):
                    _camerafragment.SetFlash();
                    if (Element.CaptureOptions == CameraCaptureOptions.Video)
                        _camerafragment.UpdateRepeatingRequest();
                    break;
                case nameof(CameraView.Zoom):
                    _camerafragment.ApplyZoom();
                    _camerafragment.UpdateRepeatingRequest();
                    break;
                case nameof(CameraView.VideoStabilization):
                    _camerafragment.SetVideoStabilization();
                    if (Element.CaptureOptions == CameraCaptureOptions.Video)
                        _camerafragment.UpdateRepeatingRequest();
                    break;
                case nameof(CameraView.PreviewAspect):
                case "MirrorFrontPreview":
                    _camerafragment?.ConfigureTransform();
                    break;
                case nameof(CameraView.KeepScreenOn):
                    if (_camerafragment != null)
                        _camerafragment.KeepScreenOn = Element.KeepScreenOn;
                    break;
            }
        }

        void OnElementChanged(ElementChangedEventArgs<CameraView> e)
        {
            CameraFragment newfragment = null;

            if (e.OldElement != null)
            {
                e.OldElement.PropertyChanged -= OnElementPropertyChanged;
                e.OldElement.ShutterClicked -= OnShutterClicked;
                _camerafragment.Dispose();
            }

            if (e.NewElement != null)
            {
                this.EnsureId();

                e.NewElement.PropertyChanged += OnElementPropertyChanged;
                e.NewElement.ShutterClicked += OnShutterClicked;

                ElevationHelper.SetElevation(this, e.NewElement);
                newfragment = new CameraFragment() { Element = _element };
            }

            FragmentManager.BeginTransaction()
                .Replace(Id, _camerafragment = newfragment, "camera")
                .Commit();

            ElementChanged?.Invoke(this, new VisualElementChangedEventArgs(e.OldElement, e.NewElement));
        }

        CameraView Element
        {
            get => _element;
            set
            {
                if (_element == value)
                    return;

	            var oldElement = _element;
                _element = value;

                OnElementChanged(new ElementChangedEventArgs<CameraView>(oldElement, _element));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _camerafragment.Dispose();

            _disposed = true;

            if (disposing)
            {
                SetOnClickListener(null);
                SetOnTouchListener(null);
                if (_visualElementTracker != null)
                {
                    _visualElementTracker.Dispose();
                    _visualElementTracker = null;
                }

                if (_visualElementRenderer != null)
                {
                    _visualElementRenderer.Dispose();
                    _visualElementRenderer = null;
                }

                if (Element != null)
                {
                    Element.PropertyChanged -= OnElementPropertyChanged;
                    Element.ShutterClicked -= OnShutterClicked;
                }
            }

            base.Dispose(disposing);
        }

        void OnShutterClicked(object sender, EventArgs e)
        {
            switch (Element.CaptureOptions)
            {
                default:
                case CameraCaptureOptions.Default:
                case CameraCaptureOptions.Photo:
                    _camerafragment.TakePhoto();
                    break;
                case CameraCaptureOptions.Video:
                    if (!_camerafragment.IsRecordingVideo)
                        _camerafragment.StartRecord();
                    else
                        _camerafragment.StopRecord();
                    break;
            }
        }

        void IViewRenderer.MeasureExactly() => MeasureExactly(this, Element, Context);

        #region IVisualElementRenderer
        VisualElement IVisualElementRenderer.Element => Element;

        ViewGroup IVisualElementRenderer.ViewGroup => null;

        VisualElementTracker IVisualElementRenderer.Tracker => _visualElementTracker;

        AView IVisualElementRenderer.View => this;

        SizeRequest IVisualElementRenderer.GetDesiredSize(int widthConstraint, int heightConstraint)
        {
            Measure(widthConstraint, heightConstraint);
            var result = new SizeRequest(new Size(MeasuredWidth, MeasuredHeight), new Size(Context.ToPixels(20), Context.ToPixels(20)));
            return result;
        }

        void IVisualElementRenderer.SetElement(VisualElement element)
        {
            if (!(element is CameraView camera))
                throw new ArgumentException($"{nameof(element)} must be of type {nameof(CameraView)}");

            Performance.Start(out string reference);

            if (_visualElementTracker == null)
                _visualElementTracker = new VisualElementTracker(this);

            Element = camera;

            Performance.Stop(reference);
        }

        void IVisualElementRenderer.SetLabelFor(int? id)
        {
            if (_defaultLabelFor == null)
                _defaultLabelFor = LabelFor;

            LabelFor = (int)(id ?? _defaultLabelFor);
        }

        void IVisualElementRenderer.UpdateLayout()
        {
            _visualElementTracker?.UpdateLayout();
        }

        internal static void MeasureExactly(AView control, VisualElement element, Context context)
        {
            if (control == null || element == null)
            {
                return;
            }

            var width = element.Width;
            var height = element.Height;

            if (width <= 0 || height <= 0)
            {
                return;
            }

            var realWidth = (int)context.ToPixels(width);
            var realHeight = (int)context.ToPixels(height);

            var widthMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realWidth, MeasureSpecMode.Exactly);
            var heightMeasureSpec = MeasureSpecFactory.MakeMeasureSpec(realHeight, MeasureSpecMode.Exactly);

            control.Measure(widthMeasureSpec, heightMeasureSpec);
        }
        #endregion
    }

    internal static class MeasureSpecFactory
    {
        public static int GetSize(int measureSpec)
        {
            const int modeMask = 0x3 << 30;
            return measureSpec & ~modeMask;
        }

        // Literally does the same thing as the android code, 1000x faster because no bridge cross
        // benchmarked by calling 1,000,000 times in a loop on actual device
        public static int MakeMeasureSpec(int size, MeasureSpecMode mode)
        {
            return size + (int)mode;
        }
    }
}
