using System;
using Localizations;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using SimpleDatabase;
using MusicPlayer.Data;
using MusicPlayer.Managers;

namespace MusicPlayer.ViewModels
{
	public partial class PlaylistViewModel : BaseViewModel<Playlist>
	{
		public PlaylistViewModel()
		{
			Title = Strings.Playlists;
			GroupInfo = Database.Main.GetGroupInfo<Playlist>();
		}

		GroupInfo offlineGroupInfo;
		MediaItemBase filterBy;


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

		public MediaItemBase FilterBy
		{
			get { return filterBy; }
			set
			{
				filterBy = value;
				var tracks = MusicManager.Shared.GetServiceTypes(value);
				var services = string.Join("','", tracks);
				var filter = $" ServiceId in ('{services}')";
				var groupInfo = Database.Main.GetGroupInfo<Playlist>().Clone();
				groupInfo.AddFilter(filter);
				GroupInfo = groupInfo;
			}
		}
	}
}