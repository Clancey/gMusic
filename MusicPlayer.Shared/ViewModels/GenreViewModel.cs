using System;
using Localizations;
using MusicPlayer.Data;
using MusicPlayer.Models;
using SimpleDatabase;

namespace MusicPlayer.ViewModels
{
	public class GenreViewModel : BaseViewModel<Genre>
	{
		public GenreViewModel()
		{
			Title = Strings.Genres;
			GroupInfo = DefaultGroupInfo;
		}

		public static GroupInfo DefaultGroupInfo = new GroupInfo()
		{
			Filter = "Name <> ''",
			OrderBy = "NameNorm",
			GroupBy = "IndexCharacter"
		};

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

		public Action<Artist> GoToArtist { get; set; }
		public Action<Genre, GroupInfo> GoToArtistList { get; set; }

		public override void RowSelected(Genre item)
		{
			//			if (GenreSelected != null) {
			//				GenreSelected (item);
			//				return;
			//			}
			var groupInfo = new GroupInfo()
			{
				Filter = "Id in (select distinct ArtistId from song where Genre = ?)",
				Params = item.Id,
				OrderBy = "NameNorm"
			};
			var offlineGroupInfo2 = groupInfo.Clone();
			offlineGroupInfo2.Filter = offlineGroupInfo2.Filter + " and OfflineCount > 0";

			var artistCount =
				Database.Main.GetDistinctObjectCount<Artist>(Settings.ShowOfflineOnly ? offlineGroupInfo2 : groupInfo, "Id");
			if (artistCount == 1)
			{
				groupInfo = new GroupInfo() {Filter = "Genre = ?", Params = item.Id};
				offlineGroupInfo2 = groupInfo.Clone();
				offlineGroupInfo2.Filter = offlineGroupInfo2.Filter + " and IsLocal = 1";

				var song = Database.Main.ObjectForRow<Song>(Settings.ShowOfflineOnly ? offlineGroupInfo2 : groupInfo, 0, 0);
				var artist = Database.Main.GetObject<Artist>(song.ArtistId);
				if (artist != null && GoToArtist != null)
				{
					GoToArtist(artist);
					return;
				}
			}
			else if (GoToArtistList != null)
			{
				groupInfo = new GroupInfo()
				{
					From = "Artist",
					Filter = "Id in (select distinct ArtistId from song where genre = ?)",
					Params = item.Id,
					OrderBy = "NameNorm"
				};
				GoToArtistList(item, groupInfo);
			}
			base.RowSelected(item);
		}


		public override void ClearEvents()
		{
			base.ClearEvents();
			GoToArtistList = null;
			GoToArtist = null;
		}
	}
}