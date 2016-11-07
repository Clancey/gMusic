using System;
using System.Threading.Tasks;
using Amazon.CloudDrive;
using System.Linq;
using System.Collections.Generic;
using Punchclock;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Data;

namespace MusicPlayer.Api
{
	public class AmazonMusicProvider : MusicProvider
	{
		public new CloudDriveApi Api {
			get {  return (CloudDriveApi) base.Api; }
		}

		public override ServiceType ServiceType {
			get {
				return ServiceType.Amazon;
			}
		}

		public override bool RequiresAuthentication {
			get {
				return true;
			}
		}

		public override string Id {
			get {
				return Api.Identifier;
			}
		}

		public override MediaProviderCapabilities[] Capabilities {
			get {
				return new[]{ MediaProviderCapabilities.None};
			}
		}
		public AmazonMusicProvider (CloudDriveApi api) : base (api)
		{

		}
		public override string Email
		{
			get
			{
				string email = "";
				Api?.CurrentAccount?.UserData?.TryGetValue("email", out email);
				return email;
			}
		}

		#region implemented abstract members of MusicProvider

		protected override async Task<bool> Sync()
		{
			if (!Api.HasAuthenticated)
				await Api.Authenticate();
			await Api.Identify();
			var s =  await SyncTracks("mp3");

			return false;
		}

