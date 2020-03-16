using System;
using System.Diagnostics;
using System.Windows.Input;
using CoBuster.Helpers;
using Xamarin.Forms;

namespace CoBuster.Controls
{
	public partial class TimerView : AbsoluteLayout
	{
		#region Bindable properties

		public static readonly BindableProperty TrackBarProperty =
			BindableProperty.Create(
				"TrackBar", typeof(View), typeof(TimerView),
				defaultValue: null);

		public View TrackBar
		{
			get { return (View)GetValue(TrackBarProperty); }
			set { SetValue(TrackBarProperty, value); }
		}

		public static readonly BindableProperty ProgressBarProperty =
			BindableProperty.Create(
				"ProgressBar", typeof(View), typeof(TimerView),
				defaultValue: null);

		public View ProgressBar
		{
			get { return (View)GetValue(ProgressBarProperty); }
			set { SetValue(ProgressBarProperty, value); }
		}

		public static readonly BindableProperty TimerLabelProperty =
			BindableProperty.Create(
				"TimerLabel", typeof(Label), typeof(TimerView),
				defaultValue: default(Label));

		public Label TimerLabel
		{
			get { return (Label)GetValue(TimerLabelProperty); }
			set { SetValue(TimerLabelProperty, value); }
		}

		public static readonly BindableProperty StartTimerCommandProperty =
			BindableProperty.Create(
				"StartTimerCommand", typeof(ICommand), typeof(TimerView),
				defaultBindingMode: BindingMode.OneWayToSource,
				defaultValue: default(ICommand));

		public ICommand StartTimerCommand
		{
			get { return (ICommand)GetValue(StartTimerCommandProperty); }
			set { SetValue(StartTimerCommandProperty, value); }
		}

		public static readonly BindableProperty PauseTimerCommandProperty =
			BindableProperty.Create(
				"PauseTimerCommand", typeof(ICommand), typeof(TimerView),
				defaultBindingMode: BindingMode.OneWayToSource,
				defaultValue: default(ICommand));

		public ICommand PauseTimerCommand
		{
			get { return (ICommand)GetValue(PauseTimerCommandProperty); }
			set { SetValue(PauseTimerCommandProperty, value); }
		}

		public static readonly BindableProperty StopTimerCommandProperty =
			BindableProperty.Create(
				"StopTimerCommand", typeof(ICommand), typeof(TimerView),
				defaultBindingMode: BindingMode.OneWayToSource,
				defaultValue: default(ICommand));

		public ICommand StopTimerCommand
		{
			get { return (ICommand)GetValue(StopTimerCommandProperty); }
			set { SetValue(StopTimerCommandProperty, value); }
		}

		public static readonly BindableProperty RemainingTimeProperty =
			BindableProperty.Create(
				"RemainingTime", typeof(TimeSpan), typeof(TimerView),
				defaultBindingMode: BindingMode.OneWayToSource,
				defaultValue: default(TimeSpan));

		public TimeSpan RemainingTime
		{
			get { return (TimeSpan)GetValue(RemainingTimeProperty); }
			set { SetValue(RemainingTimeProperty, value); }
		}

		public static readonly BindableProperty AutoStartProperty =
				BindableProperty.Create(
					"AutoStart", typeof(bool), typeof(TimerView),
					defaultValue: default(bool));

		public bool AutoStart
		{
			get { return (bool)GetValue(AutoStartProperty); }
			set { SetValue(AutoStartProperty, value); }
		}

		public static readonly BindableProperty TimeProperty =
			BindableProperty.Create(
				"Time", typeof(TimeSpan), typeof(TimerView),
				defaultValue: TimeSpan.FromSeconds(30));

		public TimeSpan Time
		{
			get { return (TimeSpan)GetValue(TimeProperty); }
			set { SetValue(TimeProperty, value); }
		}

		public static readonly BindableProperty IsTimerRunningProperty =
			BindableProperty.Create(
				"IsTimerRunning", typeof(bool), typeof(TimerView),
				defaultBindingMode: BindingMode.OneWayToSource,
				defaultValue: default(bool), propertyChanged: OnIsTimerRunningChanged);

		public bool IsTimerRunning
		{
			get { return (bool)GetValue(IsTimerRunningProperty); }
			set { SetValue(IsTimerRunningProperty, value); }
		}

		static void OnIsTimerRunningChanged(BindableObject bindable, object oldValue, object newValue)
		{
			((TimerView)bindable).OnIsTimerRunningChangedImpl((bool)oldValue, (bool)newValue);
		}

		#endregion

		readonly Stopwatch _stopWatch = new Stopwatch();
		public TimerView()
		{
			//Load view when size has been allocated, or device orientation changes
			SizeChanged += TimerView_SizeChanged;

			StartTimerCommand = new Command(HandleStartTimerCommand, CanStartTimer);
			PauseTimerCommand = new Command(HandlePauseTimerCommand, CanPauseOrStopTimer);
			StopTimerCommand = new Command(HandleStopTimerCommand, CanPauseOrStopTimer);
		}

