using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Cells;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using Xamarin;
using SimpleTables;

namespace MusicPlayer.ViewModels
{
    class OnlineArtistDetailsViewModel : TableViewModel<MediaItemBase>
	{
		public string Title { get; set; }
		SearchResults Results;
	    Artist _artist;

	    public bool IsSearching { get; set; }

	    public Artist Artist
	    {
		    get { return _artist; }
		    set
		    {
			    _artist = value;
			    Load();
		    }
	    }

	    async Task Load()
	    {
		    try
		    {
			    Results = await MusicManager.Shared.GetArtistDetails(Artist);
				ReloadData();
		    }
		    catch (Exception ex)
		    {
				LogManager.Shared.Report(ex);
		    }
			ReloadData();
		    IsSearching = false;
	    }

	    public OnlineArtistDetailsViewModel()
		{
		}

		#region implemented abstract members of TableViewModel

		public override int RowsInSection(int index)
		{
			var section = GetSection(index);
			return GetRowsInSection(section);
		}

		public override int NumberOfSections()
		{
			if (Results == null || IsSearching)
				return 1;
			var sections = GetSections().Length;
			return sections;
		}

		public override string HeaderForSection(int section)
		{
			return GetSection(section);
		}

		public int GetRowsInSection(string section)
		{
			switch (section)
			{
				case "Searching":
					return 1;
				case "Related Artists":
					return Results?.Artist.Count ?? 0;
				case "Albums":
					return Results?.Albums.Count ?? 0;
				case "Top Songs":
					return Results?.Songs.Count ?? 0;
				case "Radio Stations":
					return Results?.RadioStations.Count ?? 0;
				case "Playlists":
					return Results?.Playlists.Count ?? 0;
				case "Videos":
					return Results?.Videos.Count ?? 0;
			}
			Console.WriteLine($"Unknown Section in search: {section}");
			return 0;
		}

		public override string[] SectionIndexTitles()
		{
			return GetSections().Select(x => "\u25CF").ToArray();
		}

		public string GetSection(int index)
		{
			var sections = GetSections();
			return index >= sections.Length ? "" : sections[index];

		}
		public string[] GetSections()
		{
			if (IsSearching || Results == null)
			{
				return new[] { "Searching" };
			}
			if (Results == null)
				return new string[] { "" };
			var sections = new List<string>();

			if (Results.Albums.Count > 0)
				sections.Add("Albums");
			if (Results.Songs.Count > 0)
				sections.Add("Top Songs");
			if (Results.Artist.Count > 0)
				sections.Add("Related Artists");
			if (Results.RadioStations.Count > 0)
				sections.Add("Radio Stations");
			if (Results.Playlists.Count > 0)
				sections.Add("Playlists");
			if (Results.Videos.Count > 0)
				sections.Add("Videos");

			return sections.ToArray();
		}

		public override ICell GetICell(int section, int row)
		{
			if (IsSearching || Results == null)
				return new SpinnerCell();
			return base.GetICell (section, row);
		}

		public override MediaItemBase ItemFor(int section, int row)
		{
			var sectionName = GetSection(section);
			switch (sectionName)
			{
				case "Searching":
					return null;
				case "Related Artists":
					return Results?.Artist[row];
				case "Albums":
					return Results?.Albums[row];
				case "Top Songs":
					return Results?.Songs[row];
				case "Radio Stations":
					return Results?.RadioStations[row];
				case "Playlists":
					return Results?.Playlists[row];
				case "Videos":
					return Results?.Videos[row];
			}
			Console.WriteLine($"Unknown Section in search: {section}");
			return null;
		}

		#endregion
	}
}
