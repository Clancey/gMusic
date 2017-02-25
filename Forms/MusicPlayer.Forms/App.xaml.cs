using System.Threading.Tasks;
using Localizations;
using MusicPlayer.Forms;
using MusicPlayer.Managers;
using Xamarin.Forms;

namespace MusicPlayer
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = CreateRoot();
			if (ApiManager.Shared.Count == 0)
			{
				MainPage.Navigation.PushModalAsync(new IntroPage(), false);
			}
			OnCheckForOffline = CheckForOffline;
		}

		async Task<bool> CheckForOffline(string message)
		{
			var resp = await MainPage.DisplayActionSheet(message, Strings.Nevermind, Strings.Continue);
			return resp == Strings.Continue;
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}
	}
}
