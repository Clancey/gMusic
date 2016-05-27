using System;
using MusicPlayer.Managers;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public class PopupManager : ManagerBase<PopupManager>
	{
		public PopupManager ()
		{
		}

		public async Task<Tuple<string, string>> GetCredentials(string title, string details = "", string url = "")
		{
			throw new NotImplementedException ();
		}


	}
}

