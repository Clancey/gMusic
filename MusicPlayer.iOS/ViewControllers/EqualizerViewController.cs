using System;
using System.Collections.Generic;
using CoreGraphics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Localizations;
using UIKit;
using MusicPlayer.Models;
using MusicPlayer.Playback;
using MusicPlayer.Managers;
using MusicPlayer.iOS.ViewControllers;
using MusicPlayer.Data;

namespace MusicPlayer.iOS
{
	internal class EqualizerViewController : UIViewController
	{
		public EqualizerViewController()
		{
			Title = Strings.Equalizer;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			view.combobox.Items = Equalizer.Shared.Presets.ToArray();
			view.onSwitch.On = Equalizer.Shared.Active;
			view.SetPreset(Equalizer.Shared.CurrentPreset);
			NotificationManager.Shared.EqualizerChanged += HandleEqualizerChanged;

			NotificationManager.Shared.VideoPlaybackChanged += Shared_VideoPlaybackChanged;
			EqualizerManager.Shared.EqualizerReloaded += EqualizerManager_Shared_EqualizerReloaded;
			ApplyStyle();
		}
		void ApplyStyle()
		{
			this.StyleViewController();
			view.ApplyStyle();
		}
		void EqualizerManager_Shared_EqualizerReloaded ()
		{
			view.combobox.Items = Equalizer.Shared.Presets.ToArray();
		}

		private void Shared_VideoPlaybackChanged(object sender, SimpleTables.EventArgs<bool> e)
		{
			View.SetNeedsLayout();
		}

		void HandleEqualizerChanged(object sender, EventArgs e)
		{
			view.combobox.SelectedItem = Equalizer.Shared.CurrentPreset;
			view.onSwitch.On = Equalizer.Shared.Active;
		}

		public override UIStatusBarStyle PreferredStatusBarStyle()
		{
			return UIStatusBarStyle.LightContent;
		}

		EqualizerView view;
		UIBarButtonItem menuButton;
		public override void LoadView()
		{
			View = view = new EqualizerView(this);
			var style = View.GetStyle();
			if (NavigationController == null)
				return;
			NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = style.AccentColor
			};

			menuButton = new UIBarButtonItem(Images.MenuImage, UIBarButtonItemStyle.Plain,
				(s, e) => { NotificationManager.Shared.ProcToggleMenu(); })
			{
				AccessibilityIdentifier = "menu",
			};
			NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			NotificationManager.Shared.EqualizerChanged -= HandleEqualizerChanged;
			NotificationManager.Shared.VideoPlaybackChanged -= Shared_VideoPlaybackChanged;
			EqualizerManager.Shared.EqualizerReloaded -= EqualizerManager_Shared_EqualizerReloaded;
			try
			{
				EqualizerManager.Shared.SaveCurrent();
			}
			catch (Exception ex)
			{
				//Logger.Log (ex);
			}
		}

		public class EqualizerView : UIView
		{
			UILabel active = new UILabel();
			UILabel preset = new UILabel();
			List<UISlider> Sliders = new List<UISlider>();
			List<UILabel> labels = new List<UILabel>();
			public UISwitch onSwitch;
			nfloat sliderH = 0f;
			EqualizerViewController Parent;
			const float topItemsPadding = 10;
			const float sidePadding = 20f;
			UIView line1;
			UIView line2;
			public UIComboBox combobox;
			UIToolbar toolbar;

