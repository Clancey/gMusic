using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using AppKit;
using Foundation;

namespace MusicPlayer
{
	public static class MediaItemImageExtensions
	{
		public static async Task<NSImage> GetLocalImage(this MediaItemBase item, double width)
		{
			try
			{
				List<Track> tracks = null;
				if (item is Song)
					tracks =
						await
							Database.Main.TablesAsync<Track>()
								.Where(x => x.ServiceType == ServiceType.FileSystem && x.SongId == item.Id)
								.ToListAsync();
				else if (item is Album)
					tracks =
						await
							Database.Main.TablesAsync<Track>()
							.Where(x => x.ServiceType == ServiceType.FileSystem && x.AlbumId == item.Id)
							.ToListAsync();
				else if (item is Artist)
					tracks =
						await
						Database.Main.TablesAsync<Track>()
							.Where(x => x.ServiceType == ServiceType.FileSystem && x.ArtistId == item.Id)
							.ToListAsync();
				if(tracks != null)
					foreach (var track in tracks)
					{
						var image = await track.GetImage(width);
						if (image != null)
							return image;
					}
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return null;
		}

		static async Task<NSImage> GetImage(this Track track,double width)
		{
			return await Task.Factory.StartNew (() => {
				try {
					using (var file = TagLib.File.Create (track.FileLocation)) {
						if (file.Tag.Pictures.Any ()) {
							var bytes = file.Tag.Pictures [0].Data.Data;
							if (bytes == null)
								return null;

							using (var data = NSData.FromArray (bytes)) {
								return new NSImage(data);
							}
						}
					}
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}
				return null;
			});
		}

	}
}
