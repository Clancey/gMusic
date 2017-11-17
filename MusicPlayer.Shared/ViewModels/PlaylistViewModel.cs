using System;
using Localizations;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using SimpleDatabase;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using System.Linq;

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

		#region auto playlists
		//Due to having Auto playlists, we are incrementing each Section by 1

		public override int RowsInSection(int section)
		{
			if (Database.Main == null)
				return 0;
			if (section == 0)
				return AutoPlaylist.AutoPlaylists.Length;
			return base.RowsInSection(section - 1);
		}

		public override int NumberOfSections()
		{
			if (Database.Main == null)
				return 0;
			int sections = base.NumberOfSections();
			return sections + 1;
		}

		public override string HeaderForSection(int section)
		{
			if (Database.Main == null)
				return "";
			if (section == 0)
				return "Auto Playlists";
			return base.HeaderForSection(section + 1);
		}

		public override string[] SectionIndexTitles()
		{
			if (Database.Main == null)
				return null;
			var items = base.SectionIndexTitles();
			return items.Prepend("*").ToArray();
		}

		public override Playlist ItemFor(int section, int row)
		{
			if (section == 0)
			{
				return AutoPlaylist.AutoPlaylists[row];
			}
			return base.ItemFor(section + 1, row);
		}

  #endregion
	}
}