			public EqualizerView(EqualizerViewController eqvc)
			{
				BackgroundColor = UIColor.White;
				Parent = eqvc;

				line1 = new UIView()
				{
//						BackgroundColor =Style.Current.Equalizer.LineColor.Value,
				};
				this.AddSubview(line1);
				line2 = new UIView()
				{
//						BackgroundColor =Style.Current.Equalizer.LineColor.Value,
				};
				this.AddSubview(line2);
				active.Text = Strings.Active;
//				active.Font =Style.Current.Equalizer.TitleFont.Value;
//				active.TextColor =Style.Current.Equalizer.TitleFontColor.Value;
				active.SizeToFit();
				this.AddSubview(active);


				preset.Text = Strings.DefaultPreset;
//				preset.Font =Style.Current.Equalizer.TitleFont.Value;
//				preset.TextColor =Style.Current.Equalizer.TitleFontColor.Value;
				preset.SizeToFit();
				this.AddSubview(preset);


				onSwitch = new UISwitch(new CGRect(0, 0, 47, 10)).StyleSwitch();
				onSwitch.On = Settings.EqualizerEnabled;
//				onSwitch.TintColor = UIColor.FromPatternImage(Images.SwitchOffBackground.Value);
//				onSwitch.OnTintColor = UIColor.FromPatternImage(Images.SwitchOnBackground.Value);
//				onSwitch.ThumbTintColor =Style.Current.Equalizer.SwitchOnThumbColor.Value;

				onSwitch.ValueChanged += delegate
				{
					Equalizer.Shared.Active = onSwitch.On;
					updateImages();
				};
				//onLabel = new UILabel(new CGRect(0,0,100,22)){Text = "On".Translate(), BackgroundColor = UIColor.Clear};
				this.AddSubview(onSwitch);
				//this.AddSubview(onLabel);


				init();

				combobox = new UIComboBox
				{
					Items = Equalizer.Shared.Presets.ToArray(),
					DisplayMember = "Name",
					ViewForPicker = Parent,
					TextColor = textColor,
//					Font =Style.Current.Equalizer.PresetFont.Value,
					TextAlignment = UITextAlignment.Right
				};


				combobox.ValueChanged += delegate { SetPreset((EqualizerPreset) combobox.SelectedItem); };
				this.AddSubview(combobox);
				SetCombobox();

				toolbar = new UIToolbar();
//				toolbar.SetBackgroundImage(Images.ToolbarBackground.Value, UIToolbarPosition.Any, UIBarMetrics.Default);
				toolbar.SetItems(new UIBarButtonItem[]
				{
					new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 10},
					new UIBarButtonItem(Images.GetEditIcon(25), UIBarButtonItemStyle.Plain, (sender, args) => Rename()),

					new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
					new UIBarButtonItem(Images.GetCopyIcon(25), UIBarButtonItemStyle.Plain, (sender, args) => Duplicate()), 
						
					new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
					new UIBarButtonItem(Images.GetDeleteIcon(25), UIBarButtonItemStyle.Plain, (sender, args) => Delete()),

					new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
					new UIBarButtonItem(Images.GetUndoImage(25), UIBarButtonItemStyle.Plain, (sender, args) => Undo()),

					new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 10},
				}, false);

