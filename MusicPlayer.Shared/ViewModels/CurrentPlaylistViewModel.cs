#if !FORMS
using System;
using System.Collections.Generic;
using System.Text;
using Localizations;
using MusicPlayer.Managers;
using MusicPlayer.Models;

namespace MusicPlayer.ViewModels
{
	partial class CurrentPlaylistViewModel : BaseViewModel<Song>
	{
		#region implemented abstract members of BaseViewModel

		public override SimpleDatabase.GroupInfo OfflineGroupInfo
		{
			get { return GroupInfo; }
			set { throw new NotImplementedException(); }
		}

		#endregion

		public CurrentPlaylistViewModel()
		{
			Title = Strings.CurrentPlaylist;
		}

		#region implemented abstract members of TableViewModel

		public override int RowsInSection(int section)
		{
			return PlaybackManager.Shared.CurrentPlaylistSongCount;
		}

		public override int NumberOfSections()
		{
			return 1;
		}

		public override string HeaderForSection(int section)
		{
			return "";
		}

		public override string[] SectionIndexTitles()
		{
			return new string[]
			{
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
				"",
			};
		}

		public override Song ItemFor(int section, int row)
		{
			try
			{
				var song = PlaybackManager.Shared.GetSong(row);
				return song;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return new Song();
			}
		}

		#endregion
	}
}
#endif