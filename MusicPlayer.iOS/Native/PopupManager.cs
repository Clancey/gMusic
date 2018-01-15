using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CoreGraphics;
using MusicPlayer.Data;
using MusicPlayer.iOS.Controls;
using MusicPlayer.iOS.ViewControllers;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;
using System.Linq;
using MusicPlayer.Api;
using Localizations;

namespace MusicPlayer.iOS
{
	public class PopupManager : ManagerBase<PopupManager>
	{
		public void Show(MediaItemBase song, UIView view)
		{
			var controller = CreateController(song);

			PresentController(controller, view);
		}

		public void ShowNowPlaying(UIView view)
		{
			var song = Database.Main.GetObject<Song, TempSong>(Settings.CurrentSong);
			if (string.IsNullOrWhiteSpace(song?.Id))
				return;
			var controller = new ActionSheet(song.Name, song.DetailText);

			if (ShouldShowPlayVideoButton(song))
			{
				controller.Add("Play Video", () =>
				{
					PlaybackManager.Shared.PlayNow(song, true);
				});
			}
			AddNavigateTo(controller, song);
			AddPlaylistButtons(controller, song);
			AddShuffleButton(controller, song);
			AddRadioStationButton(controller, song);
			AddToLibrary(controller, song);
			AddRemoveFromLibrary(controller, song);
			AddOfflineButtons(controller, song);
			controller.Add("Cancel", null, true);

			PresentController(controller, view);
		}

		static void PresentController(ActionSheet controller, UIView fromView)
		{
			var current = GetCurrentViewController();
			if (current == null)
				return;
			controller.Show(current, fromView);
		}

		static UIViewController GetCurrentViewController()
		{
			var window = UIKit.UIApplication.SharedApplication.KeyWindow;
			var root = window.RootViewController;
			if (root == null) return null;
			var current = root;
			while (current.PresentedViewController != null)
			{
				current = current.PresentedViewController;
			}
			return current;
		}

		static void PresentController(UIViewController controller)
		{
			var current = GetCurrentViewController();
			current?.PresentViewControllerAsync(controller, true);
		}

		public ActionSheet CreateController(MediaItemBase song)
		{
			var controller = new ActionSheet(song.Name, song.DetailText);

			AddPlaybackButtons(controller, song);
			AddShuffleButton(controller, song);
			AddPlaylistButtons(controller, song);
			AddRadioStationButton(controller, song);
			AddToLibrary(controller,song);
			AddRemoveFromLibrary(controller,song);
			AddOfflineButtons(controller, song);
			controller.Add("Cancel", null, true);
			return controller;
		}


		public static void AddPlaylistButtons(ActionSheet controller, MediaItemBase item)
		{
			if (!ShouldShowAddPlaylist(item))
				return;
			controller.Add("Add to Playlist", async () =>
			{
				try
				{
					var playlistPicker = new PlaylistPickerViewController {FilterBy = item, ShouldHideMenu = true};
					PresentController(new UINavigationController(playlistPicker));
					var playlist = await playlistPicker.SelectPlaylist();
					Console.WriteLine(playlist);
					using (new Spinner("Adding to Playlist"))
					{
						var success = await MusicManager.Shared.AddToPlaylist(item, playlist);
						if (!success)
							App.ShowAlert("Error adding to playlist", Strings.PleaseTryAgain);
					}
				}
				catch (TaskCanceledException canceledException)
				{
					Console.WriteLine("Canceled");
				}
				catch (Exception ex)
				{
					App.ShowAlert("Error adding to playlist", Strings.PleaseTryAgain);
					LogManager.Shared.Report(ex);
				}
			});
		}

		static void AddRadioStationButton(ActionSheet controller, MediaItemBase item)
		{
			//TODO check if we can add station to the item;
			if (!SouldShowStartRadio(item))
				return;
			controller.Add("Start Radio Station", async () =>
			{
				using (new Spinner(Strings.CreatingStation))
				{
					try
					{
						var station = await MusicManager.Shared.CreateRadioStation(item);
						if (station != null)
							PlaybackManager.Shared.Play(station);
						else
							App.ShowAlert(Strings.RenameError, "There was an error creating the radio station");
					}
					catch (Exception ex)
					{
						LogManager.Shared.Report(ex);
					}
				}
			});
		}

		static void AddPlaybackButtons(ActionSheet controller, MediaItemBase item)
		{
			if (!ShouldShowQueueButtons(item))
				return;


			if (ShouldShowPlayNowButton(item))
			{
				controller.Add("Play", () => { PlaybackManager.Shared.Play(item); });
			}

			if (ShouldShowPlayVideoButton(item))
			{
				controller.Add("Play Video", () =>
				{
					PlaybackManager.Shared.PlayNow(item as Song, true);
				});
			}
			controller.Add("Play Next", () => { PlaybackManager.Shared.PlayNext(item); });

			controller.Add("Add to Queue", () => { PlaybackManager.Shared.AddtoQueue(item); });
		}

