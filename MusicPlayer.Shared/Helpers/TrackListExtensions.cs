using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Data;
using MusicPlayer.Models;
using MusicPlayer.Managers;

namespace MusicPlayer.Helpers
{
	internal static class TrackListExtensions
	{
		public static List<Track> SortByPriority(this List<Track> tracks)
		{
			return tracks.OrderByDescending(x => x, TrackPriorityComparer.Shared).ToList();
		}
	}

	public class TrackPriorityComparer : IComparer<Track>
	{
		public static TrackPriorityComparer Shared { get; set; } = new TrackPriorityComparer();
		public int Compare(Track x, Track y)
		{
			var first = OfflinePriority(TempFileManager.Shared.GetTempFile(x.Id).Item1);
			var second = OfflinePriority(TempFileManager.Shared.GetTempFile(y.Id).Item1);
			var tempPriority = first.CompareTo(second);
			if (tempPriority != 0)
				return tempPriority;
				
			                           
			var priority = x.Priority.CompareTo(y.Priority);
			if (priority != 0)
				return priority;

			var servicePriority = Priority(x.ServiceType).CompareTo(Priority(y.ServiceType));
			if (servicePriority != 0)
				return servicePriority;

			var mediaPriority = Priority(x.MediaType).CompareTo(Priority(y.MediaType));
			return mediaPriority;
		}

		public static int OfflinePriority(bool isLocal)
		{
			return isLocal ? 100 : 0;
		}

		public static int Priority(MediaType media)
		{
			switch (media)
			{
				case MediaType.Audio:
					return 100;
				case MediaType.Video:
					return Settings.PreferVideos ? 150 : 50;
			}
			return 0;
		}

		public static int Priority(ServiceType serviceType)
		{
			switch (serviceType)
			{
				case ServiceType.FileSystem:
					return 100;
				case ServiceType.iPod:
					return 90;
				case ServiceType.Google:
				case ServiceType.Amazon:
					return 80;
				default:
					return 0;
			}
		}
	}
}