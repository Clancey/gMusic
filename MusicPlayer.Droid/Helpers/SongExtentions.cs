using System;
using Android.Support.V4.Media;
using MusicPlayer.Models;

namespace MusicPlayer.Droid
{
	public static class SongExtentions
	{
		public static MediaMetadataCompat ToMediaMetadataCompat(this Song song)
		{
			return new MediaMetadataCompat.Builder()
				                          .PutString(MediaMetadataCompat.MetadataKeyMediaId, song.Id)
				                          .PutString(MediaMetadataCompat.MetadataKeyAlbum, song.Album)
										  .PutString(MediaMetadataCompat.MetadataKeyTitle, song.Name)
				                          .PutString(MediaMetadataCompat.MetadataKeyArtist,song.Artist)
				                          .PutString(MediaMetadataCompat.MetadataKeyGenre,song.Genre)
				                          .PutLong(MediaMetadataCompat.MetadataKeyTrackNumber,song.Track)
				                          .PutLong(MediaMetadataCompat.MetadataKeyNumTracks,song.TrackCount)
				                          .PutLong(MediaMetadataCompat.MetadataKeyDiscNumber,song.Disc)
										  .Build();
		}
	}
}