		static void AddShuffleButton(ActionSheet controller, MediaItemBase item)
		{
			if (!ShouldShowShuffle(item))
				return;
			controller.Add("Shuffle", () =>
			{
				Settings.ShuffleSongs = true;
				PlaybackManager.Shared.Play(item);
			});
		}
		static ServiceType[] AllowedOffLineTypes = {
			ServiceType.Amazon,
			ServiceType.DropBox,
			ServiceType.Google,
			ServiceType.YouTube
		};
		static void AddOfflineButtons(ActionSheet controller, MediaItemBase item)
		{
			if (item is RadioStation || item is AutoPlaylist)
				return;
			var song = item as Song;
			if(song != null)
			{
				
				bool canOffline = song.ServiceTypes.Any(x => AllowedOffLineTypes.Contains(x));
				if (!canOffline)
					return;
			}
			var localTitle = item.ShouldBeLocal() || item.OfflineCount > 0 ? "Remove from Device" : "Download to Device";
			controller.Add(localTitle, () => { OfflineManager.Shared.ToggleOffline(item); });
		}

		static void AddNavigateTo(ActionSheet controller, MediaItemBase item)
		{
			var song = item as Song;
			if (song == null)
				return;
			if (!string.IsNullOrWhiteSpace(song.AlbumId))
				controller.Add("Go to Album", () => { NotificationManager.Shared.ProcGoToAlbum(song.AlbumId); });

			controller.Add("Go to Artist", () => { NotificationManager.Shared.ProcGoToArtist(song.ArtistId); });
		}

		static void AddToLibrary(ActionSheet controller, MediaItemBase item)
		{
			if(!ShouldShowAddToLibrary(item))
				return;
			controller.Add("Add to Library",async () =>
			{
				var onlineSong = item as OnlineSong;
				if (onlineSong?.TrackData?.ServiceType == ServiceType.YouTube)
				{
					//Prompt for meta data
					var editor = new SongTagEditor { Song = onlineSong };
					await GetCurrentViewController().PresentViewControllerAsync(new UINavigationController(editor), true);
					var s = await editor.GetValues();
					if (!s)
						return;
				}
				var success = await MusicManager.Shared.AddToLibrary(item);
				if(!success)
					App.ShowAlert(Strings.RenameError,Strings.PleaseTryAgain);
			});
		}

		static void AddRemoveFromLibrary(ActionSheet controller, MediaItemBase item)
		{
			if (!ShouldRemoveFromLibrary(item))
				return;
			controller.Add("Remove from Library", async () =>
			{
				var success = await MusicManager.Shared.Delete(item);
				//TODO: Show Alert
				Console.WriteLine(success);
			});
		}
		public async Task<string> GetTextInput(string title, string defaultText, string buttonText = "Ok")
		{
			var current = GetCurrentViewController();
			if (current == null)
				return null;
			var textInput = new TextInputAlert(title, defaultText, buttonText);
			return await textInput.GetText(current);
		}

		public async Task<Tuple<string, string>> GetCredentials(string title, string details = "", string url = "")
		{
			var current = GetCurrentViewController();
			if (current == null)
				throw new Exception("window.RootViewController is not set");
			var loginEntry = new LoginEntryAlert(title, details);
			var result = await loginEntry.GetCredentials(current);
			if (string.IsNullOrWhiteSpace(result.Item1) || string.IsNullOrWhiteSpace(result.Item2))
			{
				result = await GetCredentials(title, "Invalid Credentials");
			}
			return result;
		}


		static bool SouldShowStartRadio(MediaItemBase item)
		{
			if (item is Playlist)
				return false;
			if (item is RadioStation)
				return false;
			if (item is Genre)
				return false;
			var song = item as OnlineSong;
			if (song != null) {
				var service = ApiManager.Shared.GetMusicProvider (song.TrackData.ServiceId);
				var hadRadio = service.Capabilities.Contains (MediaProviderCapabilities.Radio);
				return hadRadio;
			}
			return true;
		}

		static bool ShouldShowAddPlaylist(MediaItemBase item)
		{
			if (item is Playlist)
				return false;
			if (item is RadioStation)
				return false;
			var song = item as OnlineSong;
			if (song != null) {
				var service = ApiManager.Shared.GetMusicProvider (song.TrackData.ServiceId);
				return service.Capabilities.Contains (MediaProviderCapabilities.Playlists);
			}
			return true;
		}

		static bool ShouldShowPlayNowButton(MediaItemBase item)
		{
			if (item is Song)
				return false;
			if (item is RadioStation)
				return false;
			return true;
		}

		static bool ShouldShowQueueButtons(MediaItemBase item)
		{
			if (item is Playlist)
				return false;
			if (item is RadioStation)
				return false;
			return true;
		}

		static bool ShouldShowPlayVideoButton(MediaItemBase item)
		{
			var song = item as Song;
			if (song == null)
			{
				return false;
			}

			return song.HasVideo;
		}

		static bool ShouldRemoveFromLibrary(MediaItemBase item)
		{
			var radio = item as RadioStation;
			if (radio != null)
			{
				return radio.IsIncluded;
			}
			return false;
		}
		static bool ShouldShowAddToLibrary(MediaItemBase item)
		{
			var song = item as OnlineSong;
			if (song != null) {
				var service = ApiManager.Shared.GetMusicProvider (song.TrackData.ServiceId);
				return service.ServiceType == ServiceType.Google || service.ServiceType == ServiceType.YouTube;
			}
			if (item is OnlineSong || item is TempSong)
				return true;
			if (item is OnlineAlbum)
				return true;
			if (item is OnlinePlaylist)
				return true;
			if (item is OnlineRadioStation)
				return true;

			var radio = item as RadioStation;
			if (radio != null)
			{
				return !radio.IsIncluded;
			}
			return false;
		}

		static bool ShouldShowShuffle(MediaItemBase item)
		{
			if (item is Song)
				return false;
			if (item is RadioStation)
				return false;
			return true;
		}
    }
}