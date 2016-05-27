using System;
using System.Collections.Generic;
using System.Text;
using Localizations;
using MusicPlayer.Models;
using SimpleDatabase;
using MusicPlayer.Data;

namespace MusicPlayer.ViewModels
{
	public class AlbumViewModel : BaseViewModel<Album>
	{
		public AlbumViewModel()
		{
			Title = Strings.Albums;
			GroupInfo = Database.Main.GetGroupInfo<Album>();
			GroupInfo.Filter = " Name <> '' ";
		}

		protected GroupInfo offlineGroupInfo;

		public override GroupInfo OfflineGroupInfo
		{
			get
			{
				if (offlineGroupInfo == null)
				{
					offlineGroupInfo = GroupInfo.Clone();
					offlineGroupInfo.Filter = offlineGroupInfo.Filter + (string.IsNullOrEmpty(offlineGroupInfo.Filter) ? " " : " and ") +
											"OfflineCount > 0";
				}
				return offlineGroupInfo;
			}
			set { offlineGroupInfo = value; }
		}


		public void PlayItem(Album album)
		{
			MusicPlayer.Managers.PlaybackManager.Shared.Play (album);
		}
	}
}