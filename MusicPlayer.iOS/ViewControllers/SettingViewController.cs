using System;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using BigTed;
using CoreGraphics;
using Foundation;
using Localizations;
using MessageUI;
using MonoTouch.Dialog;
using MusicPlayer.Api;
using MusicPlayer.Api.GoogleMusic;
using MusicPlayer.Api.iPodApi;
using MusicPlayer.Data;
using MusicPlayer.iOS.Controls;
using MusicPlayer.iOS.Helpers;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using Xamarin;
using MusicPlayer.Playback;
using Accounts;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class SettingViewController : DialogViewController
	{
		readonly SettingsSwitch lastFmElement;
		readonly SettingsElement addNewAccountElement;
		readonly SettingsSwitch twitterScrobbleElement;
		readonly StringElement songsElement;
		MFMailComposeViewController mailController;
		MenuHelpTextElement ratingMessage;
		MenuSection accountsSection;

		public SettingViewController() : base(UITableViewStyle.Plain, null)
		{
			Title = Strings.Settings;
			accountsSection = new MenuSection (Strings.Accounts){
				(addNewAccountElement = new SettingsElement(Strings.AddStreamingService,async ()=>{
					try{
						var vc = new ServicePickerViewController();
						this.PresentModalViewController(new UINavigationController(vc),true);
						var service = await vc.GetServiceTypeAsync();
						await ApiManager.Shared.CreateAndLogin(service);
						UpdateAccounts();
					}
					catch(TaskCanceledException)
					{

					}
					catch(Exception ex)
					{
						Console.WriteLine(ex);
					}
				})),
				(lastFmElement = string.IsNullOrEmpty (ApiConstants.LastFmApiKey) ? null : new SettingsSwitch("Last.FM", Settings.LastFmEnabled)),
					(twitterScrobbleElement = new SettingsSwitch(Strings.AutoTweet, Settings.TwitterEnabled)
					{
					Detail = Settings.TwitterDisplay
				}),
					new SettingsSwitch(Strings.ImportIPodMusic, Settings.IncludeIpod)
					{
						ValueUpdated = ToggleIPod
					},
					new MenuHelpTextElement(Strings.ImportIpodHint),
			};

			Root = new RootElement(Strings.Settings)
			{
				accountsSection,
				new MenuSection(Strings.Playback)
				{
					new SettingsSwitch(Strings.EnableLikeOnTheLockScreen, Settings.ThubsUpOnLockScreen)
					{
						ValueUpdated = (b => {
							Settings.ThubsUpOnLockScreen = b;
							RemoteControlHandler.SetupThumbsUp();
						})
					},
					new MenuHelpTextElement(Strings.EnableLikeHint),
					new SettingsSwitch(Strings.EnableGaplessPlayback, Settings.EnableGaplessPlayback)
					{
						ValueUpdated = (b => {
							Settings.EnableGaplessPlayback = b;
						})
					},
					new MenuHelpTextElement(Strings.EnableGapplessHint),
					new SettingsSwitch(Strings.PlayVideosWhenAvailable, Settings.PreferVideos)
					{
						ValueUpdated = (b => { Settings.PreferVideos = b; })
					},
					new MenuHelpTextElement(Strings.PlaysMusicVideoHint),
					new SettingsSwitch(Strings.PlayCleanVersionsOfSongs,Settings.FilterExplicit)
					{
						ValueUpdated = (b=> { Settings.FilterExplicit = b; })
					},
					new MenuHelpTextElement(Strings.PlayesCleanVersionOfSongsHint),
				},
				new MenuSection(Strings.Streaming)
				{
					new SettingsSwitch (Strings.DisableAllAccess, Settings.DisableAllAccess) {
						ValueUpdated = (on) => {
							Settings.DisableAllAccess = on;
						}
					},
					new MenuHelpTextElement(Strings.DisableAllAccessHint),
					(CreateQualityPicker(Strings.CellularAudioQuality,Settings.MobileStreamQuality , (q)=> Settings.MobileStreamQuality = q)),
					(CreateQualityPicker(Strings.WifiAudioQuality,Settings.WifiStreamQuality , (q)=> Settings.WifiStreamQuality = q)),
					(CreateQualityPicker(Strings.VideoQuality,Settings.VideoStreamQuality , (q)=> Settings.VideoStreamQuality = q)),
					(CreateQualityPicker(Strings.OfflineAudioQuality,Settings.DownloadStreamQuality , (q)=> Settings.DownloadStreamQuality = q)),
					new MenuHelpTextElement(Strings.QualityHints)
				},
				new MenuSection(Strings.Feedback)
				{
					new SettingsElement(Strings.SendFeedback, SendFeedback)
					{
						TextColor = iOS.Style.DefaultStyle.MainTextColor
					},
					new SettingsElement($"{Strings.PleaseRate} {AppDelegate.AppName}", RateAppStore)
					{
						TextColor = iOS.Style.DefaultStyle.MainTextColor
					},
					(ratingMessage = new MenuHelpTextElement(Strings.NobodyHasRatedYet))
				},
				new MenuSection(Strings.Settings)
				{
					CreateLanguagePicker("Language"),
					CreateThemePicker(Strings.Theme),
					new SettingsElement(Strings.ResyncDatabase, () =>
					{
						Database.Main.ResetDatabase();
						Settings.ResetApiModes();
						ApiManager.Shared.ReSync();
					}),
					new MenuHelpTextElement (Strings.ResyncDatabaseHint),
					new SettingsSwitch(Strings.DisableAutoLock,Settings.DisableAutoLock){
						ValueUpdated = (b => {
							Settings.PreferVideos = b;
							AutolockPowerWatcher.Shared.CheckStatus();
						})
					},
					new MenuHelpTextElement(Strings.DisableAutoLockHelpText),
					new SettingsElement(Strings.DownloadQueue,
						() => NavigationController.PushViewController(new DownloadViewController(), true)),
					(songsElement = new SettingsElement(Strings.SongsCount)),
					new SettingsElement(Strings.Version){
						Value = Device.AppVersion(),
					},
					new StringElement(""),
					new StringElement(""),
					new StringElement(""),
					new StringElement(""),
				}
			};
			if (lastFmElement != null) {
				lastFmElement.ValueUpdated = async b =>
				{
					if (!b)
					{
						Settings.LastFmEnabled = false;
						ScrobbleManager.Shared.LogOut();
						return;
					}
					var success = false;
					try
					{
						success = await ScrobbleManager.Shared.LoginToLastFm();
					}
					catch (TaskCanceledException ex)
					{
						lastFmElement.Value = Settings.LastFmEnabled = false;
						TableView.ReloadData();
						return;
					}
					Settings.LastFmEnabled = success;
					if (success) return;

					lastFmElement.Value = false;
					ReloadData();
					App.ShowAlert(string.Format(Strings.ErrorLoggingInto, "Last.FM"), Strings.PleaseTryAgain);
				};
			}
			twitterScrobbleElement.ValueUpdated = async b =>
			{
				if (!b)
				{
					Settings.TwitterEnabled = false;
					Settings.TwitterDisplay = "";
					Settings.TwitterAccount = "";
					twitterScrobbleElement.Detail = "";
					ScrobbleManager.Shared.LogOutOfTwitter();

					return;
				}
				var success = await ScrobbleManager.Shared.LoginToTwitter();
				if (!success)
				{
					Settings.TwitterEnabled = false;
					twitterScrobbleElement.Value = false;
					ReloadData();
					return;
				}

				Settings.TwitterEnabled = true;
				twitterScrobbleElement.Detail = Settings.TwitterDisplay;

				ReloadData();

			};
		}
		UIBarButtonItem menuButton;
		public override void LoadView()
		{
			base.LoadView();
			var style = View.GetStyle();
			View.TintColor = style.AccentColor;
			TableView.RowHeight = 20;
			TableView.TableFooterView = new UIView(new CGRect(0, 0, 320, NowPlayingViewController.TopBarHeight));
			TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			TableView.EstimatedSectionFooterHeight = 0;
			TableView.EstimatedSectionHeaderHeight = 0;
			if (NavigationController == null)
				return;
			NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes
			{
				ForegroundColor = style.AccentColor
			};
			if (NavigationController.ViewControllers.Length == 1)
			{
				menuButton = new UIBarButtonItem(Images.MenuImage, UIBarButtonItemStyle.Plain,
					(s, e) => { NotificationManager.Shared.ProcToggleMenu(); })
				{
					AccessibilityIdentifier = "menu"
				};
				NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
			}
			//if(Device.IsIos8)
			//	NavigationController.HidesBarsOnSwipe = true;
		}
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			NavigationItem.LeftBarButtonItem = BaseViewController.ShouldShowMenuButton(this) ? menuButton : null;
		}
		async void UpdateRatingMessage()
		{
			var count = await AppStore.GetRatingCount();
			if (count < 0)
				return;
			string message = "";
			if (count == 0)
			{
				message = Strings.NobodyHasRatedYet;
			}
			else if (count == 1)
			{
				message = Strings.OnlyOnePersonHasRatedThisVersion;
			}
			else if (count < 50)
			{
				string only = Strings.Only;
				message = $"{Strings.Only} {count} {Strings.PeopleHaveRatedThisVersion}";
			}
			else
			{
				message = $"{count} {Strings.PeopleHaveRatedThisVersion}";
			}
			ratingMessage.Caption = message;
			this.ReloadData();
		}

		void UpdateAccounts ()
		{
			var endIndex = accountsSection.Elements.IndexOf (addNewAccountElement);
			var elements = accountsSection.Elements.OfType<AccountCell> ().ToList();
			var newElements = new List<Element> ();
			ApiManager.Shared.CurrentProviders.ToList ().ForEach ((x) => {
				var element = elements.FirstOrDefault (e => e.Provider.Id == x.Id);
				if (element != null)
					elements.Remove (element);
				else {
					newElements.Add (new AccountCell (x, () => {
					new AlertView (Strings.Logout, Strings.LogOutConfirmation) {
							{Strings.Logout,async()=>{
									await ApiManager.Shared.LogOut (x);
									UpdateAccounts ();
								}},
							{Strings.Nevermind,null,true}
						}.Show(this);
					}));
				}
			});
			if(newElements.Count > 0)
				accountsSection.Insert (endIndex, newElements.ToArray());
			elements.ForEach (accountsSection.Remove);

		}

		void setQualityValue(SettingsElement element, StreamQuality quality)
		{
			if (element == null)
				return;
			switch (quality)
			{
				case StreamQuality.High:
					element.Value = Strings.High;
					break;
				case StreamQuality.Medium:
					element.Value = Strings.Medium;
					break;
				case StreamQuality.Low:
					element.Value = Strings.Low;
					break;
			}
			element.Reload();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			songsElement.Value = Database.Main.GetObjectCount<Song>().ToString();
            TableView.ReloadData();
			this.StyleViewController();
			UpdateRatingMessage();
			UpdateAccounts ();
			NotificationManager.Shared.StyleChanged += Shared_StyleChanged;
		}

		void Shared_StyleChanged(object sender, EventArgs e)
		{
			this.StyleViewController();
			TableView.ReloadData();
		}
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			NotificationManager.Shared.StyleChanged -= Shared_StyleChanged;
		}
		void SendFeedback()
		{
			if (mailController != null)
			{
				mailController.Dispose();
				mailController = null;
			}
			LogManager.Shared.Log("Tapped Feedback");
			if (!MFMailComposeViewController.CanSendMail)
			{
				LogManager.Shared.Log("Feedback failed: No email");
				new AlertView(Strings.PleaseSendAnEmailTo, "Support@youriisolutions.com").Show(this);
				return;
			}
			mailController = new MFMailComposeViewController();
			mailController.SetToRecipients(new[] {"support@youriisolutions.com"});
			var tintColor = UIApplication.SharedApplication.KeyWindow.TintColor;
			mailController.SetSubject(string.Format("Feedback: {0} - {1}", AppDelegate.AppName, Device.AppVersion()));
			mailController.Finished += async (object s, MFComposeResultEventArgs args) =>
			{
				if (args.Result == MFMailComposeResult.Sent)
				{
					new AlertView(Strings.ThankYou, Strings.YouShouldReceiveAReplyShortly_).Show(this);
					LogManager.Shared.Log("Feedback Sent");
				}
				else
				{
					LogManager.Shared.Log("Feedback failed", "Reason", args.Result.ToString());
				}
				await args.Controller.DismissViewControllerAsync(true);
				if (tintColor != null)
					UIApplication.SharedApplication.KeyWindow.TintColor = tintColor;
			};
			UIApplication.SharedApplication.KeyWindow.TintColor = null;
			PresentViewController(mailController, true, null);
		}

		async void RateAppStore()
		{
			LogManager.Shared.Log("Rated app");
			try
			{
				var url =
					@"itms-apps://ax.itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id=" +
					AppDelegate.AppId;
				UIApplication.SharedApplication.OpenUrl(new NSUrl(url));
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			finally
			{
				BTProgressHUD.Dismiss();
			}
		}

		async void ToggleIPod(bool on)
		{
			Settings.IncludeIpod = on;
			var ipod = ApiManager.Shared.GetMusicProvider<iPodProvider>(ServiceType.iPod);
			if (on)
				ipod.SyncDatabase();
			else
				MusicProvider.RemoveApi(ipod.Id);
		}

		SettingsElement CreateQualityPicker(string title, StreamQuality quality,Action<StreamQuality> setQuality )
		{
			SettingsElement element = null;
			element = new SettingsElement(title, () =>
			{
				new ActionSheet(title)
				{
					{
						$"{Strings.Low} {Strings.UsesLessData}", () =>
						{
							setQuality(StreamQuality.Low);
							setQualityValue(element, StreamQuality.Low);
						}
					},
					{
						Strings.Normal, () =>
						{
							setQuality(StreamQuality.Medium);
							setQualityValue(element, StreamQuality.Medium);
						}
					},
					{
						$"{Strings.High} {Strings.UsesMoreData}", () =>
						{
							setQuality(StreamQuality.High);
							setQualityValue(element, StreamQuality.High);
						}
					}
				}.Show(this, TableView);
			})
			{
				style = UITableViewCellStyle.Value1,
				Value = quality.ToString(),
			};

			return element;
		}

		SettingsElement CreateThemePicker(string title)
		{
			SettingsElement element = null;
			element = new SettingsElement(title, () =>
			{
				var sheet = new ActionSheet(Strings.Theme);
				MusicPlayer.iOS.Style.AvailableStyles.ForEach(x=> sheet.Add(x.Id, () =>
				{
					Settings.CurrentStyle = x.Id;
					element.Value = x.Id;
				}));
				sheet.Show(this, TableView);
			})
			{
				style = UITableViewCellStyle.Value1,
				Value = Settings.CurrentStyle,
			};

			return element;
		}
		CultureInfo[] cultures = new CultureInfo[]{
			new CultureInfo("en"),
			new CultureInfo("es"),
			new CultureInfo("fr"),
			new CultureInfo("it"),
			new CultureInfo("ja"),
			new CultureInfo("ko"),
			new CultureInfo("ru"),
			new CultureInfo("zh"),

		};
		SettingsElement CreateLanguagePicker(string title)
		{
			SettingsElement element = null;
			var currentCulture = string.IsNullOrWhiteSpace(Settings.LanguageOverride) ? Strings.Default : new CultureInfo(Settings.LanguageOverride).DisplayName;
			element = new SettingsElement(title, () =>
			{
				var sheet = new ActionSheet("Language");
				sheet.Add(Strings.Default, () =>
				 {
					 Settings.LanguageOverride = null;
				 });
				cultures.ForEach(x => sheet.Add(x.NativeName, () =>
				{
					Strings.Culture = x;
					Settings.LanguageOverride = x.TwoLetterISOLanguageName;
					element.Value = x.NativeName;
				}));
				sheet.Show(this, TableView);
			})
			{
				style = UITableViewCellStyle.Value1,
				Value = currentCulture,
			};

			return element;
		}
	}
}