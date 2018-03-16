using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using MusicPlayer.Playback;
using Punchclock;
using SimpleAuth;
using MusicPlayer;
using YoutubeExtractor;
using MusicPlayer.Api;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;

namespace MusicPlayer.Api.GoogleMusic
{
	internal class GoogleMusicProvider : MusicProvider
	{
		public override MediaProviderCapabilities[] Capabilities {
			get {
				return new []{ MediaProviderCapabilities.Searchable , MediaProviderCapabilities.Radio , MediaProviderCapabilities.NewReleases , MediaProviderCapabilities.Trending , MediaProviderCapabilities.Playlists};
			}
		}

		public new GoogleMusicApi Api {
			get {  return (GoogleMusicApi) base.Api; }
		}

		public override bool RequiresAuthentication => true;
		public override ServiceType ServiceType => ServiceType.Google;

		public override string Id => Api?.Identifier;

		public GoogleMusicProvider(GoogleMusicApi api) : base(api)
		{
			CrossConnectivity.Current.ConnectivityChanged += async (sender, args) =>
			{
				if (Api.HasAuthenticated) return;
				if (!args.IsConnected) return;

				await Api.Authenticate();
				await Sync();
			};
		}

		public override string Email
		{
			get
			{
				string email = "";
				Api?.CurrentOAuthAccount?.UserData?.TryGetValue("email", out email);
				return email;
			}
		}

		public string Tier => Api?.Tier ?? "none";

		public Dictionary<string, string> UserDictionary => Api?.CurrentOAuthAccount.UserData;

		public override async Task<bool> Resync()
		{
			LastSongSync = 0;
			LastPlaylistSync = 0;
			LastPlaylistSongSync = 0;
			TotalSync = 0;
			Api.ExtraData = new GoogleMusicApiExtraData(); 
			return await SyncDatabase();

		}

		protected override async Task<bool> Sync()
		{
			try
			{
				if (Api.CurrentAccount == null)
					await Api.Authenticate();
				await Api.Identify();
				await Api.GetUserConfig();

				Decipherer.PreCache ();

				await Task.WhenAll(SyncTracks(), SyncPlaylistTracks(), SyncRadioStations());
				await SyncPlaylists();
				await SyncSharedPlaylistTracks ();
				await FinalizePlaylists(Id);
				return true;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
			}
			return false;
		}

		int TotalSync = 0;
		long LastSongSync = 0;

		async Task<bool> SyncTracks()
		{
			LastSongSync = Api.ExtraData?.LastSongSync ?? 0;
			TotalSync = 0;
			var start = DateTime.Now;
			var s = await GetTracks();
			if (TotalSync > 0)
				await FinalizeProcessing(Id);
			var finished = (DateTime.Now - start).TotalMilliseconds;
			Console.WriteLine($"Sync complete {TotalSync} in {finished} ms");
			//App.ShowAlert("Finished", $"Sync complete {TotalSync} in {finished} ms");
			return s;
		}

		async Task<bool> GetTracks(string token = null)
		{
			try
			{
				const string path = "tracks";
				var query = new Dictionary<string, string>
				{
					["max-results"] = "5000",
					["tier"] = Tier,
					["updated-min"] = (token == null ? Api.ExtraData.LastSongSync : 0).ToString(),
				};
				if (!string.IsNullOrWhiteSpace(token))
					query["start-token"] = token;

				var resp = await SyncRequestQueue.Enqueue(1, () => Api.GetLatest<RootTrackApiObject>(path,query,includeDeviceHeaders:true));
				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(resp?.NextPageToken))
					nextTask = GetTracks(resp.NextPageToken);
				var youtubeTracks = new List<FullTrackData>();
				var start = DateTime.Now;
				var tracks = resp?.Data?.Items?.Select(x =>
				{
					var id = x.Type == 0 ? x.Id : !string.IsNullOrWhiteSpace(x.StoreId) && x.StoreId.StartsWith("T") ? x.StoreId : x.Id;
                    var t = new FullTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
					{
						Deleted = x.Deleted,
						Duration = x.Duration,
						ArtistServerId = x.ArtistMatchedId,
						AlbumServerId = x.AlbumId,
						AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork {Url = a.Url}).ToList(),
						ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork {Url = a.Url}).ToList(),
						MediaType = MediaType.Audio,
						PlayCount = x.PlayCount,
						ServiceId = Api.CurrentAccount.Identifier,
						Id = id,
						ServiceExtra = x.Id == id ?  x.StoreId : x.Id,
						ServiceExtra2 = x.Type.ToString(),
						ServiceType = ServiceType.Google,
						Rating = x.Rating,
						FileExtension = "mp3",
						Disc = x.Disc,
						Track = x.Track,
						Year = x.Year,
					};

					LastSongSync = Math.Max(x.LastModifiedTimestamp, LastSongSync);
					if (x.PrimaryVideo == null)
						return t;

					var y = t.Clone();
					y.Id = x.PrimaryVideo.Id;
					y.FileExtension = "mp4";
					//y.ServiceType = ServiceType.YouTube;
					y.MediaType = MediaType.Video;
					youtubeTracks.Add(y);

					return t;
				}).ToList();
				if (tracks != null && youtubeTracks.Count > 0)
					tracks.AddRange(youtubeTracks);

				//				await Task.WhenAll (tracks?.Select (async x => await ProcessTrack (x)).ToList ());
				if ((tracks?.Count ?? 0) == 0)
					return true;
				await MusicProvider.ProcessTracks(tracks);
				TotalSync += tracks?.Count ?? 0;
				var finished = (DateTime.Now - start).TotalMilliseconds;
				Debug.WriteLine($"Batch complete {tracks.Count} in {finished} ms");
				//App.ShowAlert("Finished", $"Sync complete {tracks.Count} in {finished} ms");
				if (nextTask != null)
					return await nextTask;
				Api.ExtraData.LastSongSync = LastSongSync;
				ApiManager.Shared.SaveApi(Api);
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		#region Playlists

		long LastPlaylistSync = 0;
		long TotalPlaylistSync = 0;

		async Task<bool> SyncPlaylists()
		{
			try
			{
				TotalPlaylistSync = 0;
				LastPlaylistSync = Api.ExtraData?.LastPlaylistSync ?? 0;
				var start = DateTime.Now;
				var s = await GetPlaylists();
				var finished = (DateTime.Now - start).TotalMilliseconds;
				Console.WriteLine($"Sync complete {TotalSync} in {finished} ms");
				//App.ShowAlert("Finished", $"Sync complete {TotalSync} in {finished} ms");
				return s;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				return false;
			}
		}

