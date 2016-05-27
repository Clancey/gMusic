using System;
using AppKit;
using CoreGraphics;
using System.Collections.Generic;
using MusicPlayer.Playback;
using MusicPlayer.Data;
using MusicPlayer.Models;
using MusicPlayer.Managers;
using System.Linq;
using Foundation;

namespace MusicPlayer
{
	public class EqualizerView : NSView, ILifeCycleView
	{
		NSButton enabledSwitch;
		NSSlider preAmp;
		NSTextField preampLabel;
		NSVisualEffectView blur;
		NSComboBox comboBox;
		List<NSSlider> sliders = new List<NSSlider> ();
		List<NSTextField> sliderLabels = new List<NSTextField>();


		public EqualizerView ()
		{
			AddSubview(blur = new NSVisualEffectView());
			AddSubview (enabledSwitch = new NSButton {
				Title = "On",
				State = Equalizer.Shared.Active ? NSCellStateValue.On : NSCellStateValue.Off,
			});
			enabledSwitch.SetButtonType (NSButtonType.Switch);
			enabledSwitch.SizeToFit ();

			AddSubview(comboBox = new NSComboBox (new CGRect(0,0,300,40)));
			comboBox.Editable = false;
//			AddSubview (preAmp = new NSSlider (new CGRect(0,0,21,100)));
//			AddSubview (preampLabel = new NSTextField {StringValue = "Preamp"}.StyleAsMainTextCentered());
			init ();
		}

		#region ILifeCycleView implementation

		public void ViewWillAppear ()
		{
			NotificationManager.Shared.EqualizerChanged += NotificationManager_Shared_EqualizerChanged;
			EqualizerManager.Shared.EqualizerReloaded += EqualizerManager_Shared_EqualizerReloaded;
			enabledSwitch.Activated += EnabledSwitch_Activated;
			comboBox.SelectionChanged += ComboBox_SelectionChanged;
			NotificationManager.Shared.EqualizerEnabledChanged += NotificationManager_Shared_EqualizerEnabledChanged;

			enabledSwitch.State = Equalizer.Shared.Active ? NSCellStateValue.On : NSCellStateValue.Off ;
			SetupCombobox ();
			SetPreset (EqualizerManager.Shared.GetCurrent ());
		}

		void NotificationManager_Shared_EqualizerEnabledChanged (object sender, EventArgs e)
		{
			enabledSwitch.State = Equalizer.Shared.Active ? NSCellStateValue.On : NSCellStateValue.Off ;
		}

		public void ViewWillDissapear ()
		{
			EqualizerManager.Shared.EqualizerReloaded -= EqualizerManager_Shared_EqualizerReloaded;
			NotificationManager.Shared.EqualizerChanged -= NotificationManager_Shared_EqualizerChanged;

			NotificationManager.Shared.EqualizerEnabledChanged -= NotificationManager_Shared_EqualizerEnabledChanged;

			enabledSwitch.Activated -= EnabledSwitch_Activated;
			comboBox.SelectionChanged -= ComboBox_SelectionChanged;
		}

		void ComboBox_SelectionChanged (object sender, EventArgs e)
		{
			if (comboBox == null)
				return;
			if (comboBox.SelectedIndex < 0)
				return;
			var name = comboBox?.SelectedValue?.ToString();
			if (string.IsNullOrEmpty (name))
				return;
			var preset = Equalizer.Shared.Presets.FirstOrDefault (x => x.Name == name);
			SetPreset (preset);
		}

		void EnabledSwitch_Activated (object sender, EventArgs e)
		{
			Equalizer.Shared.Active = enabledSwitch.State == NSCellStateValue.On;
		}

		void NotificationManager_Shared_EqualizerChanged (object sender, EventArgs e)
		{
			SetPreset (EqualizerManager.Shared.GetCurrent ());
		}

		void EqualizerManager_Shared_EqualizerReloaded ()
		{
			SetupCombobox ();
		}

		void SetupCombobox()
		{
			comboBox.RemoveAll ();
			var currentItems = Equalizer.Shared.Presets.Select (x => new NSString (x.Name)).ToArray ();
			comboBox.Add (currentItems);
			var current = EqualizerManager.Shared.GetCurrent ();
			comboBox.Select(new NSString(current.Name));
		}

