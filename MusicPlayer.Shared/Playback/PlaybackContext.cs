using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using MusicPlayer.Models;
using SQLite;

namespace MusicPlayer.Playback
{
	public class PlaybackContext
	{
		[PrimaryKey, AutoIncrement]
		public int Id { get; set; }

		public enum PlaybackType
		{
			Radio,
			Playlist,
			Genre,
			Song,
			Album,
			Artist,
		}

		public PlaybackType Type { get; set; }

		public string ParentId { get; set; }
		public bool IsContinuous { get; set; }
		public override string ToString ()
		{
			return string.Format ("[PlaybackContext: Id={0}, Type={1}, ParentId={2}, IsContinuous={3}]", Id, Type, ParentId, IsContinuous);
		}

		public override bool Equals(object obj)
		{
			var context = obj as PlaybackContext;
			if(context == null)
				return false;
			return context.IsContinuous == IsContinuous && context.Type ==  Type && context.ParentId == ParentId;
		}
		public override int GetHashCode()
		{
			return ParentId.GetHashCode();
		}
	}
}