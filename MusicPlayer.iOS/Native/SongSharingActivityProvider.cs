using System;
using UIKit;
using MusicPlayer.Models;
using System.Threading.Tasks;
using Foundation;

namespace MusicPlayer.iOS
{
	public class SongSharingActivityProvider : UIActivityItemProvider
	{
		readonly bool isUrl;
		WeakReference _song;
		Song song {
			get {
				return _song?.Target as Song;
			}
			set {
				_song = new WeakReference(value);
			}
		}

		
		public SongSharingActivityProvider (Song song, bool isUrl) : base((NSString)ShareManager.Shared.TwitterShareText (song))
		{
			this.song = song;
			this.isUrl = isUrl;
			if(isUrl)
				urlTask = ShareManager.Shared.ShareUrl (song);
		}

		Task<string> urlTask;

		public override Foundation.NSObject Item {
			get {

				if (isUrl)
					return NSUrl.FromString(urlTask.Result);
				if (ActivityType == UIActivityType.PostToTwitter)
					return (NSString)ShareManager.Shared.TwitterShareText (song);
				return (NSString)ShareManager.Shared.ShareText (song);
			}
		}
		public override string GetSubjectForActivity (UIActivityViewController activityViewController, NSString activityType)
		{
			return $"Check out {song}";
		}
		
	}
}

