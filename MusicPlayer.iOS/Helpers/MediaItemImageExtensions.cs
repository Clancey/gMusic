using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Api.iPodApi;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using UIKit;

namespace MusicPlayer.iOS
{
	public static class MediaItemImageExtensions
	{
		public static async Task<UIImage> GetLocalImage(this MediaItemBase item, float width)
		{
			try
			{
				List<Track> tracks = null;
				if (item is Song)
					tracks =
						await
							Database.Main.TablesAsync<Track>()
								.Where(x => x.ServiceType == ServiceType.iPod && x.SongId == item.Id)
								.ToListAsync();
				else if (item is Album)
					tracks =
						await
							Database.Main.TablesAsync<Track>()
								.Where(x => x.ServiceType == ServiceType.iPod && x.AlbumId == item.Id)
								.ToListAsync();
				if(tracks != null)
					foreach (var track in tracks)
					{
						var image = track.GetImage(width);
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

		static UIImage GetImage(this Track track,float width)
		{
			try
			{
				var mpItem = iPodProvider.GetItem(track);
                return mpItem?.Artwork?.ImageWithSize(new CoreGraphics.CGSize(width, width));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return null;
		}
	}
}
