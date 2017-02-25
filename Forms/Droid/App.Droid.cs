using System;
using MusicPlayer.Forms;
using Xamarin.Forms;
namespace MusicPlayer
{
	public partial class App
	{
		public Page CreateRoot()
		{
			return new MasterDetailPage
			{
				Master = new ContentPage { Title = "gMusic", Content = new ListView { BackgroundColor = Color.Green } },
				Detail = new SlideUpPanel
				{
					Master = new NowPlayingPage { Title = "gMusic", Content = new ListView { BackgroundColor = Color.Blue } },
					Detail = new NavigationPage(new SongsListPage { BackgroundColor = Color.Teal }),
				},
			};
		}
	}
}
