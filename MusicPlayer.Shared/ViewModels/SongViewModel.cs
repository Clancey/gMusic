using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Localizations;
using MusicPlayer.Models;
using SimpleDatabase;
using MusicPlayer.Data;
using MusicPlayer.Managers;

namespace MusicPlayer.ViewModels
{
	public class SongViewModel : BaseViewModel<Song>
	{
		public SongViewModel()
		{
			Title = Strings.Songs;
			GroupInfo = Database.Main.GetGroupInfo<Song>();
		}

		GroupInfo offlineGroupInfo;


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

		public bool AutoPlayOnSelect { get; set; } = true;
		public override void RowSelected(Song item)
		{
			if (AutoPlayOnSelect)
				PlayItem (item);
		}

		public async void PlayItem(Song item)
		{
			await PlaybackManager.Shared.Play(item, CurrentGroupInfo);

		}
	}
}