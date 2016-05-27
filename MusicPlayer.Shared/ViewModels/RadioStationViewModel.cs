using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Localizations;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleDatabase;

namespace MusicPlayer.ViewModels
{
	public partial class RadioStationViewModel : BaseViewModel<RadioStation>
	{
		bool isIncluded;

		public RadioStationViewModel()
		{
			Title = Strings.Radio;
		}

		public bool IsIncluded
		{
			get { return isIncluded; }
			set
			{
				isIncluded = value;
				var group = Database.Main.GetGroupInfo<RadioStation>().Clone();
				if (IsIncluded)
					group.AddFilter("IsIncluded = 1");
				else
				{
					group.Limit = 10;
					group.OrderByDesc = true;
					group.OrderBy = "RecentDateTime";
					group.GroupBy = "";
				}
				GroupInfo = group;
			}
		}

		public override GroupInfo OfflineGroupInfo
		{
			get { return GroupInfo; }
			set { }
		}
		public bool AutoPlaysOnSelect { get; set; } = true;
		public override void RowSelected(RadioStation item)
		{
			base.RowSelected(item);
			if (AutoPlaysOnSelect)
				PlayItem (item);
		}
		public async void PlayItem(RadioStation item)
		{
			await PlaybackManager.Shared.Play(item);
		}
	}
}