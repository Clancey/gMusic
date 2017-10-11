using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Net.Http.Headers;
using SQLite;
using Newtonsoft.Json;

namespace MusicPlayer.Api.GoogleMusic
{
	public class RootApiObject
	{
		public Error Error { get; set; }
		public string Id { get; set; }
	}

	public class RootTrackApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public string Kind { get; set; }
			public string NextPageToken { get; set; }
			public DataClass Data { get; set; }

			public class DataClass
			{
				public List<SongItem> Items { get; set; }
			}
		}
	}

	[Serializable]
	public class SongItem
	{
		public string Kind { get; set; }
		public string Id { get; set; }
		public string ClientId { get; set; }

		[JsonProperty("CreationTimestamp")]
		public long DateCreated { get; set; }

		public long LastModifiedTimestamp { get; set; }
		public long RecentTimestamp { get; set; }
		public bool Deleted { get; set; }
		public string Title { get; set; }
		public string IndexCharacter { get; set; }
		public string TitleNorm { get; set; }
		public string Artist { get; set; }
		public string ArtistNorm { get; set; }
		public string Composer { get; set; }
		public string Album { get; set; }
		public string AlbumArtist { get; set; }

		public string RealArtist
		{
			get { return string.IsNullOrEmpty(AlbumArtist) ? Artist : AlbumArtist; }
		}

		public string AlbumArtistNorm { get; set; }
		public int Year { get; set; }
		public string Comment { get; set; }

		[JsonProperty("TrackNumber")]
		public int Track { get; set; }

		public string Genre { get; set; }

		[JsonProperty("DurationMillis")]
		public double Duration { get; set; }

		public int BeatsPerMinute { get; set; }
		public string AlbumArtUrl { get; set; }

		[Ignore]
		public List<AlbumArtRef> AlbumArtRef { get; set; } = new List<AlbumArtRef>();

		public string ArtistImageBaseUrl { get; set; }

		[Ignore]
		public List<ArtistArtRef> ArtistArtRef { get; set; } = new List<ArtistArtRef>();

		public int PlayCount { get; set; }
		public int TotalTrackCount { get; set; }

		[JsonProperty("DiscNumber")]
		public int Disc { get; set; }

		[JsonProperty("TotalDiscCount")]
		public int TotalDiscs { get; set; }

		public int Rating { get; set; }
		public double EstimatedSize { get; set; }

		[JsonProperty("TrackType")]
		public int Type { get; set; }

		public string StoreId { get; set; }

		[JsonProperty("AlbumId")]
		public string MatchedAlbumId { get; set; }

		public string ArtistMatchedId { get; set; }
		List<string> artistId;

		[Ignore]
		[JsonProperty("ArtistId")]
		public List<string> ArtistIdList
		{
			get { return artistId; }
			set
			{
				artistId = value;
				if (value != null && value.Count > 0)
					ArtistMatchedId = value.FirstOrDefault();
			}
		}

		public bool IsAllAccess
		{
			get { return Type == 7; }
			set { }
		}

		public string Nid { get; set; }

		public PrimaryVideo PrimaryVideo { get; set; }
	}

	public class Thumbnail
	{
		public string Url { get; set; }
		public int Width { get; set; }
		public int Height { get; set; }
	}

	public class PrimaryVideo
	{
		public string Kind { get; set; }
		public string Id { get; set; }
		public List<Thumbnail> Thumbnails { get; set; }
	}

	public class AlbumArtRef
	{
		public string Url { get; set; }
	}

	public class ArtistArtRef
	{
		public string Url { get; set; }
	}

	public class ImageClass
	{
		public string Url { get; set; }
		public int AspectRatio { get; set; } = 1;
	}


	public class Datum
	{
		public string Domain { get; set; }
		public string Reason { get; set; }
		public string Message { get; set; }
	}

	public class Error
	{
		public int Code { get; set; }
		public string Message { get; set; }
		public List<Datum> Data { get; set; } = new List<Datum>();
		public override string ToString()
		{
			var data = string.Join(" , ", Data);
			return $"Error: {Message} {Code} - {data}";
		}
	}

	public class RootSearchApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public string Kind { get; set; }
			public List<Entry> Entries { get; set; }

			public class Entry
			{
				public string Type { get; set; }
				public ArtistClass Artist { get; set; }
				public double Score { get; set; }
				public AlbumClass Album { get; set; }
				public SongItem Track { get; set; }
			}
		}

		public class ArtistClass
		{
			public string Kind { get; set; }
			public string Name { get; set; }
			public string ArtistArtRef { get; set; }
			public string ArtistId { get; set; }
		}

		public class AlbumClass
		{
			public string Kind { get; set; }
			public string Name { get; set; }
			public string AlbumArtist { get; set; }
			public string AlbumArtRef { get; set; }
			public string AlbumId { get; set; }
			public string Artist { get; set; }
			public List<string> ArtistId { get; set; }
			public int Year { get; set; }
		}
	}

	public class RootRadioStationsApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public string Kind { get; set; }
			public Data Data { get; set; }
			public string NextPageToken { get; set; }
		}

		public class Data
		{
			public List<Station> Items { get; set; }
		}
	}

	public class Station
	{
		public string Kind { get; set; }
		public string Id { get; set; }
		public string ClientId { get; set; }
		public long LastModifiedTimestamp { get; set; }
		public long RecentTimestamp { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public SearchResultResponse.ResultClass.Seed seed { get; set; }
		public bool InLibrary { get; set; }
		public List<SongItem> Tracks { get; set; }
		public List<ImageClass> ImageUrls { get; set; }
		public List<ImageClass> CompositeArtRefs { get; set; }
		public bool Deleted { get; set; }
	}

	public class RootRadioStationsTracksApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public DataClass Data { get; set; }

			public class DataClass
			{
				public long CurrentTimestampMillis { get; set; }
				public List<Station> Stations { get; set; }

				public class Station
				{
					public string kind { get; set; }
					public string id { get; set; }
					public string clientId { get; set; }
					public string lastModifiedTimestamp { get; set; }
					public string recentTimestamp { get; set; }
					public string name { get; set; }
					public SearchResultResponse.ResultClass.Seed seed { get; set; }
					public List<SearchResultResponse.ResultClass.Seed> stationSeeds { get; set; }
					public List<SongItem> tracks { get; set; }
					//public List<ImageUrl> imageUrls { get; set; }
					//public List<CompositeArtRef> compositeArtRefs { get; set; }
					public bool deleted { get; set; }
					public bool inLibrary { get; set; }
				}
			}
		}
	}


	public class RootCreateRadioStationsTracksApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public string currentTimestampMillis { get; set; }

			[JsonProperty("mutate_response")]
			public List<MutateResponseClass> MutateResponse { get; set; }

			public class MutateResponseClass
			{
				public string id { get; set; }
				public string client_id { get; set; }
				public string response_code { get; set; }
				public Station Station { get; set; }
			}
		}
	}

	public class RootOnlinePlaylistApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class Item
		{
			public string Kind { get; set; }
			public string Id { get; set; }
			public long CreationTimestamp { get; set; }
			public long LastModifiedTimestamp { get; set; }
			public long RecentTimestamp { get; set; }
			public bool Deleted { get; set; }
			public string Name { get; set; }
			public string Type { get; set; }
			public string ShareToken { get; set; }
			public string OwnerName { get; set; }
			public string OwnerProfilePhotoUrl { get; set; }
			public bool AccessControlled { get; set; }
			public string ClientId { get; set; }
			//Playlist tracks only
			public string PlaylistId { get; set; }
			public string TrackId { get; set; }
			public long AbsolutePosition { get; set; }
			public string Source { get; set; }

			public SongItem Track { get; set; }
		}

		public class Entry
		{
			public string responseCode { get; set; }
			public string shareToken { get; set; }
			public List<Item> playlistEntry { get; set; } = new List<Item>();
		}

		public class ResultClass
		{
			public string kind { get; set; }
			public List<Entry> entries { get; set; }
		}
	}
	public class RootPlaylistApiObject : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public string Kind { get; set; }
			public DataClass Data { get; set; }
			public string NextPageToken { get; set; }

			public class DataClass
			{
				public List<Item> Items { get; set; }

				public class Item
				{
					public string Kind { get; set; }
					public string Id { get; set; }
					public long CreationTimestamp { get; set; }
					public long LastModifiedTimestamp { get; set; }
					public long RecentTimestamp { get; set; }
					public bool Deleted { get; set; }
					public string Name { get; set; }
					public string Type { get; set; }
					public string ShareToken { get; set; }
					public string OwnerName { get; set; }
					public string OwnerProfilePhotoUrl { get; set; }
					public bool AccessControlled { get; set; }
					public string ClientId { get; set; }
					//Playlist tracks only
					public string PlaylistId { get; set; }
					public string TrackId { get; set; }
					public long AbsolutePosition { get; set; }
					public string Source { get; set; }

					public SongItem Track { get; set; }
				}
			}
		}
	}


	public class RootMutateApiObject : RootApiObject
	{
		public Result result { get; set; }


		public class MutateResponse
		{
			public string id { get; set; }
			public string client_id { get; set; }
			public string response_code { get; set; }
		}

		public class Result
		{
			public List<MutateResponse> mutate_response { get; set; }
		}
	}


	public class OAuthRootObject : RootApiObject
	{
		public string Token { get; set; }

		public long ExpiresIn { get; set; }

		public string Uri { get; set; }
	}


	public class ArtistDataResultObject : RootApiObject
	{
		public ArtistDataResult result { get; set; }


		public class ArtistDataResult
		{
			public ArtistDataResult()
			{
				albums = new List<Album>();
				topTracks = new List<TopTrack>();
				related_artists = new List<RelatedArtist>();
			}

			public string kind { get; set; }
			public string name { get; set; }
			public string artistArtRef { get; set; }
			public string artistId { get; set; }
			public string artistBio { get; set; }
			public List<Album> albums { get; set; }
			public List<TopTrack> topTracks { get; set; }
			public List<RelatedArtist> related_artists { get; set; }
			public int total_albums { get; set; }

			public class Album
			{
				public Album()
				{
					artistId = new List<string>();
				}

				public string kind { get; set; }
				public string name { get; set; }
				public string albumArtist { get; set; }
				public string albumArtRef { get; set; }
				public string albumId { get; set; }
				public string artist { get; set; }
				public List<string> artistId { get; set; }
				public int year { get; set; }
			}

			public class RelatedArtist
			{
				public string kind { get; set; }
				public string name { get; set; }
				public string artistArtRef { get; set; }
				public string artistId { get; set; }
			}

			public class TopTrack
			{
				public TopTrack()
				{
					artistId = new List<string>();
					albumArtRef = new List<AlbumArtRef>();
				}

				public string kind { get; set; }
				public string title { get; set; }
				public string artist { get; set; }
				public string composer { get; set; }
				public string album { get; set; }
				public string albumArtist { get; set; }
				public int year { get; set; }
				public int trackNumber { get; set; }
				public string genre { get; set; }
				public double durationMillis { get; set; }
				public List<AlbumArtRef> albumArtRef { get; set; }
				public int playCount { get; set; }
				public int discNumber { get; set; }
				public int totalDiscCount { get; set; }
				public int rating { get; set; }
				public string estimatedSize { get; set; }
				public int trackType { get; set; }
				public string storeId { get; set; }
				public string albumId { get; set; }
				public List<string> artistId { get; set; }
				public string nid { get; set; }
				public bool trackAvailableForSubscription { get; set; }
				public bool trackAvailableForPurchase { get; set; }
				public bool albumAvailableForPurchase { get; set; }
				public string contentType { get; set; }
				public PrimaryVideo primaryVideo { get; set; }
			}
		}
	}
	[Serializable]
	public class AlbumDataResultObject : RootApiObject
	{
		public AlbumDataResult result { get; set; }


		public class AlbumDataResult
		{
			public AlbumDataResult()
			{
				Tracks = new List<SongItem>();
			}

			public List<SongItem> Tracks { get; set; } 
			
		}
	}

	public class RecordPlaybackResponse : RootApiObject
	{
		public ResultClass result {get;set;}
		public class ResultClass
		{
			public List< EventResult> eventResults { get; set; }
		}
		public class EventResult
		{
			public string code { get; set; }
			public bool Success => code == "OK";
			public string eventId {get; set; }
		}
	}

	public class ArtistDetailsResponse : RootApiObject
	{
		public ResultClass Result { get; set; }

		public class ResultClass
		{
			public string kind { get; set; }
			public string name { get; set; }
			public string artistId { get; set; }
			public List<SongItem> topTracks { get; set; }
			public List<SearchResultResponse.ResultClass.Artist> related_artists { get; set; }
			public List<SearchResultResponse.ResultClass.Album> albums { get; set; }
			public int total_albums { get; set; }
		}
	}
	public class SearchResultResponse : RootApiObject
	{
		public ResultClass Result {get;set;}

		public class ResultClass
		{
			public string kind { get; set; }
			public List<Entry> entries { get; set; }
			public List<string> clusterOrder { get; set; }

			public class Entry
			{
				public string type { get; set; }
				public Artist artist { get; set; }
				public double score { get; set; }
				public bool best_result { get; set; }
				public bool navigational_result { get; set; }
				public Album album { get; set; }
				public SongItem track { get; set; }
				public Situation situation { get; set; }
				public YoutubeVideo youtube_video { get; set; }
				public Playlist playlist { get; set; }
				public Station station { get; set; }
			}
			public class ArtistArtRef
			{
				public string kind { get; set; }
				public string url { get; set; }
				public string aspectRatio { get; set; }
				public bool autogen { get; set; }
			}

			public class ArtistBioAttribution
			{
				public string kind { get; set; }
				public string source_title { get; set; }
				public string source_url { get; set; }
				public string license_title { get; set; }
				public string license_url { get; set; }
			}

			public class Artist
			{
				public string kind { get; set; }
				public string name { get; set; }
				public string artistArtRef { get; set; }
				public List<ArtistArtRef> artistArtRefs { get; set; }
				public string artistId { get; set; }
				public ArtistBioAttribution artist_bio_attribution { get; set; }
			}

			public class DescriptionAttribution
			{
				public string kind { get; set; }
				public string source_title { get; set; }
				public string source_url { get; set; }
				public string license_title { get; set; }
				public string license_url { get; set; }
			}

			public class Album
			{
				public string kind { get; set; }
				public string name { get; set; }
				public string albumArtist { get; set; }
				public string albumArtRef { get; set; }
				public string albumId { get; set; }
				public string artist { get; set; }
				public List<string> artistId { get; set; }
				public int year { get; set; }
				public DescriptionAttribution description_attribution { get; set; }
			}

			public class AlbumArtRef
			{
				public string kind { get; set; }
				public string url { get; set; }
				public string aspectRatio { get; set; }
				public bool autogen { get; set; }
			}

			public class Thumbnail
			{
				public string url { get; set; }
				public int width { get; set; }
				public int height { get; set; }
			}

			public class PrimaryVideo
			{
				public string kind { get; set; }
				public string id { get; set; }
				public List<Thumbnail> thumbnails { get; set; }
			}
			public class Situation
			{
				public string id { get; set; }
				public string title { get; set; }
				public string description { get; set; }
				public string imageUrl { get; set; }
				public string wideImageUrl { get; set; }
			}
			public class YoutubeVideo
			{
				public string kind { get; set; }
				public string id { get; set; }
				public string title { get; set; }
				public List<Thumbnail> thumbnails { get; set; }
			}

			public class Playlist
			{
				public string kind { get; set; }
				public string name { get; set; }
				public List<AlbumArtRef> albumArtRef { get; set; }
				public string type { get; set; }
				public string shareToken { get; set; }
				public string ownerName { get; set; }
				public string ownerProfilePhotoUrl { get; set; }
				public string description { get; set; }
			}

			public class Seed
			{
				public string kind { get; set; }
				public string artistId { get; set; }
				public string seedType { get; set; }
				public string curatedStationId { get; set; }
				public string trackId { get; set; }

				public string trackLockerId { get; set; }

				public string albumId { get; set; }

				public string Id
				{
					get
					{
						if (!string.IsNullOrWhiteSpace(artistId))
							return artistId;
						if (!string.IsNullOrWhiteSpace(trackId))
							return trackId;
						if (!string.IsNullOrWhiteSpace(curatedStationId))
							return curatedStationId;

						if (!string.IsNullOrWhiteSpace(trackLockerId))
							return trackLockerId;

						if (!string.IsNullOrWhiteSpace(albumId))
							return albumId;

						return "";
					}
				}
			}

			public class ImageUrl
			{
				public string kind { get; set; }
				public string url { get; set; }
				public int aspectRatio { get; set; }
				public bool autogen { get; set; }
			}

			public class CompositeArtRef
			{
				public string kind { get; set; }
				public string url { get; set; }
				public int aspectRatio { get; set; }
			}

			public class Station
			{
				public string kind { get; set; }
				public string name { get; set; }
				public Seed seed { get; set; }
				public List<Seed> stationSeeds { get; set; }
				public List<ImageUrl> imageUrls { get; set; }
				public List<CompositeArtRef> compositeArtRefs { get; set; }
				public string description { get; set; }
			}
		}
	}
}