#if !FORMS
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Localizations;
using MusicPlayer.Cells;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleTables;

namespace MusicPlayer.ViewModels
{
	public class AlbumDetailsViewModel : SongViewModel
	{
		Album album;

		public Album Album
		{
			set
			{
				var group = new SimpleDatabase.GroupInfo
				{
					From = "Song",
					Params = value.Id,
					Filter = "AlbumId = ?",
					OrderBy = "Disc, Track"
				};
				Title = value.Name;
				GroupInfo = group;
				album = value;
				Load();
			}
			get { return album; }
		}
		protected List<Song> Songs = new List<Song>();

		protected bool isLoading = false;

		Task<List<Song>> loadingTask;

		async Task Load()
		{
			if (Settings.DisableAllAccess && album.TrackCount > 0)
				return;
			if (loadingTask?.IsCompleted == false)
			{
				await loadingTask;
			}
			isLoading = true;
			loadingTask = MusicManager.Shared.GetOnlineTracks(album);
			Songs  = await loadingTask;
			isLoading = false;
			ReloadData();
			return;
		}

		public override int NumberOfSections()
		{
			if (isLoading || Songs.Count > 0)
				return 2;
			return base.NumberOfSections();
		}

		public override int RowsInSection(int section)
		{
			if (section == 0) return base.RowsInSection(section);
			if (isLoading)
				return 1;
			return Songs.Count;
		}

		public override Song ItemFor(int section, int row)
		{
			if (section == 1)
				return Songs.Count <= row ? null : Songs[row];
			return base.ItemFor(section, row);
		}

		public override ICell GetICell(int section, int row)
		{
			if(section == 1 && isLoading)
				return new SpinnerCell();
			return base.GetICell(section, row);
		}

		public override string HeaderForSection(int section)
		{
			if (section == 1)
				return Strings.AdditionalTracks;
			return base.HeaderForSection(section);
		}

		public override string[] SectionIndexTitles()
		{
			return new string[0];
		}

		public override void RowSelected(Song item)
		{
			if (Songs.Contains(item))
			{
				PlaybackManager.Shared.Play(item,Album, Songs);
			}
			else
				base.RowSelected(item);
		}
		public override void ClearEvents ()
		{
			base.ClearEvents ();
			loadingTask = null;
		}
	}
}
#endif