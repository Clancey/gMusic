using System;
using UIKit;
using Foundation;
using System.Threading.Tasks;
using Localizations;
using MusicPlayer.Models;
using MusicPlayer.Playback;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS
{
	public class EqualizerPickerViewController : UITableViewController
	{
		Source source;
		public Action<EqualizerPreset> PresetSelected { get; set; }

		public EqualizerPickerViewController()
		{
			this.Title = Strings.EqualizerPresets;
			this.NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel,
				(sender, args) => { this.DismissViewController(true, null); });
			//.View.BackgroundColor = Style.Current.ScreensDefaults.Background.Value;
			//this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			//TableView.RegisterClassForCellReuse (typeof(Source.EqCell), Source.EqCell.Key);
		}

		object currentItem;

		public object CurrentItem
		{
			get { return currentItem; }
			set
			{
				currentItem = value;
				source.ShowDefault = currentItem != null;
				SetSourc();
			}
		}

		public override void LoadView()
		{
			base.LoadView();
			var style = View.GetStyle();
			this.View.TintColor = this.NavigationItem.LeftBarButtonItem.TintColor = style.AccentColor;
			TableView.Source = source = new Source();
		}

		async void SetSourc()
		{
#if iPod
			source.Default = await Equalizer.GetDefault (currentItem) ?? Equalizer.Shared.CurrentPreset ;
			source.Current = currentItem == null ? Equalizer.Shared.CurrentPreset : await Equalizer.GetPreset (currentItem);
			#else
			source.Current = Equalizer.Shared.CurrentPreset;
#endif
			TableView.ReloadData();
		}

#if PIONEER
		UIGestureRecognizer panGesture;
#endif

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			source.Refresh();
			source.PresetSelected = preset => { setPreset(preset); };
#if PIONEER
			panGesture = this.TableView.AddGestures();
#endif
			TableView.ReloadData();
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			source.PresetSelected = null;
#if PIONEER
			if (panGesture != null) {
				this.View.RemoveGestureRecognizer (panGesture);
				panGesture = null;
			}
#endif
		}

		async Task setPreset(EqualizerPreset preset)
		{
			try
			{
				if (CurrentItem == null)
					Equalizer.Shared.CurrentPreset = preset;
				else
				{
#if iPod
					Equalizer.SaveEq (currentItem, preset);
					if (CurrentItem == AudioPlayer.Shared.CurrentSong)
					await Equalizer.Shared.ApplyEqualizer ();
#else
					Equalizer.Shared.ApplyEqualizer();
#endif
				}
				SetSourc();
				if (PresetSelected != null)
					PresetSelected(preset);
#if PIONEER
				if(PPioneerManager.SharedPioneerManager.Connected)
					NavigationController.PopViewController(true);
				else
#endif
				this.NavigationController.DismissViewController(true, null);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			//TableView.ReloadData ();
		}


		class Source : UITableViewSource
		{
			EqualizerPreset[] items = new EqualizerPreset[0];

			public void Refresh()
			{
				items = Equalizer.Shared.Presets.ToArray();
			}

			public EqualizerPreset Default { get; set; }
			public EqualizerPreset Current { get; set; }
			public bool ShowDefault { get; set; }

			#region implemented abstract members of UITableViewSource

			public override nint RowsInSection(UITableView tableview, nint section)
			{
				return section == 0 && ShowDefault ? 1 : items.Length;
			}

			public override UITableViewCell GetCell(UITableView tableView, Foundation.NSIndexPath indexPath)
			{
				var cell = tableView.DequeueReusableCell(EqCell.Key) as EqCell ?? new EqCell();
				cell.IsDefault = indexPath.Section == 0 && ShowDefault;
				cell.Preset = indexPath.Section == 0 && ShowDefault ? Default : items[indexPath.Row];
				cell.Accessory = (Current == null && indexPath.Section == 0 && ShowDefault) ||
								(Current != null && Current == items[indexPath.Row])
					? UITableViewCellAccessory.Checkmark
					: UITableViewCellAccessory.None;
				return cell;
			}

			public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
			{
				PresetSelected?.Invoke(ShowDefault && indexPath.Section == 0 ? null : items[indexPath.Row]);
			}

			public override nint NumberOfSections(UITableView tableView)
			{
				return ShowDefault ? 2 : 1;
			}

			public Action<EqualizerPreset> PresetSelected { get; set; }

			#endregion

			public class EqCell : UITableViewCell
			{
				const string key = "eqCell";

				public static string Key
				{
					get { return key; }
				}

				public EqCell() : base(UITableViewCellStyle.Subtitle, Key)
				{
					this.BackgroundColor = UIColor.Clear;
					//this.TextLabel.TextColor =  this.DetailTextLabel.TextColor = Style.Current.ScreensDefaults.Cell.MainFontColor.Value;
					//this.TextLabel.Font = Style.Current.ScreensDefaults.Cell.MainFont.Value;
					//this.TextLabel.BackgroundColor = UIColor.Clear;
					//this.TintColor = Style.Current.ScreensDefaults.Cell.MainFontColor.Value;
				}

				EqualizerPreset preset;

				public EqualizerPreset Preset
				{
					get { return preset; }
					set
					{
						preset = value;
						update();
					}
				}

				void update()
				{
					if (preset == null)
						return;

					this.TextLabel.Text = isDefault ? Strings.Default : preset.Name;
					this.DetailTextLabel.Text = isDefault ? preset.Name : "";
				}

				bool isDefault;

				public bool IsDefault
				{
					get { return isDefault; }
					set
					{
						isDefault = value;
						update();
					}
				}
			}
		}
	}
}