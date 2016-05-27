using System;
using Localizations;
using MusicPlayer.Models;
using SimpleDatabase;

namespace MusicPlayer.ViewModels
{
	public class ArtistViewModel : BaseViewModel<Artist>
	{
		public ArtistViewModel()
		{
			Title = Strings.Artists;
		}

		protected GroupInfo offlineGroupInfo;

		public override GroupInfo OfflineGroupInfo
		{
			get
			{
				if (offlineGroupInfo == null)
				{
					offlineGroupInfo = GroupInfo.Clone();
					offlineGroupInfo.Filter = offlineGroupInfo.Filter + (string.IsNullOrEmpty(offlineGroupInfo.Filter) ? " " : " and ") +
											"OfflineCount > 0";
				}
				return offlineGroupInfo;
			}
			set { offlineGroupInfo = value; }
		}
	}
}