using System;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using Newtonsoft.Json;
using MusicPlayer.Data;

namespace MusicPlayer.Api.GoogleMusic
{
	public class GoogleMusicApiRequest
	{
		static int count = 1;

		public GoogleMusicApiRequest()
		{
			jsonrpc = "2.0";
			id = $"gtl_{count++}";
			apiVersion = "v2.4";
			//parameters = new Params ();
		}

		public string jsonrpc { get; set; }
		public string method { get; set; }
		public string id { get; set; }

		[Newtonsoft.Json.JsonProperty("params")]
		public Params parameters { get; set; } = new Params();

		public string apiVersion { get; set; }

		public class MutateEditParams : Params
		{
			public MutateRequest request { get; set; }

			public class MutateRequest
			{
				public List<Mutation> mutations { get; set; } = new List<Mutation>();

				public class Mutation
				{
				}

				public class PlaylistMutation : Mutation
				{
					[Newtonsoft.Json.JsonProperty("delete", NullValueHandling = NullValueHandling.Ignore)]
					public string delete { get; set; }

					[Newtonsoft.Json.JsonProperty("update", NullValueHandling = NullValueHandling.Ignore)]
					public PlaylistUpdateMutation update { get; set; }

					[Newtonsoft.Json.JsonProperty("create", NullValueHandling = NullValueHandling.Ignore)]
					public PlaylistCreateMutation Create { get; set; }
					

					public class PlaylistUpdateMutation : PlaylistCreateMutation
					{
						public PlaylistUpdateMutation()
						{
							Console.WriteLine("Created");
							Type = null;
						}

						public int source { get; set; } = 2;

						public string trackId { get; set; }
						
						[Newtonsoft.Json.JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
						public string id { get; set; }

						[Newtonsoft.Json.JsonProperty("followingEntryId", NullValueHandling = NullValueHandling.Ignore)]
						public string followingEntryId { get; set; }

						public string playlistId { get; set; }

						[Newtonsoft.Json.JsonProperty("precedingEntryId", NullValueHandling = NullValueHandling.Ignore)]
						public string precedingEntryId { get; set; }

						public int relativePositionIdType { get; set; } = 1;
					}

