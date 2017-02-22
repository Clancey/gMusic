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
			accountsSection = new MenuSection ("Accounts"){
				(addNewAccountElement = new SettingsElement("Add Streaming Service",async ()=>{
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
					(twitterScrobbleElement = new SettingsSwitch("Auto Tweet", Settings.TwitterEnabled){Detail = Settings.TwitterDisplay}),
					new SettingsSwitch("Import iPod Music", Settings.IncludeIpod)
					{
						ValueUpdated = ToggleIPod
					},
					new MenuHelpTextElement(
						"Automatically imports and plays music from your local library. This saves data and space on your phone."),
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
							RemoteControlHandler.SetupThumbsUp(); })
					},
					new MenuHelpTextElement(Strings.EnableLikeHint),
					new SettingsSwitch(Strings.EnableGaplessPlayback, Settings.ThubsUpOnLockScreen)
					{
						ValueUpdated = (b => {
							Settings.ThubsUpOnLockScreen = b;
							RemoteControlHandler.SetupThumbsUp(); })
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
					CreateThemePicker("Theme"),
					new SettingsElement(Strings.ResyncDatabase, () =>
					{
						Settings.ResetApiModes();
						ApiManager.Shared.ReSync();
					}),
					new MenuHelpTextElement (Strings.ResyncDatabaseHint),
					new SettingsElement(Strings.DownloadQueue,
						() => NavigationController.PushViewController(new DownloadViewController(), true)),
					(songsElement = new SettingsElement(Strings.SongsCount))
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
					App.ShowAlert($"{Strings.ErrorLoggingInto} Last.FM", Strings.PleaseTryAgain);
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
					return;
				}

				var store = new ACAccountStore();
				var accountType = store.FindAccountType(ACAccountType.Twitter);

				var success = false;
				var result = await store.RequestAccessAsync(accountType);
				success = result.Item1;
				if (!success)
				{
					Settings.TwitterEnabled = false;
					twitterScrobbleElement.Value = false;
					ReloadData();
					return;
				}

				var accounts = store.FindAccounts(accountType);
				if ((accounts?.Length ?? 0) == 0)
				{
					Settings.TwitterEnabled = false;
					twitterScrobbleElement.Value = false;
					ReloadData();
					return;
				}

				if (accounts?.Length == 1)
				{
					Settings.TwitterEnabled = true;
					var a = accounts[0];
					Settings.TwitterAccount = a.Identifier;
					twitterScrobbleElement.Detail = Settings.TwitterDisplay = a.UserFullName;
					ReloadData();
					return;
				}

				var sheet = new ActionSheet("Twitter");
				foreach (var a in accounts)
				{
					sheet.Add(a.Identifier, () =>
					{

						Settings.TwitterEnabled = true;
						Settings.TwitterAccount = a.Identifier;
						twitterScrobbleElement.Detail = Settings.TwitterDisplay = a.UserFullName;
						ReloadData();
					});
				}
				sheet.Add(Strings.Nevermind, null, true);
				sheet.Show(this, TableView);
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
					new AlertView ("Logout", "Are you sure you want to logout?") {
							{"Logout",async()=>{
									await ApiManager.Shared.LogOut (x);
									UpdateAccounts ();
								}},
							{"Cancel",null,true}
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
			mailController.SetSubject(string.Format("Feedback: {0} - {1}", AppDelegate.AppName, versionNumber()));
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

		string versionNumber()
		{
#if ADHOC
			return string.Format("{0}.{1}", NSBundle.MainBundle.InfoDictionary ["CFBundleShortVersionString"], NSBundle.MainBundle.InfoDictionary ["CFBundleVersion"]);
#endif
			return NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
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
				var sheet = new ActionSheet("Theme");
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
	}
}