		public void SetPreset(EqualizerPreset preset)
		{
			if (preset == null)
				return;
			try
			{
				EqualizerManager.Shared.SaveCurrent();
				shouldSave = false;
				Equalizer.Shared.CurrentPreset = preset;
				Settings.EqualizerPreset = preset.Id;
				for (int i = 0; i < preset.Values.Length; i++)
				{
					var value = preset.Values[i].Value;
					sliders[i].DoubleValue = value;
					EqualizerManager.Shared.SetGain((int) sliders[i].Tag,(float) value);
				}
				shouldSave = true;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		#endregion

		void init()
		{
			clearSliders();
			for (int i = 0; i < Equalizer.Shared.Bands.Length; i++)
			{
				CreateSlider(i);
				//UpdateGain(i);
			}
			updateImages();
		}

		void updateImages()
		{
//			var image = Settings.EqualizerEnabled ? thumbImageOn : thumbImageOff;
//			foreach (var slider in Sliders)
//			{
//				slider.SetThumbImage(image, UIControlState.Normal);
//				slider.SetThumbImage(image, UIControlState.Highlighted);
//				var track = Settings.EqualizerEnabled ? Images.AccentImage.Value : Images.GetSliderTrack();
//				slider.SetMinTrackImage(track, UIControlState.Normal);
//			}
		}

		void clearSliders()
		{
			foreach (var slider in sliders)
			{
				slider.RemoveFromSuperview();
				slider.Activated -= slider_changed;
			}
			foreach (var label in sliderLabels)
			{
				label.RemoveFromSuperview();
			}
			sliderLabels = new List<NSTextField>();
			sliders = new List<NSSlider> ();
		}

		void CreateSlider (int index)
		{
			var band = Equalizer.Shared.Bands[index];
			var s = new NSSlider (new CGRect(0,0,21,100));
			s.DoubleValue = band.Gain;
			s.Tag = index;
			s.Activated += slider_changed;
			s.MinValue = -12;
			s.MaxValue = 12;
			sliders.Add (s);
			AddSubview (s);
			var label = new NSTextField {
				StringValue = band.ToString(),
			}.StyleAsMainTextCentered();
			sliderLabels.Add (label);
			AddSubview (label);

		}

		static float range = 12f;
		bool shouldSave = true;
		void slider_changed (object sender, EventArgs e)
		{
			var slider = sender as NSSlider;
			var gain = slider.DoubleValue;
			EqualizerManager.Shared.SetGain((int) slider.Tag, (float)gain);
			if (shouldSave)
				EqualizerManager.Shared.SaveCurrent();
		}





		public override bool IsFlipped {
			get {
				return true;
			}
		}

		const float topItemsPadding = 10;
		const float sidePadding = 20f;

		const float EqSliderMaxHeight = 450f;
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var bounds = Bounds;
			blur.Frame = bounds;
			var sliderCount = sliders.Count;
			var width = this.Bounds.Width/(sliderCount + 1);

			var padding = width/2;
			var right = Bounds.Width - sidePadding;


			var sliderTop = 100;
			var available = bounds.Bottom - sliderTop - (topItemsPadding*2) - 25;

			nfloat height = 0;
			//if (Util.IsIphone)
			height = NMath.Min(available, EqSliderMaxHeight);
			//else
			//	height = this.Bounds.Height - 65;
			var sliderH = sliderTop + ((available - height)/2) + topItemsPadding;
			nfloat x;

//			var offset = width * 2;
//			preAmp.Frame = new CGRect (padding, sliderH, width, height);
//			preampLabel.Frame = new CGRect(padding, preAmp.Frame.Bottom, width, 25);
			for (int i = 0; i < sliders.Count; i++)
			{
				x = width*i + padding;
				var slider = sliders[i];
				var label = sliderLabels[i];
				slider.Frame = new CGRect(x, sliderH, width, height);
			
				label.Frame = new CGRect(x, slider.Frame.Bottom, width, 25);
			}

			var frame = enabledSwitch.Frame;
			frame.X = padding + (frame.Width - width)/2;
			frame.Y = sliderH - topItemsPadding - frame.Height;
			enabledSwitch.Frame = frame;

			x = frame.Right + padding;
			comboBox.SizeToFit ();
			frame = comboBox.Frame;
			frame.Width = 300;
			frame.X = x;
			frame.Y =  sliderH - topItemsPadding - frame.Height;
			comboBox.Frame = frame;



		}
	}
}