					public class PlaylistCreateMutation
					{
						[Newtonsoft.Json.JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
						public string ClientId {get; set; }

						[Newtonsoft.Json.JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
						public string Name {get; set; }

						[Newtonsoft.Json.JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
						public string Description { get; set; }

						[Newtonsoft.Json.JsonProperty("shareToken", NullValueHandling = NullValueHandling.Ignore)]
						public string ShareToken { get; set; }

						[Newtonsoft.Json.JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
						public string Type { get; set; } = "USER_GENERATED";
					}
					
				}
				


				public class TrackMutation : Mutation
				{
					[Newtonsoft.Json.JsonProperty("delete", NullValueHandling = NullValueHandling.Ignore)]
					public string delete { get; set; }

					[Newtonsoft.Json.JsonProperty("create", NullValueHandling = NullValueHandling.Ignore)]
					public TrackCreateMutation Create { get; set; }

					
					public class TrackCreateMutation
					{
						[Newtonsoft.Json.JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
						public string ClientId { get; set; }

						[Newtonsoft.Json.JsonProperty("nid", NullValueHandling = NullValueHandling.Ignore)]
						public string Nid { get; set; }

						[Newtonsoft.Json.JsonProperty("trackType", NullValueHandling = NullValueHandling.Ignore)]
						public int TrackType { get; set; } = 8;
					}

				}
			}
		}

		public class RadioMutateEditParams : Params
		{
			public enum SeedTypes
			{
				Locker = 1,
				TrackId = 2,
				Artist = 3,
				Album = 4,
				CurratedRadioStation = 9,
			}
			public List<Mutation> mutations { get; set; } = new List<Mutation>();

			public class Mutation
			{
			}

			public class RadioStationMutation : Mutation
			{
				[Newtonsoft.Json.JsonProperty("delete", NullValueHandling = NullValueHandling.Ignore)]
				public string delete { get; set; }

				[Newtonsoft.Json.JsonProperty("update", NullValueHandling = NullValueHandling.Ignore)]
				public RadioStationsUpdate update { get; set; }


				public class RadioStationsUpdate
				{
					public string id { get; set; }

					[JsonProperty("inLibrary", NullValueHandling = NullValueHandling.Ignore)]
					public string InLibrary { get; set; }

					[JsonProperty("lastModifiedTimestamp", NullValueHandling = NullValueHandling.Ignore)]
					public long LastModifiedTimestamp { get; set; }

					[JsonProperty("recentTimestamp", NullValueHandling = NullValueHandling.Ignore)]
					public long RecentTimestamp { get; set; }

					[JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
					public string Name { get; set; }

					[JsonProperty("seed", NullValueHandling = NullValueHandling.Ignore)]
					public CreateRadioStationMutation.CreateOrGetClass.SeedClass Seed { get; set; }

					[JsonProperty("seeds", NullValueHandling = NullValueHandling.Ignore)]
					public RadioMutateEditParams.CreateRadioStationMutation.CreateOrGetClass.SeedClass[] Seeds { get; set; }
				}
			}
			public class CreateRadioStationMutation : Mutation
			{
				[Newtonsoft.Json.JsonProperty("createOrGet", NullValueHandling = NullValueHandling.Ignore)]
				public CreateOrGetClass CreateOrGet { get; set; }

				[JsonProperty("includeFeed")]
				public bool IncludeFeed { get; set; } = true;

				[JsonProperty("numEntries")]
				public int NumEntries { get; set; } = 25;


				[Newtonsoft.Json.JsonProperty("params")]
				public RadioStationParams Paramaters { get; set; }


				public class CreateOrGetClass
				{
					[JsonProperty("name")]
					public string Name { get; set; }

					[JsonProperty("seed")]
					public SeedClass Seed { get; set; }

					public class SeedClass
					{
		
						[JsonProperty("seedType")]
						public SeedTypes SeedType { get; set; }

						[Newtonsoft.Json.JsonProperty("albumId", NullValueHandling = NullValueHandling.Ignore)]
						public string AlbumId { get; set; }

						[Newtonsoft.Json.JsonProperty("artistId", NullValueHandling = NullValueHandling.Ignore)]
						public string ArtistId { get; set; }

						[Newtonsoft.Json.JsonProperty("trackId", NullValueHandling = NullValueHandling.Ignore)]
						public string TrackId { get; set; }

						[Newtonsoft.Json.JsonProperty("trackLockerId", NullValueHandling = NullValueHandling.Ignore)]
						public string TrackLockerId { get; set; }

						[Newtonsoft.Json.JsonProperty("curatedStationId", NullValueHandling = NullValueHandling.Ignore)]
						public string CuratedStationId { get; set; }

						[JsonIgnore]
						public string Id
						{
							set
							{
								switch (SeedType)
								{
									case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Album:
										AlbumId = value;
										break;
									case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Artist:
										ArtistId = value;
										break;
									case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Locker:
										TrackLockerId = value;
										break;
									case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.TrackId:
										TrackId = value;
										break;
									case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.CurratedRadioStation:
										CuratedStationId = value;
										break;
								}
							}
						}
					}
				}
			}
		}

		public class RadioStationParams : Params
		{
			[Newtonsoft.Json.JsonProperty("rz", NullValueHandling = NullValueHandling.Ignore)]
			public string rz { get; set; }

			[Newtonsoft.Json.JsonProperty("stations", NullValueHandling = NullValueHandling.Ignore)] public List<StationRequest>
				stations = new List<StationRequest>();

			[Newtonsoft.Json.JsonProperty("contentFilter", NullValueHandling = NullValueHandling.Ignore)]
			public string contentFilter { get; set; }

			public class StationRequest
			{
				public int numEntries { get; set; }
				public string radioId { get; set; }
				public string[] recentlyPlayed { get; set; }
			}
		}

		public class Params
		{
			public Params()
			{
				tier = Settings.DisableAllAccess ? "fr" : "aa";
				refresh = "0";
				hl = "en";
			}

			[Newtonsoft.Json.JsonProperty("hl", NullValueHandling = NullValueHandling.Ignore)]
			public string hl { get; set; }
			[Newtonsoft.Json.JsonProperty("refresh", NullValueHandling = NullValueHandling.Ignore)]
			public string refresh { get; set; }
			[Newtonsoft.Json.JsonProperty("tier", NullValueHandling = NullValueHandling.Ignore)]
			public string tier { get; set; }

			[Newtonsoft.Json.JsonProperty("updated-min", NullValueHandling = NullValueHandling.Ignore)]
			public long? updatedMin { get; set; }

			[Newtonsoft.Json.JsonProperty("max-results", NullValueHandling = NullValueHandling.Ignore)]
			public long? maxResults { get; set; }

			[Newtonsoft.Json.JsonProperty("start-token", NullValueHandling = NullValueHandling.Ignore)]
			public string startToken { get; set; }
		}

		public class SearchParams : Params
		{
			public SearchParams()
			{
				refresh = null;
			}
			[JsonProperty("q")]
			public string Query { get;set; }

			[JsonProperty("ct")]
			public string Ct { get; set; } = "1,2,3,7,6,8,4";

			[JsonProperty("max-results")]
			public int MaxResults { get; set; } = 100;
		}

		public class ArtistParams : Params
		{
			[JsonProperty("nid")] 
			public string ArtistId { get; set; }

			[JsonProperty("num-top-tracks")]
			public int NumberOfSongs { get; set; } = 50;

			[JsonProperty("include-albums")]
            public bool IncludeAlbums { get; set; } = true;

			[JsonProperty("num-related-artists")]
			public int NumberOfArtist { get; set; } = 20;
		}

		public class ItemDetailsParams : Params
		{
			public string nid { get;set; }

			[JsonProperty("include-tracks", NullValueHandling = NullValueHandling.Ignore)]
			public bool IncludeTracks { get; set; }

			[JsonProperty("include-albums", NullValueHandling = NullValueHandling.Ignore)]
			public bool IncludeAlbums { get; set; }
			
			[JsonProperty("num-related-artist", NullValueHandling = NullValueHandling.Ignore)]
			public int? NumberOfRelatedArtists { get; set; }


			[JsonProperty("num-top-tracks", NullValueHandling = NullValueHandling.Ignore)]
			public int? NumverOfTopTracks { get; set; }
			
		}


		public class EntriesParams : Params
		{
			[JsonProperty("request", NullValueHandling = NullValueHandling.Ignore)]
			public EntryRequest Request { get; set; }
			public class EntryRequest
			{

				[JsonProperty("entries", NullValueHandling = NullValueHandling.Ignore)]
				public List<Entry> Entries { get; set; } = new List<Entry>();
			}

			public class Entry
			{
				[JsonProperty("shareToken", NullValueHandling = NullValueHandling.Ignore)]
				public string ShareToken { get; set; }

				[JsonProperty("maxResults", NullValueHandling = NullValueHandling.Ignore)]
				public int maxResults { get; set; } = 5000;

				[JsonProperty("updatedMin", NullValueHandling = NullValueHandling.Ignore)]
				public long UpdatedMin { get; set; } = 0;
			}

			[JsonProperty("includeDeleted")]
			public bool IncludeDeleted  { get; set; } 

		}

		public class NowPlayingParams : Params
		{
			public NowPlayingParams()
			{
				hl = null;
				refresh = null;
				tier = null;
			}
			public RequestClass request { get; set; }
			public class RequestClass
			{
				public RequestClass()
				{
					clientTimeMillis = DateTime.UtcNow.ToUnixTimeMs();
					requestSignals = new RequestSignals
					{
						timeZoneOffsetSecs = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalSeconds,
					};
				}

				public long clientTimeMillis { get; set; }
				public RequestSignals requestSignals { get; set; }
				public List<Event> events { get; set; }
				static int count = 1;
				public class Event
				{
					public Event()
					{
						int dividend = count++;
						string columnName = "";

						while (dividend > 0)
						{
							var modulo = (dividend - 1) % 26;
							columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
							dividend = (int)((dividend - modulo) / 26);
						}
						eventId = columnName.PadLeft(11, 'A') + "=";

					}
					public long createdTimestampMillis { get; set; }
					public Details details { get; set; }
					public string eventId { get; set; }
					public TrackId trackId { get; set; }
				}

				public class Details
				{
					[Newtonsoft.Json.JsonProperty("play", NullValueHandling = NullValueHandling.Ignore)]
					public Play play { get; set; }

					[Newtonsoft.Json.JsonProperty("rating", NullValueHandling = NullValueHandling.Ignore)]
					public SongRating rating { get; set; }
				}

				public class SongRating
				{
					public Context context { get; set; }
					public string rating { get; set; }
				}
				public class TrackId
				{
					[Newtonsoft.Json.JsonProperty("metajamCompactKey", NullValueHandling = NullValueHandling.Ignore)]
					public string metajamCompactKey { get; set; }
					[Newtonsoft.Json.JsonProperty("lockerId", NullValueHandling = NullValueHandling.Ignore)]
					public string lockerId { get; set; }
				}
				public class RequestSignals
				{
					public int timeZoneOffsetSecs { get; set; }
				}


				public class Context
				{
					[Newtonsoft.Json.JsonProperty("albumId", NullValueHandling = NullValueHandling.Ignore)]
					public AlbumId albumId { get; set; }
					[Newtonsoft.Json.JsonProperty("radioId", NullValueHandling = NullValueHandling.Ignore)]
					public RadioId radioId { get; set; }

					public class RadioId
					{
						public string radioId { get; set; }
					}

					public class AlbumId
					{
						[Newtonsoft.Json.JsonProperty("metajamCompactKey", NullValueHandling = NullValueHandling.Ignore)]
						public string metajamCompactKey { get; set; }
					}
				}

				public class WoodstockPlayDetails
				{
					public bool isWoodstockPlay { get; set; }
				}

				public class Play
				{
					public int trackDurationMillis { get; set; }
					public int playTimeMillis { get; set; }
					public Context context { get; set; }
					public WoodstockPlayDetails woodstockPlayDetails { get; set; } = new WoodstockPlayDetails();
				}


			}
		}
	}

	public class SettingsRootObject
	{

		public string Kind { get; set; }
		public DataItems Data { get; set; }
		public class Item
		{
			public string Kind { get; set; }
			public string Id { get; set; }
			public string FriendlyName { get; set; }
			public string Type { get; set; }
			public string LastAccessedTimeMs { get; set; }
			public bool SmartPhone { get; set; }
		}

		public class DataItems
		{
			public List<Item> items { get; set; }
		}
	}
}