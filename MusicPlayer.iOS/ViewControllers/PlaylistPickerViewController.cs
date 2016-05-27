using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using UIKit;
using System.Linq;
using Localizations;

namespace MusicPlayer.iOS.ViewControllers
{
	internal class PlaylistPickerViewController : BaseTableViewController
	{
		PlaylistViewModel model;

		public PlaylistPickerViewController()
		{
			model = new PlaylistViewModel();
			Title = model.Title;
			NavigationItem.LeftBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (s, e) => {
				tcs.TrySetCanceled();
				this.DismissViewControllerAsync(true);
			});
			NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, async (s, e) => {
				try {
					var title = await PopupManager.Shared.GetTextInput(Strings.PlaylistName, "", Strings.Add);
					var services = MusicManager.Shared.GetServiceTypes(FilterBy).FirstOrDefault();
					tcs.TrySetResult(new Playlist { Name = title, ServiceId = services});
					this.DismissViewControllerAsync(true);
				} catch (TaskCanceledException) {

				}
			});
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model;
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			model.ClearEvents();
		}

		public MediaItemBase FilterBy {
			get { return model.FilterBy; }
			set { model.FilterBy = value; }
		}

		readonly TaskCompletionSource<Playlist> tcs = new TaskCompletionSource<Playlist>();

		public Task<Playlist> SelectPlaylist()
		{
			return tcs.Task;
		}

		public override void SetupEvents()
		{
			NotificationManager.Shared.PlaylistsDatabaseUpdated += PlaylistDatabaseUpdated;
			model.ItemSelected += (sender, e) => {
				tcs.TrySetResult(e.Data);
				this.DismissViewControllerAsync(true);
			};
		}

		void PlaylistDatabaseUpdated(object sender, EventArgs eventArgs)
		{
			TableView.ReloadData();
		}

		public override void TeardownEvents()
		{
			NotificationManager.Shared.PlaylistsDatabaseUpdated -= PlaylistDatabaseUpdated;
		}
	}
}