		async Task<bool> SyncTracks(string extension, string continuation = null)
		{
			try
			{
				var nodes = await Api.GetNodeList(new CloudNodeRequest { Filter = new ContentExtensionFilter { extension }, IncludeLinks = true, StartToken = continuation });
				List<CloudNode> indexedNodes = new List<CloudNode>();
				List<CloudNode> unIndexedNodes = new List<CloudNode>();
				foreach (var node in nodes.Data)
				{
					string id;
					Dictionary<string, string> properties;
					if (node.Properties?.TryGetValue(ApiConstants.AmazonAppIdentifier, out properties) ?? false && properties.TryGetValue("Id", out id) && !string.IsNullOrWhiteSpace(id))
					{
						indexedNodes.Add(node);
					}
					else
						unIndexedNodes.Add(node);
				}
				var indexTask = IndexTracks(unIndexedNodes);
				var processTask = ProcessTracks(indexedNodes);
				Task<bool> nextTask = null;
				if (!string.IsNullOrWhiteSpace(nodes.NextToken))
					nextTask = SyncTracks(extension, nodes.NextToken);
				await Task.WhenAll(indexTask, processTask);
				if (nextTask != null)
					return await nextTask;
				return indexTask.Result && processTask.Result;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		async Task<bool> ProcessTracks(List<CloudNode> nodes)
		{
			var tracks = await Task.Run(()=>nodes.Select(CreateFromNode).Where(x=> x != null).ToList());
			return await ProcessTracks(tracks);
		}

		FullTrackData CreateFromNode(CloudNode node)
		{
			try
			{
				string title;
				string id;
				string album;
				string artist;
				string genre;
				int year = 0;
				string yearStr;
				var dict = node.Properties[ApiConstants.AmazonAppIdentifier];
				dict.TryGetValue("Title", out title);
				dict.TryGetValue("Id", out id);
				dict.TryGetValue("Album", out album);
				dict.TryGetValue("Artist", out artist);
				dict.TryGetValue("Genre", out genre);
				int rating = 0;
				string ratingString;
				if (dict.TryGetValue("Year", out yearStr))
					year = int.Parse(yearStr);
				if (dict.TryGetValue ("Rating", out ratingString))
					rating = int.Parse (ratingString);
				var track = new FullTrackData(title, artist, "", album, genre)
				{
					Year = year,
					SongId = id,
					Id = node.Id,
					ServiceId = Id,
					Rating = rating,
					ServiceType = ServiceType.Amazon,
					MediaType = node.ContentProperties.Video != null ? MediaType.Video : node.ContentProperties.image != null ? MediaType.Photo : MediaType.Audio,
					FileExtension = node.ContentProperties.Extension,
				};
				return track;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return null;
		}

		async Task<bool> IndexTracks(List<CloudNode> nodes)
		{
			List<FullTrackData> tracks = new List<FullTrackData>();
			await nodes.BatchForeach(25, (nodeBatch) => Task.Run(async () =>
			{
				try
				{
					var batchTasks = nodeBatch.Select(UpdateServerMetaData);
					await Task.WhenAll(batchTasks);
					var batchTracks = batchTasks.Select(x => x.Result).Where(x => x != null).ToList();
					tracks.AddRange(batchTracks);
				}
				catch (Exception ex)
				{
					LogManager.Shared.Report(ex);
				}
			}));

			return await ProcessTracks(tracks);

		}

		public override System.Threading.Tasks.Task<bool> Resync ()
		{
			throw new NotImplementedException ();
		}

		public override async System.Threading.Tasks.Task<Uri> GetPlaybackUri (MusicPlayer.Models.Track track)
		{
			var result = await Api.GetTemporaryUrl(track.Id);
			return new Uri(result);
		}

		public override async Task<MusicPlayer.Models.DownloadUrlData> GetDownloadUri (MusicPlayer.Models.Track track)
		{
			var result = await Api.GetTemporaryUrl(track.Id);
			return new DownloadUrlData { Url = result };
		}

		public override System.Threading.Tasks.Task<bool> LoadRadioStation (MusicPlayer.Models.RadioStation station, bool isContinuation)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.RadioStationSeed seed)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.AlbumIds track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<MusicPlayer.Models.RadioStation> CreateRadioStation (string name, MusicPlayer.Models.ArtistIds track)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> DeleteRadioStation (MusicPlayer.Models.RadioStation station)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> DeletePlaylist (MusicPlayer.Models.Playlist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> DeletePlaylistSong (MusicPlayer.Models.PlaylistSong song)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> MoveSong (MusicPlayer.Models.PlaylistSong song, string previousId, string nextId, int index)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToPlaylist (System.Collections.Generic.List<MusicPlayer.Models.Track> songs, MusicPlayer.Models.Playlist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToPlaylist (System.Collections.Generic.List<MusicPlayer.Models.Track> songs, string playlistName)
		{
			throw new NotImplementedException ();
		}

		public override Task<bool> SetRating (Track track, int rating)
		{

			return Api.UpdateMetaDataProperty(track.Id, ApiConstants.AmazonAppIdentifier, "Rating", rating.ToString());
		}

		public override System.Threading.Tasks.Task<System.Collections.Generic.List<MusicPlayer.Models.Song>> GetAlbumDetails (string id)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<SearchResults> GetArtistDetails (string id)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<System.Collections.Generic.List<MusicPlayer.Models.OnlinePlaylistEntry>> GetPlaylistEntries (MusicPlayer.Models.OnlinePlaylist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> RecordPlayack (MusicPlayer.Models.Scrobbling.PlaybackEndedEvent data)
		{
			//TODO: update playback count
			return Task.FromResult(true);
		}

		public override System.Threading.Tasks.Task<SearchResults> Search (string query)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.OnlinePlaylist playlist)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.RadioStation station)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.OnlineSong song)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.OnlineAlbum album)
		{
			throw new NotImplementedException ();
		}

		public override System.Threading.Tasks.Task<bool> AddToLibrary (MusicPlayer.Models.Track track)
		{
			throw new NotImplementedException ();
		}

		public override Task<string> GetShareUrl (MusicPlayer.Models.Song song)
		{
			return Task.FromResult("");
			//var tracks = await Database.Main.TablesAsync<Track> ().Where (x => x.SongId == song.Id && x.ServiceType == ServiceType).ToListAsync () ?? new List<Track> ();
			//var track = tracks.FirstOrDefault ();
			//if (track == null)
			//	return null;
			//var url = await Api.
		}

		protected static OperationQueue MetaDataQueue = new OperationQueue();
		public async Task<FullTrackData> UpdateServerMetaData(CloudNode node)
		{
			var success = await MetaDataQueue.Enqueue(5,()=> updateServerMetaData(node));
			return success;

		}

		async Task<FullTrackData> updateServerMetaData(CloudNode node)
		{
			string id = "";
			//Dictionary<string, string> properties;
			//if (node.Properties?.TryGetValue(ApiConstants.AmazonAppIdentifier,out properties) ?? false && properties.TryGetValue("Id",out id) && !string.IsNullOrWhiteSpace(id))
			//{
			//	return true;
			//}
			Models.FullTrackData fileInfo = null;
			try
			{
				fileInfo = await GetTrackDataFromWebServer(Api, node.TempLink);
				fileInfo.ServiceType = ServiceType.Amazon;
				fileInfo.ServiceId = Id;
				fileInfo.Id = node.Id;
				fileInfo.FileExtension = node.ContentProperties.Extension;
				fileInfo.MediaType = node.ContentProperties.Video != null ? MediaType.Video : node.ContentProperties.image != null ? MediaType.Photo : MediaType.Audio;

				await Api.UpdateMetaDataProperty(node.Id, ApiConstants.AmazonAppIdentifier, "Id", fileInfo.SongId);
				await Api.UpdateMetaDataProperty(node.Id, ApiConstants.AmazonAppIdentifier, "Artist", fileInfo.Artist);
				await Api.UpdateMetaDataProperty(node.Id, ApiConstants.AmazonAppIdentifier, "Album", fileInfo.Album);
				await Api.UpdateMetaDataProperty(node.Id, ApiConstants.AmazonAppIdentifier, "Title", fileInfo.Title);
				if (!string.IsNullOrWhiteSpace(fileInfo.Genre))
					await Api.UpdateMetaDataProperty(node.Id, ApiConstants.AmazonAppIdentifier, "Genre", fileInfo.Genre);

				if (fileInfo.Year > 0)
					await Api.UpdateMetaDataProperty(node.Id, ApiConstants.AmazonAppIdentifier, "Year", fileInfo.Year.ToString());
				return fileInfo;
			}
			catch (Exception ex)
			{

				//TODO: Add to heavy indexing!
				//Console.WriteLine(ex);
				return fileInfo;
			}

		}

		#endregion
	}
}

