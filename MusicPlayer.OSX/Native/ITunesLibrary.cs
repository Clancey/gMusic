using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace ITunesLibraryParser
{

	public class ItunesTrack
	{
		public int TrackId { get; set; }

		public string Name { get; set; }

		public string Artist { get; set; }

		public string AlbumArtist { get; set; }

		public string Composer { get; set; }

		public string Album { get; set; }

		public string Genre { get; set; }

		public string Kind { get; set; }

		public long Size { get; set; }

		public string PlayingTime { get; set; }

		public int? TrackNumber { get; set; }

		public int? Year { get; set; }

		public DateTime? DateModified { get; set; }

		public DateTime? DateAdded { get; set; }

		public int? BitRate { get; set; }

		public int? SampleRate { get; set; }

		public int? PlayCount { get; set; }

		public DateTime? PlayDate { get; set; }

		public bool PartOfCompilation { get; set; }

		public string Location { get; set; }

		public int? DiscNumber {get;set;}

		public int? DiscCount {get;set;}
		public string TrackType {get;set;}
		public override string ToString ()
		{
			return string.Format ("Artist: {0} - Track: {1} - Album: {2}", Artist, Name, Album);
		}

		public ItunesTrack Copy ()
		{
			return this.MemberwiseClone () as ItunesTrack;
		}
	}

	public interface IITunesLibrary
	{
		IEnumerable<ItunesTrack> Parse (string xmlFileLocation);
	}

	public class ITunesLibrary : IITunesLibrary
	{
		public IEnumerable<ItunesTrack> Parse (string fileLocation)
		{
			var trackElements = LoadTrackElements (fileLocation);
			return trackElements.Select (CreateTrack);
		}

		private static IEnumerable<XElement> LoadTrackElements (string fileLocation)
		{
			return from x in XDocument.Load (fileLocation).Descendants ("dict").Descendants ("dict").Descendants ("dict")
			       where x.Descendants ("key").Count () > 1
			       select x;
		}

		private ItunesTrack CreateTrack (XElement trackElement)
		{
			return new ItunesTrack {
				TrackId = Int32.Parse (ParseStringValue (trackElement, "Track ID")),
				Name = ParseStringValue (trackElement, "Name"),
				Artist = ParseStringValue (trackElement, "Artist"),
				AlbumArtist = ParseStringValue (trackElement, "AlbumArtist"),
				Composer = ParseStringValue (trackElement, "Composer"),
				Album = ParseStringValue (trackElement, "Album"),
				Genre = ParseStringValue (trackElement, "Genre"),
				Kind = ParseStringValue (trackElement, "Kind"),
				Size = ParseLongValue (trackElement, "Size"),
				PlayingTime = ConvertMillisecondsToFormattedMinutesAndSeconds ((ParseLongValue (trackElement, "Total Time"))),
				TrackNumber = ParseNullableIntValue (trackElement, "Track Number"),
				Year = ParseNullableIntValue (trackElement, "Year"),
				DateModified = ParseNullableDateValue (trackElement, "Date Modified"),
				DateAdded = ParseNullableDateValue (trackElement, "Date Added"),
				BitRate = ParseNullableIntValue (trackElement, "Bit Rate"),
				SampleRate = ParseNullableIntValue (trackElement, "Sample Rate"),
				PlayDate = ParseNullableDateValue (trackElement, "Play Date UTC"),
				PlayCount = ParseNullableIntValue (trackElement, "Play Count"),
				PartOfCompilation = ParseBoolean (trackElement, "Compilation"),
				Location = ParseStringValue(trackElement,"Location"),
				DiscCount = ParseNullableIntValue(trackElement,"Disc Count"),
				DiscNumber = ParseNullableIntValue(trackElement,"Disc Number"),
				TrackType = ParseStringValue(trackElement,"Track Type"),
			};
		}

		bool ParseBoolean (XElement track, string keyValue)
		{
			return (from keyNode in track.Descendants ("key")
			        where keyNode.Value == keyValue
			        select (keyNode.NextNode as XElement).Name).FirstOrDefault () == "true";
		}

		string ParseStringValue (XElement track, string keyValue)
		{
			return (from key in track.Descendants ("key")
			        where key.Value == keyValue
			        select (key.NextNode as XElement).Value).FirstOrDefault ();
		}

		long ParseLongValue (XElement track, string keyValue)
		{
			var stringValue = ParseStringValue (track, keyValue);
			return string.IsNullOrWhiteSpace (stringValue) ? 0 : Int64.Parse (stringValue);
		}

		int? ParseNullableIntValue (XElement track, string keyValue)
		{
			var stringValue = ParseStringValue (track, keyValue);
			return String.IsNullOrEmpty (stringValue) ? (int?)null : Int32.Parse (stringValue);
		}

		DateTime? ParseNullableDateValue (XElement track, string keyValue)
		{
			var stringValue = ParseStringValue (track, keyValue);
			return String.IsNullOrEmpty (stringValue) ? (DateTime?)null : DateTime.SpecifyKind (DateTime.Parse (stringValue, CultureInfo.InvariantCulture), DateTimeKind.Utc).ToLocalTime ();
		}

		static string ConvertMillisecondsToFormattedMinutesAndSeconds (long milliseconds)
		{
			var totalSeconds = Math.Round (TimeSpan.FromMilliseconds (milliseconds).TotalSeconds);
			var minutes = (int)(totalSeconds / 60);
			var seconds = (int)(totalSeconds - (minutes * 60));
			var timespan = new TimeSpan (0, minutes, seconds);
			return timespan.ToString ("m\\:ss");
		}
	}
}
