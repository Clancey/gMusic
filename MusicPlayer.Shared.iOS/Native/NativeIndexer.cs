using System;
using CoreSpotlight;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using System.Threading.Tasks;
using MobileCoreServices;
using System.Collections.Generic;
using System.Linq;


namespace MusicPlayer
{
	public class NativeIndexer : ManagerBase<NativeIndexer>
	{
		public NativeIndexer()
		{
		}

		public Task<bool> Index(IEnumerable<Song> songs)
		{
			var tcs = new TaskCompletionSource<bool>();

			Task.Run(() => {
				var items = songs.Select(x=> CreateSearchItem(x)).Where(x=>  x != null).ToArray();
				if(!(items?.Any() == true)){
					tcs.TrySetResult(true);
					return;
				}
				CSSearchableIndex.DefaultSearchableIndex.Index(items,(error)=>{
					if (error !=null) {
						tcs.TrySetException(new Exception(error.LocalizedDescription));
					}
					else
						tcs.TrySetResult(true);
				});
			});
			return tcs.Task;
		}

		CSSearchableItem CreateSearchItem(Song song)
		{
			try{
				var attributes = new CSSearchableItemAttributeSet();
				attributes.Album = song.Album;
				attributes.ContentType = UTType.Audio;
				attributes.Title = song.Name;
				attributes.Artist = song.Artist;
				attributes.ContentRating = song.Rating;
				attributes.MusicalGenre = song.Genre;
				attributes.Identifier = song.Id;
				var item = new CSSearchableItem(song.Id, "songdata", attributes);
				return item;
			}
			catch(Exception ex) {
				return null;
			}
		}
	}
}

