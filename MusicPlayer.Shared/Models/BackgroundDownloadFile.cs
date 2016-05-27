using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using Newtonsoft.Json;

namespace MusicPlayer.Models
{
	[Serializable]
	public partial class BackgroundDownloadFile
	{
		public string Id { get; set; } = Guid.NewGuid().ToString();
		public string SessionId { get; set; }

		public string Url { get; set; }

		public string Destination { get; set; }

		public float Percent { get; set; }

		public string Error { get; internal set; }

		public string TempLocation { get; set; }

		public FileStatus Status { get; set; }

		public DateTime LastUpdate { get; set; }

		public int RetryCount { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public bool IsActive { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public bool IsCompleted { get; set; }

		public enum FileStatus
		{
			Downloading,
			Completed,
			Error,
			Canceled,
			Temporary,
		}

		public string TrackId { get; set; }

		public long TaskId { get; set; }
		public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

		Track track;
		public Track Track
		{
			get {return track?? (track = Database.Main.GetObject<Track,TempTrack>(TrackId));}
			set { track = value; }
		}
	}
}