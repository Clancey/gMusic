using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Data;
using MusicPlayer.Models;
using System.Threading.Tasks;
namespace MusicPlayer.Managers
{
	internal class ArtworkManager : ManagerBase<ArtworkManager>
	{
		public string GetUrl(Artwork artwork)
		{
			if (artwork == null)
				return null;
			if (artwork.ServiceType == ServiceType.Google)
				return GoogleArtworkUrl(artwork.Url);
			//TODO: handle other providers
			return artwork.Url;
		}

		string GoogleArtworkUrl(string url)
		{
			try
			{
				if (string.IsNullOrEmpty(url))
					return null;
				if (!url.Contains("=s"))
					url += "=s";

				int index = url.LastIndexOf("=s");
				string newString = url.Substring(0, index + 2);

				if (!newString.StartsWith("http"))
					newString = $"http:{newString}";
				return $"{newString}{Images.AlbumArtScreenSize}-c";
			}
			catch (Exception exception)
			{
				Console.WriteLine(exception);
				return null;
			}
		}


		public async Task<string> GetArtwork(MediaItemBase item)
		{
			var album = item as Album;
			if (album != null)
				return await GetArtwork (album);

			var song = item as Song;
			if (song != null)
				return await GetArtwork (song);

			var artist = item as Artist;
			if (artist != null)
				return await GetArtwork (artist);

			var station = item as RadioStation;
			if (station != null)
				return await GetArtwork (station);
			
			return null;
		}


		public async Task<string> GetArtwork(Album album)
		{
			var artowrk = await album.GetAllArtwork();
			return GetUrl(artowrk?.LastOrDefault());
		}

		public async Task<string[]> GetArtwork(Playlist playlist)
		{
			var artowrk = await playlist.GetAllArtwork();
			return artowrk.Select(GetUrl).ToArray();
		}

		public async Task<string[]> GetArtwork(Genre genre)
		{
			var artowrk = await genre.GetAllArtwork();
			return artowrk.Select(GetUrl).ToArray();
		}


		public async Task<string> GetArtwork(Artist artist)
		{
			var artwork = await artist.GetAllArtwork();
			return GetUrl(artwork?.FirstOrDefault());
		}

		public async Task<string> GetArtwork(Song song)
		{
			if (song == null)
				return null;
			try
			{
				var onlineSong = song as OnlineSong;
				if (onlineSong != null)
				{
					return GetUrl(onlineSong.TrackData.AlbumArtwork.FirstOrDefault());
				}
				var album = await Task.Run(()=> Database.Main.GetObject<Album, TempAlbum>(song.AlbumId));
				return await GetArtwork(album);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return null;
		}

		public async Task<string> GetArtwork(RadioStation station)
		{
			var artowrk = await station.GetAllArtwork();
			return GetUrl(artowrk?.OrderBy(x => x.Ratio).FirstOrDefault());
		}
	}
}