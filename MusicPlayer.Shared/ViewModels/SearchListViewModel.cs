using System;
using MusicPlayer.Models;
using SimpleTables;
using System.Collections.Generic;
using System.Linq;
using MusicPlayer.Api;
using MusicPlayer.Cells;
using MusicPlayer.Managers;
using Localizations;

namespace MusicPlayer
{
	public class SearchListViewModel : TableViewModel<MediaItemBase>
	{
		public ServiceType ServiceType { get; set; }
		public string Title { get; set; }
		SearchResults results;

		public bool IsSearching { get; set; }

		public SearchResults Results
		{
			get { return results; }
			set
			{
				results = value;
				IsSearching = results == null;
				ReloadData();
			}
		}

		public SearchListViewModel()
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
			if (Results == null)
				return 1;
			var sections = GetSections().Length;
			return sections;
		}

		public override string HeaderForSection(int section)
		{
			switch (GetSection(section))
			{
				case "Searching":
					return Strings.Searching;
				case "Artist":
					return Strings.Artists;
				case "Albums":
					return Strings.Albums;
				case "Songs":
					return Strings.Songs;
				case "Radio Stations":
					return Strings.RadioStations;
				case "Playlists":
					return Strings.Playlists;
				case "Videos":
					return Strings.Videos;
			}
			return "";
		}

		public int GetRowsInSection(string section)
		{
			switch (section)
			{
				case "Searching":
					return 1;
                case "Artist":
					return Results?.Artist.Count ?? 0;
				case "Albums":
					return Results?.Albums.Count ?? 0;
				case "Songs":
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
			return GetSections().Select(x=> "\u25CF").ToArray();
		}

		public string GetSection(int index)
		{
			var sections = GetSections();
			return index >= sections.Length ? "" : sections[index];

		}
		public string[] GetSections()
		{
			if (IsSearching)
			{
				return new [] {"Searching"};
			}
			if (Results == null)
				return new string[] {""};
			var sections = new List<string>();
			if(Results.Artist.Count > 0)
				sections.Add("Artist");

			if(Results.Albums.Count > 0)
				sections.Add("Albums");
			if(Results.Songs.Count > 0)
				sections.Add("Songs");
			if(Results.RadioStations.Count > 0)
				sections.Add("Radio Stations");
			if(Results.Playlists.Count > 0)
				sections.Add("Playlists");
			if(Results.Videos.Count > 0)
				sections.Add("Videos");

			return sections.ToArray();
		}

		public override ICell GetICell(int section, int row)
		{
			if(IsSearching)
				return new SpinnerCell();
			return base.GetICell (section, row);
		}

	public override MediaItemBase ItemFor(int section, int row)
		{
			try
			{
				var sectionName = GetSection(section);
				switch (sectionName)
				{
					case "Searching":
						return null;
					case "Artist":
						return Results?.Artist[row];
					case "Albums":
						return Results?.Albums[row];
					case "Songs":
						return Results?.Songs[row];
					case "Radio Stations":
						return Results?.RadioStations[row];
					case "Playlists":
						return Results?.Playlists[row];
					case "Videos":
						return Results?.Videos[row];
				}
			}
			catch(Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			Console.WriteLine($"Unknown Section in search: {section}");
			return null;
		}

		#endregion
	}
}

