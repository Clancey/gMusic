using System;
using System.Collections.Generic;
using System.Text;
using Localizations;
using MusicPlayer.Cells;
using MusicPlayer.Managers;
using MusicPlayer.ViewModels;

namespace MusicPlayer.iOS.ViewControllers
{
	class DownloadViewController : BaseTableViewController
	{
		DownloadViewModel model;

		public DownloadViewController()
		{
			Title = Strings.CurrentDownloads;
		}

		public override void LoadView()
		{
			base.LoadView();
			TableView.Source = model = new DownloadViewModel();
		}

		public override void SetupEvents()
		{
			base.SetupEvents();
			NotificationManager.Shared.DownloaderStarted += SharedOnDownloaderStarted;
			model.CellFor += item => new SongDownloadCell { BindingContext = item };
		}

		void SharedOnDownloaderStarted(object sender, EventArgs eventArgs)
		{
			TableView.ReloadData();
		}

		public override void TeardownEvents()
		{
			base.TeardownEvents();
			NotificationManager.Shared.DownloaderStarted -= SharedOnDownloaderStarted;
		}
		
	}
}