		async Task<bool> GetPlaylists(string token = null)
		{
			try
			{
				const string path = "playlists";
				var query = new Dictionary<string, string>
				{
					["max-results"] = "5000",
					["tier"] = Tier,
					["updated-min"] = (token == null ? Api.ExtraData.LastPlaylistSync : 0).ToString(),
				};
				if (!string.IsNullOrWhiteSpace(token))
					query["start-token"] = token;

				var resp = await SyncRequestQueue.Enqueue(1, () => Api.GetLatest<RootPlaylistApiObject>(path, query));
				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(resp?.NextPageToken))
					nextTask = GetPlaylists(resp.NextPageToken);

				var start = DateTime.Now;

				var playlists = resp?.Data?.Items.Where(x => !string.IsNullOrWhiteSpace(x.Name)).Select(x =>
				{
					TotalPlaylistSync++;
					LastPlaylistSync = Math.Max(x.LastModifiedTimestamp, LastPlaylistSync);
					return new Playlist(x.Name)
					{
						Id = x.Id,
						ServiceType = ServiceType.Google,
						ServiceId = Api.Identifier,
						Deleted = x.Deleted,
						AccessControlled = x.AccessControlled,
						Owner = x.OwnerName,
						OwnerImage = x.OwnerProfilePhotoUrl,
						PlaylistType = x.Type,
						ShareToken = x.ShareToken,
					};
				}).ToList() ?? new List<Playlist>();


				var success = await MusicProvider.ProcessPlaylists(playlists);
				var finished = (DateTime.Now - start).TotalMilliseconds;
				Debug.WriteLine($"Batch complete {playlists?.Count} in {finished} ms");
				//App.ShowAlert("Finished", $"Sync complete {tracks.Count} in {finished} ms");
				if (nextTask != null)
					return await nextTask;
				if (!success)
					return false;
				Api.ExtraData.LastPlaylistSync = LastPlaylistSync;
				ApiManager.Shared.SaveApi(Api);
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				return false;
			}
		}

		long _lastPlaylistSong = 0;
		long LastPlaylistSongSync
		{
			get { return _lastPlaylistSong; }
			set
			{
				_lastPlaylistSong = value;
				//Debug.WriteLine(value);
			}
		}

		async Task<bool> SyncPlaylistTracks()
		{
			LastPlaylistSongSync = Api.ExtraData?.LastPlaylistSongSync ?? 0;
			TotalSync = 0;
			var start = DateTime.Now;
			var s = await GetPlaylistTracks();
			var finished = (DateTime.Now - start).TotalMilliseconds;
			Console.WriteLine($"Sync complete {TotalSync} in {finished} ms");
			return s;
		}

		async Task<bool> GetPlaylistTracks(string token = null)
		{
			try
			{
				bool success = false;
				const string path = "plentries";
				var query = new Dictionary<string, string>
				{
					["max-results"] = "5000",
					["tier"] = Tier,
					["updated-min"] = (token == null ? Api.ExtraData.LastPlaylistSongSync : 0).ToString(),
				};
				if (!string.IsNullOrWhiteSpace(token))
					query["start-token"] = token;

				var resp = await SyncRequestQueue.Enqueue(1, () => Api.GetLatest<RootPlaylistApiObject>(path, query));
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}
				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(resp?.NextPageToken))
					nextTask = GetPlaylistTracks(resp.NextPageToken);
				var start = DateTime.Now;

				var fullTracks = new List<FullPlaylistTrackData>();
				var partTracks = new List<TempPlaylistEntry>();
				if (resp?.Data?.Items?.Count == 0)
					return true;
				resp?.Data?.Items?.ForEach(s =>
				{
					TotalPlaylistSync ++;
					LastPlaylistSongSync = Math.Max(s.LastModifiedTimestamp, LastPlaylistSongSync);
					if (s.Track == null)
					{
						var ple = new TempPlaylistEntry
						{
							PlaylistEntryId = s.Id,
							TrackId = s.TrackId,
							PlaylistId = s.PlaylistId,
							SOrder = s.AbsolutePosition,
							LastUpdate = s.LastModifiedTimestamp,
						};

						partTracks.Add(ple);
						return;
					}
					var x = s.Track;
					var t = new FullPlaylistTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
					{
						ParentId = s.PlaylistId,
						Deleted = x.Deleted,
						Duration = x.Duration,
						ArtistServerId = x.ArtistMatchedId,
						AlbumServerId = x.AlbumId,
						AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork {Url = a.Url}).ToList(),
						ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork {Url = a.Url}).ToList(),
						MediaType = MediaType.Audio,
						PlayCount = x.PlayCount,
						ServiceId = Api.CurrentAccount.Identifier,
						Id = x.Type == 0 ? x.Id : !string.IsNullOrWhiteSpace(x.StoreId) && x.StoreId.StartsWith("T") ? x.StoreId : x.Id,
						ServiceType = ServiceType.Google,
						Rating = x.Rating,
						FileExtension = "mp3",
						LastUpdated = s.LastModifiedTimestamp,
						//Playlist Stuff
						PlaylistEntryId = s.Id,
						TrackId = s.TrackId,
						PlaylistId = s.PlaylistId,
						SOrder = s.AbsolutePosition,
					};
					fullTracks.Add(t);
					if (x.PrimaryVideo != null)
					{
						var y = t.Clone();
						y.Id = x.PrimaryVideo.Id;
						y.FileExtension = "mp4";
						y.MediaType = MediaType.Video;
						fullTracks.Add(y);
					}
				});

				success = await MusicProvider.ProcessPlaylistTracks(fullTracks, partTracks);
				var finished = (DateTime.Now - start).TotalMilliseconds;
				Debug.WriteLine($"Batch complete {fullTracks.Count + partTracks.Count} in {finished} ms");
				//App.ShowAlert("Finished", $"Sync complete {tracks.Count} in {finished} ms");
				if (nextTask != null)
					return await nextTask;
				if (!success)
					return false;
				Api.ExtraData.LastPlaylistSongSync = LastPlaylistSongSync;
				ApiManager.Shared.SaveApi(Api);
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		//TODO: Update based off NextToken
		public Task<bool> SyncSharedPlaylistTracks()
		{
			return Task.Run(async () =>
			{
				try
				{
					var playlists = await Database.Main.TablesAsync<Playlist>().Where(x => x.PlaylistType == "SHARED").ToListAsync();
					if(playlists.Count == 0)
						return true;
					var entries = playlists.Select(x => new GoogleMusicApiRequest.EntriesParams.Entry
					{
						ShareToken = x.ShareToken,
						UpdatedMin = x.LastSync,
					}).ToList();
					bool success = false;
					var request = new { entries = entries, includeDeleted = true };
					const string path = "plentries/shared";
					var query = new Dictionary<string, string>
					{
						["tier"] = Tier,
					};

					var resp = await SyncRequestQueue.Enqueue(1, () => Api.PostLatest<RootOnlinePlaylistApiObject>(path,request, query));


					var fullTracks = new List<FullPlaylistTrackData>();
					var partTracks = new List<TempPlaylistEntry>();
					foreach (var entry in resp?.Entries ?? new List<RootOnlinePlaylistApiObject.Entry>()) {
						var playlist = playlists.FirstOrDefault (x => x.ShareToken == entry.shareToken);
						foreach (var s in entry.playlistEntry) {
							try {
								if (s.Track == null) {
									var ple = new TempPlaylistEntry {
										PlaylistEntryId = $"{playlist.Id} - {s.TrackId}",
										TrackId = s.TrackId,
										PlaylistId = playlist.Id,
										LastUpdate = s.LastModifiedTimestamp,
										SOrder = s.AbsolutePosition,
									};
									partTracks.Add (ple);
									continue;
								}
								var x = s.Track;
								var t = new FullPlaylistTrackData (x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre) {
									ParentId = playlist.Id,
									Deleted = x.Deleted,
									Duration = x.Duration,
									ArtistServerId = x.ArtistMatchedId,
									AlbumServerId = x.AlbumId,
									AlbumArtwork = x.AlbumArtRef.Select (a => new AlbumArtwork { Url = a.Url }).ToList (),
									ArtistArtwork = x.ArtistArtRef.Select (a => new ArtistArtwork { Url = a.Url }).ToList (),
									MediaType = MediaType.Audio,
									PlayCount = x.PlayCount,
									ServiceId = Api.CurrentAccount.Identifier,
									Id = x.Type == 0 ? x.Id : !string.IsNullOrWhiteSpace (x.StoreId) && x.StoreId.StartsWith ("T") ? x.StoreId : x.Id,
									ServiceType = ServiceType.Google,
									Rating = x.Rating,
									FileExtension = "mp3",
									//Playlist Stuff
									PlaylistEntryId = $"{playlist.Id} - {s.TrackId}",
									TrackId = s.TrackId,
									PlaylistId = playlist.Id,
									SOrder = s.AbsolutePosition,
									LastUpdated = s.LastModifiedTimestamp,
								};
								fullTracks.Add (t);
							} catch (Exception e) {
								LogManager.Shared.Report (e);
							}
						}
					}
					if(fullTracks.Count == 0 && partTracks.Count == 0)
							return true;
					success = await MusicProvider.ProcessPlaylistTracks(fullTracks, partTracks);
					return success;
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
				return false;
			});
		}

		public override async Task<bool> DeletePlaylist(Playlist playlist)
		{
			try
			{
				bool success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.playlists.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams()
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest()
						{
							mutations =
							{
								new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation
								{
									delete = playlist.Id,
								},
							}
						}
					}
				};

				var resp = await Api.Post<RootMutateApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}

				SyncDatabase();
				success = resp?.result?.mutate_response?.Any(x => x.response_code.ToUpper() == "OK") ?? false;
				if (success)
				{
					Database.Main.Delete(playlist);
					Database.Main.ClearMemory<Playlist>();
				}
				if (success)
					Task.Run(async () =>
					{
						await Task.Delay(1000);
						SyncDatabase();
					});
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}


		public override async Task<bool> DeletePlaylistSong(PlaylistSong song)
		{
			try
			{
				bool success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.playlistentries.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams()
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest()
						{
							mutations =
							{
								new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation
								{
									delete = song.Id,
								},
							}
						}
					}
				};

				var resp = await Api.Post<RootMutateApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}
				SyncDatabase();
				success = resp?.result?.mutate_response?.Any(x => x.response_code.ToUpper() == "OK") ?? false;
				if (success)
					Database.Main.Delete(song);
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override async Task<bool> MoveSong(PlaylistSong song, string previousId, string nextId, int index)
		{
			try
			{
				bool success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.playlistentries.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams()
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest()
						{
							mutations =
							{
								new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation()
								{
									update = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation.PlaylistUpdateMutation()
									{
										id = song.Id,
										trackId = song.SongId,
										followingEntryId = nextId,
										playlistId = song.PlaylistId,
										precedingEntryId = nextId,
									}
								},
							}
						}
					}
				};

				var resp = await Api.Post<RootMutateApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}
				await SyncPlaylistTracks();
				return resp?.result?.mutate_response?.Any(x => x.response_code.ToUpper() == "OK") ?? false;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override async Task<bool> AddToPlaylist(List<Track> tracks, Playlist playlist)
		{
			if (string.IsNullOrWhiteSpace(playlist.Id))
			{
				if (!await CreatePlaylist(playlist))
				{
					return false;
				}
			}
			try
			{
				var trackData = new Dictionary<string, Track>();
				var mutations = new List<GoogleMusicApiRequest.MutateEditParams.MutateRequest.Mutation>();
				var lastTrack =
					(await
						Database.Main.TablesAsync<PlaylistSong>()
							.Where(x => x.PlaylistId == playlist.Id)
							.OrderByDescending(x => x.SOrder)
							.FirstOrDefaultAsync());
				var lastId = lastTrack?.Id;
                foreach (var track in tracks)
				{
					var clientid = $"CLIENT-{Guid.NewGuid()}";
					trackData[clientid] = track;
					mutations.Add(new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation
					{
						Create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation.PlaylistUpdateMutation()
						{
							ClientId = clientid,
							playlistId = playlist.Id,
							precedingEntryId = lastId,
							trackId = track.Id,
							source = track.Id.StartsWith("T") ? 2 : 1,
						}
					});
					lastId = clientid;
				}
				//loop through and set up following entryid
				if (mutations.Count > 1)
				{
					int index = 0;
					foreach (GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation mutation in mutations)
					{
						var next = index ++;
						if (next >= mutations.Count)
							break;
						(mutation.Create as GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation.PlaylistUpdateMutation).followingEntryId =
							(mutations[next] as GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation).Create.ClientId;
					}
				}
				var request = new GoogleMusicApiRequest
				{
					method = "sj.playlistentries.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest
						{
							mutations = mutations,
						},
						refresh = null,
					},
				};
				var result = await Api.Post<RootMutateApiObject>(request);
				var success = true;
				var songs = new List<PlaylistSong>();
				var sOrder = lastTrack?.SOrder + 10?? 10;
				foreach (var resp in result.result.mutate_response)
				{
					if (resp.response_code == "OK")
					{
						var t = trackData[resp.client_id];
						songs.Add(new PlaylistSong {Id = resp.id, PlaylistId = playlist.Id, SongId = t.SongId, SOrder = sOrder});
						sOrder += 10;
					}
					if (resp.response_code != "OK" && success)
						success = false;
				}
				if (songs.Count >= 1)
				{
					Database.Main.InsertOrReplaceAll(songs);
					SyncDatabase();
				}

				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override Task<bool> AddToPlaylist(List<Track> tracks, string playlistName)
		{
			return AddToPlaylist(tracks, new Playlist(playlistName));
		}

		public override async Task<bool> SetRating(Track track, int rating)
		{
			try
			{
				var ratingSring = "";
				switch (rating)
				{
					case 0:
						ratingSring = "NOT_RATED";
						break;
					case 1:
						ratingSring = "ONE_STAR";
						break;
					case 2:
						ratingSring = "TWO_STARS";
						break;
					case 3:
						ratingSring = "THREE_STARS";
						break;
					case 4:
						ratingSring = "FOUR_STARS";
						break;
					case 5:
						ratingSring = "FIVE_STARS";
						break;
				}
				var success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.activity.recordUserActivity",
					parameters = new GoogleMusicApiRequest.NowPlayingParams()
					{
						request = new GoogleMusicApiRequest.NowPlayingParams.RequestClass()
						{
							events = new List<GoogleMusicApiRequest.NowPlayingParams.RequestClass.Event>
							{
								new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Event()
								{
									createdTimestampMillis = DateTime.Now.ToUnixTimeMs(),
									details = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Details
									{
										rating = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.SongRating()
										{
											rating = ratingSring,
										}
									},
									trackId = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.TrackId
									{
										metajamCompactKey = track.Id.StartsWith("T") ? track.Id : null,
										lockerId = track.Id.StartsWith("T") ? null : track.Id
									},
								}
							}
						}
					}

				};

				var resp = await Api.Post<RecordPlaybackResponse>(request, true);
				
				return resp.result?.eventResults?.All(x => x.Success) == true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public async Task<bool> CreatePlaylist(Playlist playlist)
		{
			try
			{
				var clientid = $"CLIENT-{Guid.NewGuid()}".ToUpper();
                var request = new GoogleMusicApiRequest
				{
					method = "sj.playlists.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest
						{
							mutations = new List<GoogleMusicApiRequest.MutateEditParams.MutateRequest.Mutation>()
							{
								new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation
								{
									Create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation.PlaylistCreateMutation
									{
										ClientId =clientid,
										Name = playlist.Name,
									}
								}	
							},
						},
						refresh = null,
					}
				};

				var resp = await Api.Post<RootMutateApiObject>(request);
				var result = resp.result.mutate_response.FirstOrDefault(x => x.client_id == clientid);
				if (result.response_code != "OK")
					return false;
				playlist.Id = result.id;
				Database.Main.InsertOrReplace(playlist);
				SyncDatabase();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;

		}

		public override async Task<List<Song>> GetAlbumDetails(string id)
		{
			try
			{
				bool success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.album.get",
					parameters = new GoogleMusicApiRequest.ItemDetailsParams()
					{
						IncludeTracks = true,
						nid = id,
					}

				};

				var songs = new List<Song>();

				var cachedResult = RequestCache<AlbumDataResultObject>.AlbumResults?.Get(id);
				if (cachedResult == null)
				{
					var resp = await Api.Post<AlbumDataResultObject>(request);

					if (!await ProcessAlbumTracks(id, resp?.result?.Tracks))
						return songs;
					RequestCache<AlbumDataResultObject>.AlbumResults.Add(id,resp);
                }
				var tempSongs = await Database.Main.TablesAsync<TempSong>().Where(x => x.ParentId == id).ToListAsync() ??
								new List<TempSong>();
				if(tempSongs.Count == 0)
				{
					if (!await ProcessAlbumTracks(id, cachedResult.result.Tracks))
						return songs;
					tempSongs = await Database.Main.TablesAsync<TempSong>().Where(x => x.ParentId == id).ToListAsync() ??
						new List<TempSong>();
				}
				var theAlbum = (await Database.Main.TablesAsync<AlbumIds>().Where(x => x.Id == id).FirstOrDefaultAsync());
                var albumId = theAlbum?.AlbumId;
				var existingSongs = string.IsNullOrWhiteSpace(albumId) ? new List<string>()
					: (await Database.Main.TablesAsync<Song>().Where(x => x.AlbumId == albumId).ToListAsync()).Select(x => x.Id).ToList();


				songs.AddRange(tempSongs.Where(x=> !existingSongs.Contains(x.Id)));
				return songs.OrderBy(x=> x.Disc).ThenBy(x=> x.Track).ToList();
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return new List<Song>();
		}

		public override async Task<SearchResults> GetArtistDetails(string id)
		{
			return await Task.Run(async () =>
			{
				try
				{
					var request = new GoogleMusicApiRequest
					{
						method = "sj.artist.get",
						parameters = new GoogleMusicApiRequest.ArtistParams
						{
							ArtistId = id,
						}
					};
					var resp = await Api.Post<ArtistDetailsResponse>(request);
					var result = new SearchResults
					{
					};

					foreach (var x in resp.Result.topTracks)
					{
						try
						{
							var t = new FullTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
							{
								Deleted = x.Deleted,
								Duration = x.Duration,
								ArtistServerId = x.ArtistMatchedId,
								AlbumServerId = x.AlbumId,
								AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork {Url = a.Url}).ToList(),
								ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork {Url = a.Url}).ToList(),
								MediaType = MediaType.Audio,
								PlayCount = x.PlayCount,
								ServiceId = Api.CurrentAccount.Identifier,
								Id = x.Type == 0 ? x.Id : !string.IsNullOrWhiteSpace(x.StoreId) && x.StoreId.StartsWith("T") ? x.StoreId : x.Id,
								ServiceType = ServiceType.Google,
								Rating = x.Rating,
								FileExtension = "mp3",
								Disc = x.Disc,
								Track = x.Track,
								Year = x.Year,
							};
							if (!x.IsAllAccess)
							{
								result.Songs.Add(new Song(t.Title, t.NormalizedTitle));
								continue;
							}

							result.Songs.Add(new OnlineSong(t.Title, t.NormalizedTitle)
							{
								Id = t.SongId,
								Artist = t.Artist,
								Album = t.Album,
								AlbumId = t.AlbumId,
								ArtistId = t.Artist,
								Disc = t.Disc,
								Genre = t.Genre,
								Rating = t.Rating,
								TrackCount = t.Track,
								Year = t.Year,
								TrackData = t,
							});
							continue;
						}
						catch (Exception ex)
						{
							ex.Data["Description"] = "Error processing Search Entries";
							ex.Data["Service Type"] = ServiceType;
							ex.Data["Entry"] = x.ToJson();
							LogManager.Shared.Report(ex);
						}
					}

					foreach (var r in resp.Result.related_artists)
					{

						var t = new FullTrackData("", r.name, r.name, "", "");
						var a = new OnlineArtist(t.Artist, t.NormalizedAlbumArtist)
						{
							OnlineId = r.artistId,
                            AllArtwork =
								new[] { new ArtistArtwork { ArtistId = t.ArtistId, ServiceType = ServiceType, Url = r.artistArtRef } },
						};
						result.Artist.Add(a);
					}

					foreach (var r in resp.Result.albums)
					{
						var t = new FullTrackData("", r.artist, r.albumArtist, r.name, "");
						var a = new OnlineAlbum(t.Album, t.NormalizedAlbum)
						{
							ServiceType = ServiceType,
							Id = t.AlbumId,
							Artist = t.Artist,
							AlbumArtist = t.AlbumArtist,
							ArtistId = t.ArtistId,
							AlbumId = r.albumId,
							Year = r.year,
							AllArtwork =
								new[] { new AlbumArtwork { AlbumId = t.AlbumId, Url = r.albumArtRef, ServiceType = ServiceType }, },
						};
						result.Albums.Add(a);
					}

					return result;
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
				return null;
			});
		}

		public override Task<List<OnlinePlaylistEntry>> GetPlaylistEntries(OnlinePlaylist playlist)
		{
			return Task.Run(async () =>
			{
				try
				{
					bool success = false;

					var entries = new[] {
						new GoogleMusicApiRequest.EntriesParams.Entry
						{
							ShareToken = playlist.ShareToken
						}
					};
					var request = new { entries = entries, includeDeleted = true };
					const string path = "plentries/shared";
					var query = new Dictionary<string, string>
					{
						["max-results"] = "5000",
						["tier"] = Tier,
					};

					var resp = await Api.PostLatest<RootOnlinePlaylistApiObject>(path, request, query);

					Console.WriteLine(resp);
					var songs = new List<OnlinePlaylistEntry>();
					foreach (var s in resp.Entries[0].playlistEntry)
					{
						if (s.Track == null)
						{
							var ple = new TempPlaylistEntry
							{
								PlaylistEntryId = s.AbsolutePosition + " - " + s.CreationTimestamp,
								TrackId = s.TrackId,
								PlaylistId = s.PlaylistId,
								SOrder = s.AbsolutePosition,
							};
							// partTracks.Add(ple);
							continue;
						}
						var x = s.Track;
						var t = new FullPlaylistTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
						{
							ParentId = s.PlaylistId,
							Deleted = x.Deleted,
							Duration = x.Duration,
							ArtistServerId = x.ArtistMatchedId,
							AlbumServerId = x.AlbumId,
							AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork {Url = a.Url}).ToList(),
							ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork {Url = a.Url}).ToList(),
							MediaType = MediaType.Audio,
							PlayCount = x.PlayCount,
							ServiceId = Api.CurrentAccount.Identifier,
							Id = x.Type == 0 ? x.Id : !string.IsNullOrWhiteSpace(x.StoreId) && x.StoreId.StartsWith("T") ? x.StoreId : x.Id,
							ServiceType = ServiceType.Google,
							Rating = x.Rating,
							FileExtension = "mp3",
							//Playlist Stuff
							PlaylistEntryId = s.Id,
							TrackId = s.TrackId,
							PlaylistId = s.PlaylistId,
							SOrder = s.AbsolutePosition,
						};
						var song = new OnlineSong(t.Title, t.NormalizedTitle)
						{
							TrackData = t,
							AlbumId = t.AlbumId,
							Id = t.SongId,
							ArtistId = t.ArtistId,
							Artist = t.DisplayArtist,
							Album = t.Album,
							Genre = t.Genre,
							Rating = t.Rating,
							PlayedCount = t.PlayCount,
						};
						songs.Add(new OnlinePlaylistEntry
						{
							OnlineSong = song,
							Id = s.AbsolutePosition + " - " + s.CreationTimestamp,
							PlaylistId = s.PlaylistId,
							SOrder = s.AbsolutePosition,
							SongId = t.SongId,
						});
					}
					return songs;
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
				return new List<OnlinePlaylistEntry>();
			});
		}

		static GoogleMusicApiRequest.NowPlayingParams.RequestClass.Context CreateContext(PlaybackContext evt)
		{
			switch (evt.Type)
			{
				case PlaybackContext.PlaybackType.Album:
					return new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Context
					{
						albumId = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Context.AlbumId
						{
							metajamCompactKey = evt.ParentId
						} // new  evt.ParentId,
					};
				case PlaybackContext.PlaybackType.Radio:
					return new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Context
					{
						radioId = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Context.RadioId
						{
							radioId = evt.ParentId,
						}
					};
				default:
					return null;
			}

			return null;;
		}
		public override async Task<bool> RecordPlayack(PlaybackEndedEvent data)
		{
			try
			{
				var success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.activity.recordUserActivity",
					parameters = new GoogleMusicApiRequest.NowPlayingParams()
					{
						request = new GoogleMusicApiRequest.NowPlayingParams.RequestClass()
						{
							events = new List<GoogleMusicApiRequest.NowPlayingParams.RequestClass.Event>
							{
								new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Event()
								{
									createdTimestampMillis = data.Time.ToLocalTime().ToUnixTimeMs(),
									details = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Details
									{
										play = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.Play
										{
											playTimeMillis = (int)data.Position,
											trackDurationMillis = (int)data.Duration,
											context = CreateContext(data.Context),
										}
									},
									trackId = new GoogleMusicApiRequest.NowPlayingParams.RequestClass.TrackId
									{
										metajamCompactKey = data.TrackId.StartsWith("T") ? data.TrackId : null,
										lockerId = data.TrackId.StartsWith("T") ? null : data.TrackId
									},
								}
							}
						}
					}

				};

				var resp = await Api.Post<RecordPlaybackResponse>(request,true);

				Console.WriteLine(resp.Error);
				return resp.result?.eventResults?.All(x => x.Success) == true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}
		
		public override async Task<SearchResults> Search(string query)
		{
			var cachedResult = RequestCache<SearchResultResponse>.WebSearchResults.Get(query);

			return await Task.Run(async() =>
			{
				try
				{
					var request = new GoogleMusicApiRequest
					{
						method = "sj.search.subscription",
						parameters = new GoogleMusicApiRequest.SearchParams
						{
							Query = query,
							MaxResults = 25
						}
					};
					var resp = cachedResult ?? await Api.Post<SearchResultResponse>(request);
                    var result = new SearchResults
					{
						Query = query,
					};
					foreach (var r in resp.Result.entries)
					{
						try
						{
							if (r.track != null)
								{
								var x = r.track;
								var id = x.Type == 0 ? x.Id : !string.IsNullOrWhiteSpace(x.StoreId) && x.StoreId.StartsWith("T") ? x.StoreId : x.Id;
								var t = new FullTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
								{
									Deleted = x.Deleted,
									Duration = x.Duration,
									ArtistServerId = x.ArtistMatchedId,
									AlbumServerId = x.AlbumId,
									AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork {Url = a.Url}).ToList(),
									ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork {Url = a.Url}).ToList(),
									MediaType = MediaType.Audio,
									PlayCount = x.PlayCount,
									ServiceId = Api.CurrentAccount.Identifier,
									Id = id,
									ServiceExtra = x.Id == id ?  x.StoreId : x.Id,
									ServiceExtra2 = x.Type.ToString(),
									ServiceType = ServiceType.Google,
									Rating = x.Rating,
									FileExtension = "mp3",
									Disc = x.Disc,
									Track = x.Track,
									Year = x.Year,
								};
								if (!r.track.IsAllAccess)
								{
									result.Songs.Add(new Song(t.Title, t.NormalizedTitle));
									continue;
								}

								result.Songs.Add(new OnlineSong(t.Title, t.NormalizedTitle)
								{
									Id = t.SongId,
									Artist = t.Artist,
									Album = t.Album,
									AlbumId = t.AlbumId,
									ArtistId = t.ArtistId,
									Disc = t.Disc,
									Genre = t.Genre,
									Rating = t.Rating,
									TrackCount = t.Track,
									Year = t.Year,
									TrackData = t,
								});
								continue;
							}
							if (r.youtube_video != null)
							{
								var x = r.youtube_video;
								var t = new FullTrackData(x.title, "", "", "", "")
								{
									AlbumArtwork =
										x.thumbnails.Select(a => new AlbumArtwork {Url = a.url, Height = a.height, Width = a.width}).ToList(),
									MediaType = MediaType.Video,
									ServiceId = Api.CurrentAccount.Identifier,
									Id = x.id,
									ServiceType = ServiceType.Google,
									FileExtension = "mp4",
								};
								result.Videos.Add(new OnlineSong(t.Title, t.NormalizedTitle)
								{
									Id = t.SongId,
									Artist = t.Artist,
									Album = t.Album,
									AlbumId = t.AlbumId,
									ArtistId = t.Artist,
									Disc = t.Disc,
									Genre = t.Genre,
									Rating = t.Rating,
									TrackCount = t.Track,
									Year = t.Year,
									TrackData = t
								});
								continue;
							}

							if (r.album != null)
							{
								var t = new FullTrackData("", r.album.artist, r.album.albumArtist, r.album.name, "");
								var a = new OnlineAlbum(t.Album, t.NormalizedAlbum)
								{
									ServiceType = ServiceType,
									Id = t.AlbumId,
									Artist = t.Artist,
									AlbumArtist = t.AlbumArtist,
									ArtistId = t.ArtistId,
									AlbumId = r.album.albumId,
									Year = r.album.year,
									AllArtwork =
										new[] {new AlbumArtwork {AlbumId = t.AlbumId, Url = r.album.albumArtRef, ServiceType = ServiceType},},
								};
								result.Albums.Add(a);
								continue;
							}

							if (r.artist != null)
							{

								var t = new FullTrackData("", r.artist.name, r.artist.name, "", "");
								var a = new OnlineArtist(t.Artist, t.NormalizedAlbumArtist)
								{
									OnlineId = r.artist.artistId,
									AllArtwork =
										new[] {new ArtistArtwork {ArtistId = t.ArtistId, ServiceType = ServiceType, Url = r.artist.artistArtRef}},
								};
								result.Artist.Add(a);
								continue;
							}

							if (r.station != null)
							{
								var artwork = r.station.CompositeArtRefs?.Select(x => new RadioStationArtwork
								{
									ServiceType = ServiceType,
									Url = x.Url,
									Ratio = x.AspectRatio
										}).ToList() ?? new List<RadioStationArtwork>();
								artwork.AddRange(
									r.station?.ImageUrls?.Select(
									x => new RadioStationArtwork {ServiceType = ServiceType, Url = x.Url, Ratio = x.AspectRatio}).ToList() ?? new List<RadioStationArtwork>());
								var s = new OnlineRadioStation()
								{
									ServiceId = Api.Identifier,
									Name = r.station.Name,
									Description = r.station.Description,
									AllArtwork = artwork.ToArray(),
									StationSeeds = r.station.StationSeeds.Select(ss =>
									{
										int k;
										if (!int.TryParse(ss.SeedType, out k))
										{
											Console.WriteLine(ss.SeedType);
										}
										return new RadioStationSeed
										{
											ItemId = ss.Id,
											Description = ss.Kind,
											Kind = k,
										};
									}).ToArray(),
								};
								result.RadioStations.Add(s);
								continue;
							}

							if (r.playlist != null)
							{
								var p = new OnlinePlaylist
								{
									ServiceType = ServiceType,
									Name = r.playlist.name,
									Owner = r.playlist.ownerName,
									OwnerImage = r.playlist.ownerProfilePhotoUrl,
									ServiceId = Api.Identifier,
									PlaylistType = r.playlist.type,
									ShareToken = r.playlist.shareToken,
									Description = r.playlist.description,
									AllArtwork = r.playlist.albumArtRef?.Select(x=> new AlbumArtwork {Url = x.url}).ToArray() ?? new AlbumArtwork[0],
								};
								result.Playlists.Add(p);
								continue;
							}
						}
						catch (Exception ex)
						{
							ex.Data["Description"] = "Error processing Search Entries";
							ex.Data["Service Type"] = ServiceType;
							ex.Data["Entry"] = r.ToJson();
							LogManager.Shared.Report(ex);
						}
					}

					RequestCache<SearchResultResponse>.WebSearchResults.Add(query,resp);
					return result;
				}
				catch (Exception e)
				{
					LogManager.Shared.Report(e);
				}
				return null;
			});
		}


		protected async Task<bool> ProcessAlbumTracks(string id,List<SongItem> tracks)
		{
			var fullTracks = new List<FullTrackData>();

			tracks?.ForEach(x =>
			{
				if (!string.IsNullOrEmpty(x.Id))
				{
					return;
				}
				var t = new FullTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
				{
					ParentId = id,
					Deleted = x.Deleted,
					Duration = x.Duration,
					ArtistServerId = x.ArtistMatchedId,
					AlbumServerId = x.AlbumId,
					AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork { Url = a.Url }).ToList(),
					ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork { Url = a.Url }).ToList(),
					MediaType = MediaType.Audio,
					PlayCount = x.PlayCount,
					ServiceId = Api.CurrentAccount.Identifier,
					Id = x.Nid,
					ServiceExtra = x.Id == x.Nid ?  x.StoreId : x.Id,
					ServiceExtra2 = x.Type.ToString(),
					ServiceType = ServiceType.Google,
					Rating = x.Rating,
					FileExtension = "mp3",
					Track = x.Track,
					Disc = x.Disc,
					Year = x.Year
				};

				fullTracks.Add(t);
				if (x.PrimaryVideo != null)
				{
					var y = t.Clone();
					y.Id = x.PrimaryVideo.Id;
					y.FileExtension = "mp4";
					y.MediaType = MediaType.Video;
					fullTracks.Add(y);
				}
			});

			return await MusicProvider.ProcessAlbumTracks(fullTracks);
		}


		#endregion //Playlists

		long LastRadioStationSync = 0;

		async Task<bool> SyncRadioStations()
		{
			LastRadioStationSync = Api.ExtraData?.LastRadioSync ?? 0;
			TotalSync = 0;
			var start = DateTime.Now;
			var s = await GetRadioStations();
			var finished = (DateTime.Now - start).TotalMilliseconds;
			Console.WriteLine($"Sync complete {TotalSync} in {finished} ms");
			//App.ShowAlert("Finished", $"Sync complete {TotalSync} in {finished} ms");
			return s;
		}

		async Task<bool> GetRadioStations(string token = null)
		{
			try
			{
				bool success = false;
				var request = new PostRequest
				{
					MaxResults = 250,
				};

				const string path = "radio/station";
				var query = new Dictionary<string, string>
				{
					["tier"] = Tier,
					["updated-min"] = (token == null ? Api.ExtraData.LastRadioSync : 0).ToString(),
				};
				if (token != null)
					query["startToken"] = token;

				var resp = await SyncRequestQueue.Enqueue(1, () => Api.PostLatest<RootRadioStationsApiObject>(path,request,query));
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}
				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(resp?.NextPageToken))
					nextTask = GetRadioStations(resp?.NextPageToken);
				var start = DateTime.Now;
				List<RadioStationArtwork> artwork = new List<RadioStationArtwork>();
				var stations = resp?.Data?.Items?.Select(x => ProcessStation(x, ref artwork)).ToList() ?? new List<RadioStation>();
				success = await MusicProvider.ProcessRadioStations(stations, artwork);
				var finished = (DateTime.Now - start).TotalMilliseconds;
				//App.ShowAlert("Finished", $"Sync complete {tracks.Count} in {finished} ms");
				if (nextTask != null)
					return await nextTask;
				if (!success)
					return false;
				Api.ExtraData.LastRadioSync = LastRadioStationSync;
				ApiManager.Shared.SaveApi(Api);
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		protected RadioStation ProcessStation(Station x, ref List<RadioStationArtwork> artwork, bool updateTimestamp = true)
		{
			LastRadioStationSync = Math.Max(LastRadioStationSync, x.LastModifiedTimestamp);
			if (x.CompositeArtRefs != null)
				artwork.AddRange(x.CompositeArtRefs?.Select(a => new RadioStationArtwork
				{
					Id = $"{x.Id} - {a.Url}",
					Ratio = a.AspectRatio,
					StationId = x.Id,
					Url = a.Url,
				}));
			int k;
			if (!int.TryParse(x.Seed.SeedType, out k))
			{
				Console.WriteLine(x.Seed.SeedType);
			}
			return new RadioStation(x.Name)
			{
				Id = x.Id,
				DateCreated = x.RecentTimestamp,
				RecentDateTime = x.LastModifiedTimestamp,
				IsIncluded = x.InLibrary,
				ServiceType = this.ServiceType,
				ServiceId = Api.Identifier,
				Description = x.Description,
				Deleted = x.Deleted,
				StationSeeds = new [] {
					new RadioStationSeed
					{
						Id = $"{x.Id} - {x.Seed.Id}",
						StationId = x.Id,
						ItemId = x.Seed.Id,
						Description = x.Seed.Kind,
						Kind = k,
					}
				}
			};
		}


		public override async Task<bool> LoadRadioStation(RadioStation station, bool isContinuation)
		{
			try
			{
				bool success = false;

				const string path = "radio/stationfeed";
				var query = new Dictionary<string, string>
				{
					["tier"] = Tier,
					//TODO: switch for continuation
					["rz"] = "start",
				};
				bool isIFL = station.Id == "IFL";
				var request = new
				{
					contentFilter = Settings.FilterExplicit ? "2" : "1",
					stations = new List<GoogleMusicApiRequest.RadioStationParams.StationRequest>
					{
						new GoogleMusicApiRequest.RadioStationParams.StationRequest
						{
							radioId = isIFL ? null : station.Id,
							numEntries = 25,
							seed = isIFL ? new {seedType = 6} : null,
						}
					},
				};

				var resp = await SyncRequestQueue.Enqueue(1, () => Api.PostLatest<RootRadioStationsTracksApiObject>(path, request, query));
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}
				success = await ProcessStationTracks(station, resp.Data.Stations.First().Tracks);
				if (!isContinuation)
					UpdateRadioStationLastPlayed(station);
				else
					SyncDatabase();
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override Task<RadioStation> CreateRadioStation(string name, RadioStationSeed seed)
		{
			var mutation = CreateRadioRequestMutation(name);
			var kind =
				 (GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes)seed.Kind;
			mutation.CreateOrGet.Seed = new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation.
				CreateOrGetClass.SeedClass
			{
				SeedType = kind,
			};
			
			switch (kind)
			{
				case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Album:
					mutation.CreateOrGet.Seed.AlbumId = seed.ItemId;
					break;
				case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Artist:
					mutation.CreateOrGet.Seed.ArtistId = seed.ItemId;
					break;
				case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Locker:
					mutation.CreateOrGet.Seed.TrackLockerId = seed.ItemId;
					break;
				case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.TrackId:
					mutation.CreateOrGet.Seed.TrackId = seed.ItemId;
					break;
				case GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.CurratedRadioStation:
					mutation.CreateOrGet.Seed.CuratedStationId = seed.ItemId;
					break;
			}
			return CreateRadioStation(mutation);
		}

	protected async Task<bool> UpdateRadioStationLastPlayed(RadioStation station)
		{
			try
			{
				bool success = false;

				var updated = ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds);
				var request = new GoogleMusicApiRequest
				{
					method = "sj.radio.edit",

					parameters = new GoogleMusicApiRequest.RadioMutateEditParams()
					{
						mutations =new List<GoogleMusicApiRequest.RadioMutateEditParams.Mutation>()
						{
							
							new GoogleMusicApiRequest.RadioMutateEditParams.RadioStationMutation
							{
								update = new GoogleMusicApiRequest.RadioMutateEditParams.RadioStationMutation.RadioStationsUpdate
								{
									id = station.Id,
									LastModifiedTimestamp = updated,
									RecentTimestamp = updated,
								}
							}
                        }
					}
				};

				var resp = await Api.Post<RootRadioStationsApiObject>(request);
				SyncDatabase();
				return success;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}
		protected async Task<bool> ProcessStationTracks(RadioStation station, List<SongItem> tracks)
		{
			var fullTracks = new List<FullPlaylistTrackData>();
			var partTracks = new List<TempRadioStationSong>();
			var order =
				MusicPlayer.Data.Database.Main.ExecuteScalar<int>("select count(*) from RadioStationSong where StationId = ?",
					station.Id);
			tracks.ForEach(s =>
			{
				if (!string.IsNullOrEmpty(s.Id) && string.IsNullOrWhiteSpace(s.Title))
				{
					var ple = new TempRadioStationSong
					{
						PlaylistEntryId = s.Id,
						TrackId = $"{s.Id} - {station.Id}",
						PlaylistId = station.Id,
						SOrder = order++,
					};

					partTracks.Add(ple);
					return;
				}
				var id = string.IsNullOrEmpty(s.Id) ? s.Nid : s.Id;
				var x = s;
				var t = new FullPlaylistTrackData(x.Title, x.Artist, x.AlbumArtist, x.Album, x.Genre)
				{
					ParentId = station.Id,
					Deleted = x.Deleted,
					Duration = x.Duration,
					ArtistServerId = x.ArtistMatchedId,
					AlbumServerId = x.AlbumId,
					AlbumArtwork = x.AlbumArtRef.Select(a => new AlbumArtwork {Url = a.Url}).ToList(),
					ArtistArtwork = x.ArtistArtRef.Select(a => new ArtistArtwork {Url = a.Url}).ToList(),
					MediaType = MediaType.Audio,
					PlayCount = x.PlayCount,
					ServiceId = Api.CurrentAccount.Identifier,
					Id = id,
					ServiceType = ServiceType.Google,
					Rating = x.Rating,
					FileExtension = "mp3",
					//Playlist Stuff
					PlaylistEntryId = $"{id} - {station.Id}",
					TrackId = id,
					ServiceExtra = x.Nid,
					PlaylistId = station.Id,
					SOrder = order++,
					Track = x.Track,
					Disc = x.Disc,
					Year = x.Year
				};
				fullTracks.Add(t);
				if (x.PrimaryVideo != null)
				{
					var y = t.Clone();
					y.Id = x.PrimaryVideo.Id;
					y.FileExtension = "mp4";
					y.MediaType = MediaType.Video;
					fullTracks.Add(y);
				}
			});

			return await MusicProvider.ProcessRadioStationTracks(fullTracks, partTracks);
		}

		public override Task<RadioStation> CreateRadioStation(string name, Track track)
		{
			var mutation = CreateRadioRequestMutation(name);
			var isAllAccess = track.Id.StartsWith("T");
			mutation.CreateOrGet.Seed = new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation.
				CreateOrGetClass.SeedClass
			{
				SeedType = isAllAccess
					? GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.TrackId
					: GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Locker,
			};
			if (isAllAccess)
				mutation.CreateOrGet.Seed.TrackId = track.Id;
			else
			{
				mutation.CreateOrGet.Seed.TrackLockerId = track.Id;
			}
			return CreateRadioStation(mutation);
		}

		public override Task<RadioStation> CreateRadioStation(string name, AlbumIds track)
		{
			var mutation = CreateRadioRequestMutation(name);
			mutation.CreateOrGet.Seed = new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation.
				CreateOrGetClass.SeedClass
			{
				SeedType =
					GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Album,
				AlbumId = track.Id,
			};
			return CreateRadioStation(mutation);
		}

		public override Task<RadioStation> CreateRadioStation(string name, ArtistIds track)
		{
			var mutation = CreateRadioRequestMutation(name);

			mutation.CreateOrGet.Seed = new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation.
				CreateOrGetClass.SeedClass
			{
				SeedType =
					GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes.Artist,
				ArtistId = track.Id,
			};
			return CreateRadioStation(mutation);
		}

		protected GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation CreateRadioRequestMutation(
			string name)
		{
			var mutation = new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation()
			{
				CreateOrGet = new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation.CreateOrGetClass()
				{
					Name = name,
				},
				Paramaters = new GoogleMusicApiRequest.RadioStationParams
				{
					contentFilter = Settings.FilterExplicit ? "2" : "1",
				}
			};

			return mutation;
		}

		public async Task<RadioStation> CreateRadioStation(
			GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation mutation)
		{
			try
			{
				var request = new GoogleMusicApiRequest
				{
					method = "sj.radio.edit",
					parameters = new GoogleMusicApiRequest.RadioMutateEditParams()
					{
						mutations = {mutation}
					}
				};


				var resp = await Api.Post<RootCreateRadioStationsTracksApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return null;
				}

				foreach (var mutationResp in resp.Result.MutateResponse)
				{
					var artwork = new List<RadioStationArtwork>();
					var station = ProcessStation(mutationResp.Station, ref artwork, false);

					var success = await MusicProvider.ProcessRadioStations(new List<RadioStation> {station}, artwork);
					if (!success)
						continue;
					await ProcessStationTracks(station, mutationResp.Station.Tracks);
					UpdateRadioStationLastPlayed(station);
					return station;
				}
				;

				return null;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return null;
		}

		public override async Task<bool> DeleteRadioStation(RadioStation station)
		{
			try
			{
				bool success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.radio.edit",
					parameters = new GoogleMusicApiRequest.RadioMutateEditParams()
					{
						mutations =
						{
							new GoogleMusicApiRequest.RadioMutateEditParams.RadioStationMutation
							{
								update = new GoogleMusicApiRequest.RadioMutateEditParams.RadioStationMutation.RadioStationsUpdate
								{
									id = station.Id,
									InLibrary = "false",
									LastModifiedTimestamp = station.RecentDateTime,
									RecentTimestamp = station.DateCreated,
								}
							},
						}
					}
				};


				var resp = await Api.Post<RootMutateApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}

				success = resp?.result?.mutate_response?.Any(x => x.response_code.ToLower() == "ok") ?? false;
				if (!success)
					return false;

				station.IsIncluded = false;
				Database.Main.Update(station);
				SyncDatabase();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override async Task<bool> AddToLibrary(RadioStation station)
		{
			try
			{
				bool success = false;
				var seeds = station.StationSeeds.Select(x => new GoogleMusicApiRequest.RadioMutateEditParams.CreateRadioStationMutation.CreateOrGetClass.SeedClass
				{
					SeedType = (GoogleMusicApiRequest.RadioMutateEditParams.SeedTypes)x.Kind,
					Id = x.ItemId,
				}).ToArray();
				var request = new GoogleMusicApiRequest
				{
					method = "sj.radio.edit",
					parameters = new GoogleMusicApiRequest.RadioMutateEditParams
					{
						mutations = 
						{
						new GoogleMusicApiRequest.RadioMutateEditParams.RadioStationMutation()
						{
								update = new GoogleMusicApiRequest.RadioMutateEditParams.RadioStationMutation.RadioStationsUpdate
								{
									id = station.Id,
									InLibrary = "true",
									LastModifiedTimestamp = station.RecentDateTime,
									RecentTimestamp = station.DateCreated,
									Name = station.Name,
									Seed = seeds.FirstOrDefault(),
									Seeds = seeds
								}
							},
						}
					}
				};


				var resp = await Api.Post<RootMutateApiObject>(request, true);

				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}

				success = resp?.result?.mutate_response?.Any(x => x.response_code.ToLower() == "ok") ?? false;
				if (!success)
					return false;

				station.IsIncluded = true;
				Database.Main.Update(station);
				SyncDatabase();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override async Task<bool> AddToLibrary(OnlineSong song)
		{

			var create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation
			{
				Create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation.TrackCreateMutation
				{
					ClientId = $"Client-{Guid.NewGuid()}",
					Nid = song.TrackData.Id,
				},
			};
			return await AddToLibrary(create);

		}

		public override async Task<bool> AddToLibrary(OnlineAlbum album)
		{
			var songs = (await GetAlbumDetails(album.AlbumId)).Select(x=> x.Id).ToArray();

			var tracks = await Database.Main.TablesAsync<TempTrack>().Where(x => x.AlbumId == album.Id && x.MediaType== MediaType.Audio).ToListAsync();
			if(tracks.Count() == 0)
				return false;

			tracks = tracks.Where(x=> songs.Contains(x.SongId)).ToList();
			if(tracks.Count() == 0)
				return true;
			var edits = tracks.Select(x => new GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation
			{
				Create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation.TrackCreateMutation
				{
					ClientId = $"Client-{Guid.NewGuid()}",
					Nid = x.Id,
				},
			}).ToArray();
			return await AddToLibrary(edits);
		}

		public async override Task<bool> AddToLibrary(Track track)
		{
			var create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation
			{
				Create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation.TrackCreateMutation
				{
					ClientId = $"Client-{Guid.NewGuid()}",
					Nid = track.Id,
				},
			};
			return await AddToLibrary(create);
		}
        public async Task<bool> AddToLibrary(params GoogleMusicApiRequest.MutateEditParams.MutateRequest.TrackMutation[] mutations)
		{
			try
			{
				bool success = false;
				var request = new GoogleMusicApiRequest
				{
					method = "sj.tracks.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams()
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest()
						{
							mutations = mutations.OfType<GoogleMusicApiRequest.MutateEditParams.MutateRequest.Mutation>().ToList(),
						}
					}
				};


				var resp = await Api.Post<RootMutateApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}

				success = resp?.result?.mutate_response?.Any(x => x.response_code.ToLower() == "ok") ?? false;
				if (!success)
					return false;
				SyncDatabase();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		//public async Task<bool> AddToLibrary()


		public override async Task<DownloadUrlData> GetDownloadUri(Track track)
		{
			if (!Api.HasAuthenticated)
				await Api.Authenticate();
			try
			{
				if (string.IsNullOrWhiteSpace(Api.DeviceId))
					await Api.GetDeviceId();
				var qualityString = QualityString(Settings.DownloadStreamQuality,true);

				var result = await GetTrackUri(track, qualityString);
				
				var auth = new System.Net.Http.Headers.AuthenticationHeaderValue(Api.CurrentOAuthAccount.TokenType,
					Api.CurrentOAuthAccount.Token);
				var data = new DownloadUrlData
				{
					Url = result.Item1,
					Headers =
					{
						{"X-Device-ID", Api.DeviceId},
						{"X-Device-FriendlyName", Device.Name},
						{"Authorization", auth.ToString()},
                        {"User-Agent","com.google.PlayMusic/2.1.0 iSL/1.0 iPhone/8.2 hw/iPhone7_2 (gzip)" },
					}
				};
				#if __IOS__
				if(Api.DeviceId.StartsWith("ios:"))
					data.Headers.Add("X-Device-ID-IOS-Deprecated", Api.DeviceId);
				#endif
				return data;
			}
			catch (Exception ex)
			{
				ex.Data["Method"] = "GetDownloadUri";
                if (!(ex is TimeoutException))
					LogManager.Shared.Report(ex);
				else
					Console.WriteLine(ex);
			}
			return null;
		}

		public override async Task<bool> AddToLibrary(OnlinePlaylist playlist)
		{
			try
			{
				bool success = false;
				var clientid = $"CLIENT-{Guid.NewGuid()}";
				var request = new GoogleMusicApiRequest
				{
					method = "sj.playlists.batchmutate",
					parameters = new GoogleMusicApiRequest.MutateEditParams()
					{
						request = new GoogleMusicApiRequest.MutateEditParams.MutateRequest()
						{
							mutations =
							{
								new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation
								{
									Create = new GoogleMusicApiRequest.MutateEditParams.MutateRequest.PlaylistMutation.PlaylistCreateMutation
									{
										ClientId = clientid,
										Name = playlist.Name,
										Description = playlist.Description,
										Type = "SHARED",
										ShareToken = playlist.ShareToken,
									}
								},
							}
						}
					}
				};


				var resp = await Api.Post<RootMutateApiObject>(request);
				if (resp?.Error != null)
				{
					Console.WriteLine(resp.Error);
					return false;
				}

				success = resp?.result?.mutate_response?.Any(x => x.response_code.ToLower() == "ok") ?? false;
				if (!success)
					return false;
				
				SyncDatabase();
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public override async Task<string> GetShareUrl (Song song)
		{
			var tracks = await Database.Main.TablesAsync<Track> ().Where (x => x.SongId == song.Id && x.ServiceType == ServiceType).ToListAsync () ?? new List<Track>();
			tracks.AddRange(await Database.Main.TablesAsync<TempTrack>().Where(x => x.SongId == song.Id && x.ServiceType == ServiceType).ToListAsync());
			var youtube = tracks.FirstOrDefault (x => x.MediaType == MediaType.Video);
			if (youtube != null) {
				return $"https://www.youtube.com/watch?v={youtube.Id}";
			}
			string trackId = null;
			foreach(var track in tracks){
				trackId = track.Id;
				if (trackId.StartsWith ("T")) {
					break;
				}
				trackId = track.ServiceExtra ?? "";
				if (trackId.StartsWith ("T"))
					break;
				trackId = null;
			}
			if (string.IsNullOrWhiteSpace (trackId) || !trackId.StartsWith ("T"))
				return null;
			var name = song.Name?.Replace(' ','_') ?? "";
			return $"https://play.google.com/music/m/{trackId}?t={name}";
		}
		public override async Task<Uri> GetPlaybackUri(Track track)
		{
			if (track.MediaType == MediaType.Video)
				return await GetVideoUri(track);
			if (!Api.HasAuthenticated)
				await Api.Authenticate();
			var wifi = CrossConnectivity.Current.ConnectionTypes.Any (x => x == ConnectionType.WiFi);// (Reachability.LocalWifiConnectionStatus() == NetworkStatus.ReachableViaWiFiNetwork);
			var qualityString = QualityString( wifi ? Settings.WifiStreamQuality : Settings.MobileStreamQuality, wifi);
			try
			{

				var result = await GetTrackUri(track, qualityString);
				return result?.Item2;
			}
			catch (Exception ex)
			{
				if (!(ex is TimeoutException))
					LogManager.Shared.Report(ex);
				else
					Console.WriteLine(ex);
			}
			return null;
		}
		bool AccountLocked = false;
		DateTime AccountLockedTime;
		async Task<Tuple<string, Uri>> GetTrackUri(Track track, string qualityString, int tryCount = 0)
		{
			if (AccountLocked && (DateTime.Now - AccountLockedTime).Minutes < 5)
				return new Tuple<string, Uri>("ERROR", null);
			const string key = "34ee7983-5ee6-4147-aa86-443ea062abf774493d6a-2a15-43fe-aace-e78566927585\n";
			try
			{
				if (string.IsNullOrWhiteSpace(Api.DeviceId))
					await Api.GetDeviceId();
				var guid = ((long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalMilliseconds).ToString();
				var songId = !string.IsNullOrWhiteSpace(track.ServiceExtra) && tryCount > 0 ? track.ServiceExtra : track.Id;
				var sig = GetSig(songId + guid, key);
				var parameter = $"&{(songId.StartsWith("T") ? "mjck" : "songid")}={songId}";

				var startUrl = $"https://android.clients.google.com/music/mplay?slt={guid}&sig={sig}{parameter}&pt=e{qualityString}";

				var client = new HttpClient();
				await Api.PrepareClient(client);
				var devices = await Api.GetDeviceId();
				client.DefaultRequestHeaders.Add("X-Device-FriendlyName", Api.DeviceName);
				client.DefaultRequestHeaders.Add("X-Device-ID", devices);
				if(devices.StartsWith("ios:"))
					client.DefaultRequestHeaders.Add("X-Device-ID-IOS-Deprecated", devices);
				client.DefaultRequestHeaders.UserAgent.ParseAdd("com.google.PlayMusic/2.1.0 iSL/1.0 iPhone/8.2 hw/iPhone7_2 (gzip)");

				var cancelToken = new CancellationTokenSource(TimeSpan.FromSeconds(60));
				var respTask = client.GetAsync(startUrl, HttpCompletionOption.ResponseHeadersRead,cancelToken.Token);
				if (await Task.WhenAny(respTask, Task.Delay(TimeSpan.FromSeconds(60))) != respTask)
					throw new TimeoutException();
				var resp = respTask.Result;

				var returlUri = resp.RequestMessage.RequestUri;
				//resp.Headers.ForEach(x => Console.WriteLine(x));
				if(resp.IsSuccessStatusCode)
					return new Tuple<string, Uri>(startUrl, returlUri);
				if (tryCount == 0 && !string.IsNullOrWhiteSpace(track.ServiceExtra))
				{
					LogManager.Shared.GetPlaybackUrlError(resp.StatusCode.ToString(),tryCount,track);
					return await GetTrackUri(track, qualityString, 1);
				}
				else
				{
					IEnumerable<string> reasons;
					if (resp.Headers.TryGetValues("x-rejected-reason", out reasons))
					{
						var reason = reasons.FirstOrDefault();
						Console.WriteLine("Rejected reason: " + reason);
						LogManager.Shared.Log("Rejected reason", "Reason", reason);
						if (reason == "DEVICE_NOT_AUTHORIZED" || reason == "EXCEEDED_DEVICE_TRANSITION_QUOTA")
						{
							LogManager.Shared.GetPlaybackUrlError(reason, tryCount, track);
							if (Api.HasMoreDevices())
							{
								await Api.GetDeviceId(true);
								return await GetTrackUri(track, qualityString, 0);
							}
							else
								return new Tuple<string, Uri>("ERROR", null);
						}
						else if (reason == "STREAM_RATE_LIMIT_REACHED")
						{
							AccountLocked = true;
							AccountLockedTime = DateTime.Now;
							return new Tuple<string, Uri>("ERROR", null);
						}
						else
						{
							LogManager.Shared.Report(new Exception($"Get Url Rejected: {reason}"));
							return new Tuple<string, Uri>("ERROR", null);
						}
					}

					else
					{
						if(Api.HasMoreDevices())
						{
							await Api.GetDeviceId(true);
							return await GetTrackUri(track, qualityString, 0);
						}
						else{

							LogManager.Shared.GetPlaybackUrlError(resp.StatusCode.ToString(), tryCount, track);
							return new Tuple<string, Uri>("ERROR", null) ;
						}
					}
                    LogManager.Shared.GetPlaybackUrlError(resp.StatusCode.ToString(), tryCount, track);
					Console.WriteLine("Error downloading track url");
				}
				return new Tuple<string, Uri>("ERROR", null) ;
			}
			catch (Exception ex)
			{
				if (!(ex is TimeoutException))
					LogManager.Shared.Report(ex);
				else
					Console.WriteLine(ex);
			}
			return new Tuple<string, Uri>("ERROR", null) ;
		}


		public async Task<Uri> GetVideoUri(Track track)
		{
			return await Task.Run(()=>{
				var url = $"https://www.youtube.com/watch?v={track.Id}";
				var videoInfos = YoutubeExtractor.DownloadUrlResolver.GetDownloadUrls(url);
				var video = YouTubeHelper.GetVideoInfo(videoInfos, true);
				return new Uri(video.DownloadUrl);
			});
		}

		public static string GetSig(string input, string key, bool trim = true)
		{
			var encoding = new ASCIIEncoding();
			//var newKey = StringToAscii(key);
			byte[] newKey = StringToAscii(key); // 
			byte[] newKey2 = encoding.GetBytes(key);
			var hmacsha1 = new HMACSHA1(newKey);

			//byte[] byteArray = StringToAscii(input);
			byte[] byteArray = StringToAscii(input); // encoding.GetBytes (input);
			byte[] foo = hmacsha1.ComputeHash(byteArray);

			string ret = Convert.ToBase64String(foo);
			ret = ret.Replace('+', '-')
				.Replace('/', '_');
			if (!trim)
				return ret;
			return ret.Substring(0, ret.Length - 1);
			//.Replace('=', '.');
		}

		public static byte[] StringToAscii(string s)
		{
			var retval = new byte[s.Length];
			for (int ix = 0; ix < s.Length; ++ix)
			{
				char ch = s[ix];
				if (ch <= 0x7f)
					retval[ix] = (byte) ch;
				else
					retval[ix] = (byte) '?';
			}
			return retval;
		}

		static string QualityString(StreamQuality quality,bool wifi)
		{
			var netString = wifi ? "wifi" : "mob";
			switch (quality)
			{
				case StreamQuality.High:
					return $"&opt=hi&net={netString}&targetkbps=512";
				case StreamQuality.Medium:
					return $"&opt=med&net={netString}&targetkbps=128";
				case StreamQuality.Low:
					return $"&opt=low&net={netString}&targetkbps=64";
			}
			return "&opt=low&net=wifi&targetkbps=128";
		}

		async Task LogIn(bool allowCancel)
		{
			try
			{

				await Api.Authenticate();
			}
			catch (TaskCanceledException ex)
			{
				if(!allowCancel)
					await LogIn(allowCancel);
			}
		}
	}
}