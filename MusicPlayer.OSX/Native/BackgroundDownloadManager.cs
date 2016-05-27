using System;
using MusicPlayer.Managers;
using System.Threading.Tasks;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class BackgroundDownloadManager: ManagerBase<BackgroundDownloadManager>
	{
		public BackgroundDownloadManager ()
		{
		}

		public async Task Download (Track track)
		{
			App.ShowNotImplmented();
		}

		public int Count {
			get;
			set;
		}

		public Song PendingItemForRow (int row)
		{
			throw new NotImplementedException ();
		}

		public async Task Init ()
		{
			
		}
	}
}

