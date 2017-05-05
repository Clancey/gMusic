using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Groove
{
	public enum GrooveNamespace
	{
		[EnumMember(Value = "music")]
		Music,

		[EnumMember(Value = "music.playlist")]
		Playlist,

		[EnumMember(Value = "music.amg")]
		Amg,

		[EnumMember(Value = "music.isrc")]
		ISRC,

		[EnumMember(Value = "music.icpn")]
		ICPN,

		[EnumMember(Value = "music.onedrive")]
		OneDrive

	}

	public enum GrooveTypes
	{
		[EnumMember(Value = "albums")]
		Albums,

		[EnumMember(Value = "artists")]
		Artists,

		[EnumMember(Value = "playlists")]
		Playlists,

		[EnumMember(Value = "tracks")]
		Tracks

	}
	public class BaseGrooveResponse
	{
		public GrooveError Error { get; set; }
	}
	public class GrooveError
	{
		public string ErrorCode { get; set; }
		public string Description { get; set; }
		public string Message { get; set; }
	}
	public class TrackActionResult : BaseGrooveResponse
	{
		[JsonProperty("InputId")]
		public string InputId { get; set; }

		[JsonProperty("Id")]
		public string Id { get; set; }
	}

	public class TrackActionResponse : BaseGrooveResponse
	{
		[JsonProperty("TrackActionResults")]
		public IList<TrackActionResult> TrackActionResults { get; set; }
	}

	public class TrackActionRequest
	{
		[JsonProperty("TrackIds")]
		public string[] TrackIds { get; set; }
	}

	public class StreamResponse : BaseGrooveResponse
	{

		[JsonProperty("Url")]
		public string Url { get; set; }

		[JsonProperty("ContentType")]
		public string ContentType { get; set; }

		[JsonProperty("ExpiresOn")]
		public DateTime ExpiresOn { get; set; }
	}


	public class Collection
	{

		[JsonProperty("Token")]
		public string Token { get; set; }

		[JsonProperty("RemainingPlaylistCount")]
		public int RemainingPlaylistCount { get; set; }

		[JsonProperty("RemainingTrackCount")]
		public int RemainingTrackCount { get; set; }
	}

	public class Subscription
	{

		[JsonProperty("Type")]
		public string Type { get; set; }

		[JsonProperty("Region")]
		public string Region { get; set; }
	}

	public class UserSubsrciption
	{

		[JsonProperty("HasSubscription")]
		public bool HasSubscription { get; set; }

		[JsonProperty("IsSubscriptionAvailableForPurchase")]
		public bool IsSubscriptionAvailableForPurchase { get; set; }

		[JsonProperty("Culture")]
		public string Culture { get; set; }

		[JsonProperty("Collection")]
		public Collection Collection { get; set; }

		[JsonProperty("Subscription")]
		public Subscription Subscription { get; set; }
	}
	public class Album
	{

		[JsonProperty("ReleaseDate")]
		public DateTime ReleaseDate { get; set; }

		[JsonProperty("Genres")]
		public IList<string> Genres { get; set; }

		[JsonProperty("Id")]
		public string Id { get; set; }

		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("ImageUrl")]
		public string ImageUrl { get; set; }

		[JsonProperty("Link")]
		public string Link { get; set; }

		[JsonProperty("Source")]
		public string Source { get; set; }
	}

	public class ArtistDetails
	{

		[JsonProperty("Id")]
		public string Id { get; set; }

		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("ImageUrl")]
		public string ImageUrl { get; set; }

		[JsonProperty("Link")]
		public string Link { get; set; }

		[JsonProperty("Source")]
		public string Source { get; set; }
	}

	public class Contributor
	{

		[JsonProperty("Role")]
		public string Role { get; set; }

		[JsonProperty("Artist")]
		public ArtistDetails Artist { get; set; }
	}

	public class TrackItem
	{

		[JsonProperty("ReleaseDate")]
		public DateTime ReleaseDate { get; set; }

		[JsonProperty("Duration")]
		public string Duration { get; set; }

		[JsonProperty("TrackNumber")]
		public int TrackNumber { get; set; }

		[JsonProperty("IsExplicit")]
		public bool IsExplicit { get; set; }

		[JsonProperty("Genres")]
		public IList<string> Genres { get; set; }

		[JsonProperty("Album")]
		public Album Album { get; set; }

		[JsonProperty("Artists")]
		public IList<Contributor> Artists { get; set; }

		[JsonProperty("Id")]
		public string Id { get; set; }

		[JsonProperty("Name")]
		public string Name { get; set; }

		[JsonProperty("ImageUrl")]
		public string ImageUrl { get; set; }

		[JsonProperty("Link")]
		public string Link { get; set; }

		[JsonProperty("Source")]
		public string Source { get; set; }

		[JsonProperty("Rights")]
		public IList<string> Rights { get; set; }
	}

	public class Tracks
	{

		[JsonProperty("Items")]
		public IList<TrackItem> Items { get; set; }
	}

	public class ContentResponse : BaseGrooveResponse
	{
		[JsonProperty("Tracks")]
		public Tracks Tracks { get; set; }
	}

}
