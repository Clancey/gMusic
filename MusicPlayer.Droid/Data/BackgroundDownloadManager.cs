using System;
using System.Threading.Tasks;
using MusicPlayer.Models;

namespace MusicPlayer.Managers
{
	public class BackgroundDownloadManager : ManagerBase<BackgroundDownloadManager>
	{
		public void Init()
		{

		}
		public BackgroundDownloadManager()
		{
			
		}

		public int Count { get; set; }

		public Song PendingItemForRow(int row)
		{
			return null;
		}

		internal Task Download(Track track)
		{
			throw new NotImplementedException();
		}
	}
}