				this.AddSubview(toolbar);
			}

			public void ApplyStyle()
			{
				var style = this.GetStyle();
				BackgroundColor = style.BackgroundColor;
				toolbar.BarStyle = style.BarStyle;
				combobox.TextColor = style.MainTextColor;
				labels.ForEach(x => x.TextColor = style.SubTextColor);
				active.StyleAsMainText();
				preset.StyleAsMainText();
				combobox.TitleLabel.StyleAsMainText();
			}

			public async void Rename()
			{
				try
				{
					var currentPreset = EqualizerManager.Shared.GetCurrent();
					var newName = await PopupManager.Shared.GetTextInput(Strings.RenamePreset, currentPreset.Name);
					if (string.IsNullOrWhiteSpace(newName))
					{
						App.ShowAlert(Strings.RenameError, Strings.InvalidName);
						return;
					}
					currentPreset.Name = newName;
					currentPreset.Save();
					EqualizerManager.Shared.ReloadPresets();
				}
				catch (TaskCanceledException ex)
				{
					Console.WriteLine(ex);
				}
			}

			public async void Duplicate()
			{
				try
				{
					var currentPreset = EqualizerManager.Shared.GetCurrent();
					var newName = await PopupManager.Shared.GetTextInput(Strings.CopyPreset, currentPreset.Name + Strings.Copy);
					currentPreset.Clone();
					currentPreset.Name = newName;
					currentPreset.Save();
					EqualizerManager.Shared.ReloadPresets();
					SetPreset(currentPreset);
				}
				catch (TaskCanceledException ex)
				{
					
				}
			}

			public void Undo()
			{
				var currentPreset = EqualizerManager.Shared.GetCurrent();
				var alert = new UIAlertView(Strings.AreYouSure, $"{Strings.Reset} {currentPreset.Name}", null, Strings.No, Strings.Yes);
				alert.Clicked += (sender, args) =>
				{
					if (args.ButtonIndex != 1)
						return;
					EqualizerManager.Shared.Reset(currentPreset);
					SetPreset(currentPreset);
					return;
				};
				alert.Show();
			}

			public void Delete()
			{
				var currentPreset = EqualizerManager.Shared.GetCurrent();
				var alert = new UIAlertView(Strings.AreYouSure, $"{Strings.Delete} {currentPreset.Name}", null, Strings.No, Strings.Yes);
				alert.Clicked += (sender, args) =>
				{
					if (args.ButtonIndex != 1)
						return;
					EqualizerManager.Shared.Delete(currentPreset);
					return;
				};
				alert.Show();
			}

			public async void AddPreset()
			{

				try
				{
					var newName = await PopupManager.Shared.GetTextInput(Strings.NewPreset, "");
					EqualizerManager.Shared.AddPreset(newName);
				}
				catch (TaskCanceledException ex)
				{

				}
			}


			public void SetCombobox()
			{
				combobox.SelectedItem = Equalizer.Shared.CurrentPreset;
				SetPreset((EqualizerPreset) combobox.SelectedItem);
			}

			const float EqSliderMaxHeight = 250f;

			public override void LayoutSubviews()
			{
				var width = this.Bounds.Width/(Sliders.Count + 1);
				var padding = width/2;
				var right = Bounds.Width - sidePadding;

				var leftPadding = this.GetSafeArea().Left;
				var frame = onSwitch.Frame;
				frame.X = right - frame.Width;
				frame.Y = this.Parent.NavigationController.NavigationBar.Frame.Bottom + topItemsPadding;
				onSwitch.Frame = frame;

				active.Center = new CGPoint(active.Frame.Width/2 + sidePadding/2 + leftPadding, onSwitch.Center.Y);

				var fullwidth = Bounds.Width;
				line1.Frame = new CGRect(0, frame.Bottom + topItemsPadding/2, fullwidth, 1);

				frame = combobox.Frame;
				frame.X = right - frame.Width;
				frame.Y = onSwitch.Frame.Bottom + topItemsPadding;
				combobox.Frame = frame;

				preset.Center = new CGPoint(preset.Frame.Width/2 + sidePadding/2 + leftPadding, combobox.Center.Y);

				line2.Frame = new CGRect(0, frame.Bottom + topItemsPadding/2, fullwidth, 1);

				const float BottomBarHeight = 34;
				toolbar.Frame = new CGRect(0, Bounds.Height - NowPlayingViewController.Current.GetCurrentTopHeight() - BottomBarHeight, Bounds.Width,
					BottomBarHeight);

				var sliderTop = line2.Frame.Bottom;
				var available = toolbar.Frame.Top - sliderTop - (topItemsPadding*2) - 25;

				nfloat height = 0;
				//if (Util.IsIphone)
				height = NMath.Min(available, EqSliderMaxHeight);
				//else
				//	height = this.Bounds.Height - 65;
				sliderH = sliderTop + ((available - height)/2) + topItemsPadding;
				for (int i = 0; i < Sliders.Count; i++)
				{
					var x = width*i + padding;
					var slider = Sliders[i];
					var label = labels[i];
					slider.Frame = new CGRect(x, sliderH, width, height);
					label.Frame = new CGRect(x, slider.Frame.Bottom, width, 25);
				}

				//combobox.Frame = combobox.Frame.SetLocation (xOffset + padding, height + 75);
				//SetNeedsDisplay();
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
						Sliders[i].SetValue((float) preset.Values[i].Value, true);
						EqualizerManager.Shared.SetGain((int) Sliders[i].Tag, Sliders[i].Value);
					}
					shouldSave = true;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			}

			void updateImages()
			{
				//onSwitch.ThumbTintColor = Settings.EqualizerEnabled ?Style.Current.Equalizer.SwitchOnThumbColor.Value :Style.Current.Equalizer.SwitchOffThumbColor.Value;

				var image = Settings.EqualizerEnabled ? thumbImageOn : thumbImageOff;
				foreach (var slider in Sliders)
				{
					slider.SetThumbImage(image, UIControlState.Normal);
					slider.SetThumbImage(image, UIControlState.Highlighted);
					var track = Settings.EqualizerEnabled ? Images.AccentImage.Value : Images.GetSliderTrack();
					slider.SetMinTrackImage(track, UIControlState.Normal);
				}
			}

			void init()
			{
				clearSliders();
				for (int i = 0; i < Equalizer.Shared.Bands.Length; i++)
				{
					createSlider(i);
					//UpdateGain(i);
				}
				updateImages();
			}

			void clearSliders()
			{
				foreach (var slider in Sliders)
				{
					slider.RemoveFromSuperview();
					slider.ValueChanged -= HandleValueChanged;
				}
				foreach (var label in labels)
				{
					label.RemoveFromSuperview();
				}
				labels = new List<UILabel>();
				Sliders = new List<UISlider>();
			}

			UIImage thumbImageOn, thumbImageOff;
			UIColor textColor = UIColor.Black;

			void createSlider(int index)
			{
				if (thumbImageOn == null)
				{
					thumbImageOn = UIImage.FromBundle("slider-handle-rotated");
					thumbImageOff = UIImage.FromBundle("slider-handle-off-rotated");
				}
				var band = Equalizer.Shared.Bands[index];
				var slider = new UISlider() {MinValue = -1*range, MaxValue = range, Value = band.Gain};
				slider.Transform = CGAffineTransform.MakeRotation((float) Math.PI*-.5f);
				slider.ValueChanged += HandleValueChanged;
				slider.Tag = index;
				slider.MaximumTrackTintColor = UIColor.DarkGray;
				slider.MinimumTrackTintColor = UIColor.DarkGray;
				Sliders.Add(slider);
//				slider.SetMaxTrackImage(Images.EqSliderOffImage.Value, UIControlState.Normal);
				this.AddSubview(slider);

				var label = new UILabel()
				{
					Text = band.ToString(),
					TextColor = textColor,
					BackgroundColor = UIColor.Clear,
					Font = UIFont.SystemFontOfSize(10),
					TextAlignment = UITextAlignment.Center
				};
				labels.Add(label);
				this.AddSubview(label);
			}

			static float range = 12f;
			bool shouldSave = true;

			void HandleValueChanged(object sender, EventArgs e)
			{
				var slider = sender as UISlider;
				var gain = slider.Value;
				EqualizerManager.Shared.SetGain((int) slider.Tag, gain);
				if (shouldSave)
					EqualizerManager.Shared.SaveCurrent();
			}

			public override void Draw(CGRect rect)
			{
				base.Draw(rect);
				//// General Declarations
				var colorSpace = CGColorSpace.CreateDeviceRGB();
				var context = UIGraphics.GetCurrentContext();

				//				//// Color Declarations
				//				UIColor gradient2Color = UIColor.FromRGBA(0.906f, 0.910f, 0.910f, 1.000f);
				//				UIColor gradient2Color2 = UIColor.FromRGBA(0.588f, 0.600f, 0.616f, 1.000f);
				//				
				//				//// Gradient Declarations
				//				var gradient2Colors = new CGColor [] {gradient2Color.CGColor, gradient2Color2.CGColor};
				//				var gradient2Locations = new float [] {0, 1};
				//				var gradient2 = new CGGradient(colorSpace, gradient2Colors, gradient2Locations);

				//// Abstracted Attributes
				var textContent = "+ " + range;
				var text2Content = "0";
				var text3Content = "- " + range;

				//// Rectangle Drawing
				//				var rectanglePath = UIBezierPath.FromRect(rect);
				//				context.SaveState();
				//				rectanglePath.AddClip();
				//				context.DrawLinearGradient(gradient2, new CGPoint(rect.Height, 0), new CGPoint(rect.Height, rect.Height), 0);
				//				context.RestoreState();

				if (Sliders.Count == 0)
					return;

				var sliderFrame = Sliders[0].Frame;
				var thumbH = 0; //Sliders[0].CurrentThumbImage.Size.Height / 2;
				var h = (sliderFrame.Height - (thumbH*2))/8;
				var offset = sliderFrame.Y;
				var x = sliderFrame.X;
				var width = Sliders.Last().Frame.Right;
				for (int i = 0; i < 9; i++)
				{
					UIColor.Black.ColorWithAlpha(0f).SetStroke();
					var currH = (i*h) + thumbH;
					//if (i == 0)
					//{
					//	//// Text Drawing
					//	var textRect = new CGRect(0, currH + offset - 7.5f, 37, 13);
					//	textColor.SetFill();
					//	new Foundation.NSString(textContent).DrawString(textRect, UIFont.FromName(Style.Fonts.AvenirLight, 10), UILineBreakMode.WordWrap, UITextAlignment.Right);
					//	//UIColor.Black.ColorWithAlpha(.5f).SetStroke ();
					//}
					//else 
					if (i == 4)
					{
						//// Text Drawing
						//var textRect = new CGRect(0, currH + offset - 7.5f, 37, 13);
						//textColor.SetFill();
						//new Foundation.NSString(text2Content).DrawString(textRect, UIFont.FromName(Style.Fonts.AvenirLight, 10), UILineBreakMode.WordWrap, UITextAlignment.Right);
						//textColor.ColorWithAlpha(.5f).SetStroke();
//						Style.Colors.LightGray.Value.ColorWithAlpha (.1f).SetStroke ();
					}
					//else if (i == 8)
					//{
					//	//// Text Drawing
					//	var textRect = new CGRect(0, currH + offset - 7.5f, 37, 13);
					//	textColor.SetFill();
					//	new Foundation.NSString(text3Content).DrawString(textRect, UIFont.FromName(Style.Fonts.AvenirLight, 10), UILineBreakMode.WordWrap, UITextAlignment.Right);
					//	//UIColor.Black.ColorWithAlpha(.5f).SetStroke ();
					//}


					context.MoveTo(x, currH + offset);
					context.AddLineToPoint(width, currH + offset);
					context.StrokePath();
				}
			}
		}
	}
}