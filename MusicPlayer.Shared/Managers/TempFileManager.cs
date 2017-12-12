using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Models;

namespace MusicPlayer.Managers
{
	internal class TempFileManager : ManagerBase<TempFileManager>
	{
		SlideQueue<string> Queue = new SlideQueue<string>(5);

		public TempFileManager()
		{
			Queue.Removed = (file) => { File.Delete(Path.Combine(Locations.TmpMusicCacheDir, file)); };
			var files = Directory.EnumerateFiles(Locations.TmpMusicCacheDir)
				.Where(x => x.EndsWith("mp3", StringComparison.CurrentCultureIgnoreCase) ||
							x.EndsWith("mp4", StringComparison.CurrentCultureIgnoreCase))
				.OrderBy(File.GetCreationTime).Where(x =>
				{
					var info = new FileInfo(x);
					if (info.Length == 0)
						File.Delete(x);
					return (info.Length > 0);
				}).Select(Path.GetFileName).ToList();
			files.ForEach(Queue.Add);
		}

		public void Cleanup()
		{
			Directory.GetFiles(Path.GetTempPath(), "*.tmp").ToList().ForEach(File.Delete);
		}

		public Tuple<bool, string> GetTempFile(string trackId)
		{
			var track = Database.Main.GetObject<Track, TempTrack>(trackId);
			var newPath = track.FileName;
			if (Queue.Contains(newPath))
				return new Tuple<bool, string>(true, Path.Combine(Locations.TmpMusicCacheDir, newPath));
			return new Tuple<bool, string>(false, null);
		}

		public string Add(string trackId, string filePath)
		{
			var track = Database.Main.GetObject<Track, TempTrack>(trackId);
			var newPath = track.FileName;
			var info = new FileInfo(filePath);
			track.FileLocation = Path.Combine(Locations.TmpMusicCacheDir, newPath);
			if (info.Length == 0)
				return null;
			if(filePath != track.FileLocation)
				File.Copy(filePath, track.FileLocation, true);
			Queue.Add(newPath);
			return track.FileLocation;
		}
	}
}