		async void HandleStartTimerCommand(object param = null)
		{
			if (IsTimerRunning)
				return;

			ParseForTime(param);
			if (InitRemainingTime())
				_stopWatch.Reset();

			SetProgressBarWidth();

			IsTimerRunning = true;

			//Start animation
			await ProgressBar.WidthTo(0, Convert.ToUInt32(RemainingTime.TotalMilliseconds));

			//reset state
			IsTimerRunning = false;
		}

		bool InitRemainingTime()
		{
			//set remaining time if not paused
			if (RemainingTime < TimeSpan.FromSeconds(1))
			{
				RemainingTime = Time;
				return true;
			}

			return false;
		}

		void HandlePauseTimerCommand(object unused)
		{
			if (!IsTimerRunning)
				return;

			ProgressBar.CancelWidthToAnimation(); //abort animation
		}

		void HandleStopTimerCommand(object unused)
		{
			if (!IsTimerRunning)
				return;

			ProgressBar.CancelWidthToAnimation(); //abort animation

			ResetTimer();
		}

		protected virtual void OnIsTimerRunningChangedImpl(bool oldValue, bool newValue)
		{
			if (IsTimerRunning)
			{
				_stopWatch.Start();
				StartIntervalTimer(); //to update RemainingTime
			}
			else
				_stopWatch.Stop();

			((Command)StartTimerCommand).ChangeCanExecute();
			((Command)PauseTimerCommand).ChangeCanExecute();
			((Command)StopTimerCommand).ChangeCanExecute();
		}

		void TimerView_SizeChanged(object sender, EventArgs e)
		{
			if (Width == 0 || Height == 0)
				return;

			if (TrackBar == null || ProgressBar == null)
				return;

			if (Children.Count == 0)
			{
				//ensure track-bar gets full width and height as parent
				AddTrackBar();

				//ensure progress-bar gets full height, but width can be changed
				AddProgressBar();

				//if timer-label available, ensure it gets full width and height
				if (TimerLabel != null)
				{
					AddTimerLabel();
					TimerLabel.SetBinding(BindingContextProperty, new Binding(nameof(RemainingTime), source: this));
				}

				if (AutoStart)
					StartTimerCommand.Execute(Time);
			}
			else
			{
				if (IsTimerRunning)
				{
					//if timer runnnig already - restart animation
					ProgressBar.CancelWidthToAnimation();
					StartTimerCommand.Execute(null); //Use remaining time
				}
				else
					SetProgressBarWidth();
			}
		}

		void AddTimerLabel()
		{
			SetLayoutFlags(TimerLabel, AbsoluteLayoutFlags.SizeProportional);
			SetLayoutBounds(TimerLabel, new Rectangle(0, 0, 1, 1));
			Children.Add(TimerLabel);
		}

		void AddProgressBar()
		{
			SetLayoutFlags(ProgressBar, AbsoluteLayoutFlags.None);
			SetLayoutBounds(ProgressBar, new Rectangle(0, 0, Width, Height));
			Children.Add(ProgressBar);
		}

		void AddTrackBar()
		{
			SetLayoutFlags(TrackBar, AbsoluteLayoutFlags.SizeProportional);
			SetLayoutBounds(TrackBar, new Rectangle(0, 0, 1, 1));
			Children.Add(TrackBar);
		}

		bool _intervalTimer;
		void StartIntervalTimer()
		{
			if (_intervalTimer)
				return;

			Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
			{
				if (IsTimerRunning)
				{
					var remainingTime = Time.TotalMilliseconds - _stopWatch.Elapsed.TotalMilliseconds;
					if (remainingTime <= 100)
					{
						_intervalTimer = false;
						ResetTimer();
					}
					else
						RemainingTime = TimeSpan.FromMilliseconds(remainingTime);
				}

				return _intervalTimer = IsTimerRunning; //stop device-timer if timer was stopped
			});
		}

		void ResetTimer()
		{
			ProgressBar.CancelWidthToAnimation();
			RemainingTime = default(TimeSpan); //reset timer
			SetProgressBarWidth(); //reset width
		}

		bool CanPauseOrStopTimer(object arg)
		{
			return IsTimerRunning;
		}

		bool CanStartTimer(object arg)
		{
			return !IsTimerRunning;
		}

		void SetProgressBarWidth()
		{
			if (RemainingTime == Time)
				SetLayoutBounds(ProgressBar, new Rectangle(0, 0, Width, Height));
			else
			{
				var progress = ((double)RemainingTime.Seconds / Time.Seconds);
				SetLayoutBounds(ProgressBar, new Rectangle(0, 0, Width * progress, Height));
			}
		}

		void ParseForTime(object param)
		{
			if (param == null)
				return;

			if (param is string paramStr)
			{
				if (TimeSpan.TryParse(paramStr, out TimeSpan parsed))
				{
					Time = TimeSpan.FromSeconds(parsed.TotalSeconds);
				}
			}
			else if (param is TimeSpan timeSpan)
				Time = TimeSpan.FromSeconds(timeSpan.TotalSeconds);
		}
	}
}
