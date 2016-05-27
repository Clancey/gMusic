using System;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public class ShareManager : ManagerBase<ShareManager>
	{

		static Uri gMusicUrl = new Uri ("http://bit.ly/18qzWRW");
		public ShareManager ()
		{
			
		}

		public string ShareText(Song song)
		{
			var text = string.Format ("I'm listening to: \"{0}\".", song);
			return text;
		}

		public string TwitterShareText(Song song)
		{
			var text = string.Format ("#NowPlaying \"{0}\".", song.ToString (120));
			return text;
		}


		public async Task<string> ShareUrl(Song song)
		{
			if (song.ServiceTypes.Contains (MusicPlayer.Api.ServiceType.YouTube)) {
				var api = ApiManager.Shared.GetMusicProvider (MusicPlayer.Api.ServiceType.YouTube);
				var url = await api.GetShareUrl (song);
				if (!string.IsNullOrWhiteSpace (url))
					return url;
			}

			if (song.ServiceTypes.Contains (MusicPlayer.Api.ServiceType.Google)) {
				var api = ApiManager.Shared.GetMusicProvider (MusicPlayer.Api.ServiceType.Google);
				var url = await api.GetShareUrl (song);
				if (!string.IsNullOrWhiteSpace (url))
					return url;
			}

//			if (song.ServiceTypes.Contains (MusicPlayer.Api.ServiceType.Amazon)) {
//				var api = ApiManager.Shared.GetMusicProvider (MusicPlayer.Api.ServiceType.Google);
//				var url = await api.GetShareUrl (song);
//				if (!string.IsNullOrWhiteSpace (url))
//					return url;
//			}
			return gMusicUrl.AbsoluteUri;

		}
		#if __IOS__
		public Task<UIKit.UIImage> ShareImage(Song song)
		{
			return null;
		}
		#endif
	}
}

