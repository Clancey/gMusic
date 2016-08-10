using System;
using System.Threading.Tasks;
using MusicPlayer.Managers;
namespace MusicPlayer
{
	public class PopupManager : ManagerBase<PopupManager>
	{
		public PopupManager ()
		{
		}

		public Task<Tuple<string, string>> GetCredentials (string title, string details = "", string url = "")
		{
			throw new NotImplementedException ();
		}
	}
}

