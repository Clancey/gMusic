using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using MusicPlayer.Managers;

namespace MusicPlayer.Droid.UI
{

	[Activity(Label = "gMusic", MainLauncher = true, Icon = "@drawable/ic_launcher", Theme = "@style/UAmpAppTheme")]
	class MusicPlayerActivity : BaseActivity
	{

		public const string ExtraCurrentPage = "come.IIS.gMusic.CURRENT_PAGE";

		public const string EXTA_START_FULLSCREEN = "com.IIS.gMusic.EXTRA_START_FULLSCREEN";
		public const string CurrentMediaDescription = "com.IIS.gMusic.CURRENT_MEDIA_DESCRIPTION";
		public const string FRAGMENT_TAG = "gmusic_list_container";

		protected override void OnStart()
		{
			base.OnStart();

			SimpleAuth.WebAuthenticatorActivity.UserAgent = "Mozilla/5.0(iPhone;U;CPUiPhoneOS4_0likeMacOSX;en-us)AppleWebKit/532.9(KHTML,likeGecko)Version/4.0.5Mobile/8A293Safari/6531.22.7";
			SetupEverything();
		}
		Bundle voiceSearchParams;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			App.Context = this;
			SetContentView(Resource.Layout.activity_player);

			InitializeToolbar();
			InitializeFromParams(savedInstanceState, Intent);
			if (savedInstanceState == null)
				StartFullScreenActivityIfNeeded(Intent);
		}

		AlertDialog spinnerDialog;
		static bool IsSetup;
		public void SetupEverything()
		{
			if (IsSetup)
				return;
			IsSetup = true;
			MobileCenter.Start(ApiConstants.MobileCenterApiKey,
					typeof(Analytics), typeof(Crashes));
			var appInfo = this.ApplicationInfo;

			App.Context = this;
			App.Invoker = RunOnUiThread;
			App.DoPushFragment = (f) =>
			{
				Navigate(f);
			};
			App.OnShowSpinner = (text) => {
				if (spinnerDialog != null)
					spinnerDialog.Dismiss ();
				spinnerDialog = new ProgressDialog.Builder (this).SetMessage (text).SetCancelable (false).Show();
			};
			App.OnDismissSpinner = () => {
				spinnerDialog?.Dismiss ();
				spinnerDialog = null;
			};

			//Downloader.Init ();
			//Settings.Init (this);
			//var metrics = new DisplayMetrics();
			//WindowManager.DefaultDisplay.GetMetrics(metrics);

			CheckApi();

		}

		async void CheckApi()
		{
			ApiManager.Shared.Load();
			ApiManager.Shared.StartSync();
			if (ApiManager.Shared.Count > 0)
			{
				return;
			}
			try
			{
				var api = ApiManager.Shared.CreateApi(MusicPlayer.Api.ServiceType.Google);
				api.ResetData();
				var account = await api.Authenticate();
				if (account == null)
					return;
				ApiManager.Shared.AddApi(api);
				await ApiManager.Shared.CreateYouTube();
				ApiManager.Shared.GetMusicProvider(Api.ServiceType.YouTube)?.SyncDatabase();
				var manager = ApiManager.Shared.GetMusicProvider(api.Identifier);
				using (new Spinner("Syncing Database"))
				{
					await manager.Resync();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		protected override void OnSaveInstanceState(Bundle outState)
		{
			var id = CurrentPageId;
			if(!string.IsNullOrWhiteSpace(id))
				outState.PutString(ExtraCurrentPage, id);
			base.OnSaveInstanceState(outState);
		}

		protected override void OnNewIntent(Intent intent)
		{
			InitializeFromParams(null,intent);
			StartFullScreenActivityIfNeeded(intent);
		}

		protected void StartFullScreenActivityIfNeeded(Intent intent)
		{
			if (!intent?.GetBooleanExtra(EXTA_START_FULLSCREEN, false) ?? false)
				return;
			var fullScreenIntent =
				new Intent(this, typeof (FullScreenPlayerActivity)).SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop)
					.PutExtra(CurrentMediaDescription, (IParcelable) intent.GetParcelableExtra(CurrentMediaDescription));
			StartActivity(fullScreenIntent);
		}

		protected void InitializeFromParams(Bundle savedInstanceState, Intent intent)
		{
			string currentPage = null;
			var action = intent.Action;
			if (action?.Equals(MediaStore.IntentActionMediaPlayFromSearch) ?? false)
			{
				voiceSearchParams = intent.Extras;
			}
			else
			{
				currentPage = savedInstanceState?.GetString(ExtraCurrentPage);
			}
			Navigate(currentPage);

		}

		public void Navigate(Fragment fragment)
		{
			var transaction = FragmentManager.BeginTransaction();
			transaction.SetCustomAnimations(
				Resource.Animator.slide_in_from_right, Resource.Animator.slide_out_to_left,
				Resource.Animator.slide_in_from_left, Resource.Animator.slide_out_to_right);
			transaction.Replace(Resource.Id.container, fragment, FRAGMENT_TAG);
			// If this is not the top level media (root), we add it to the fragment back stack,
			// so that actionbar toggle and Back will work appropriately:
			//if (mediaId != null)
			//{
				transaction.AddToBackStack(null);
			//}
			transaction.Commit();
		}

		void SetRoot(Fragment fragment)
		{
			if(FragmentManager.BackStackEntryCount > 0)
				FragmentManager.PopBackStack(null, PopBackStackFlags.Inclusive);
			Navigate(fragment);
		}

		public void Navigate(string page)
		{
			if (string.IsNullOrWhiteSpace(page))
			{
				SetRoot(new SongFragment());
				return;
			}

			//TODO: parse page to find out where to go
		}

		public string CurrentPageId
		{
			get
			{
				//TODO: get from CurrentFragment;
				return null;
			}
		}

		//TODO: cast to base fragment
		public Fragment CurrentFragment => FragmentManager.FindFragmentByTag(FRAGMENT_TAG);


		public override void OnMediaControllConnected()
		{
			if (voiceSearchParams != null)
			{

				var query = voiceSearchParams.GetString(Android.App.SearchManager.Query);
				SupportMediaController.GetTransportControls().PlayFromSearch(query,voiceSearchParams);
				voiceSearchParams = null;
			}
			//TODO: CurrentFragment.OnConnected();
		}
	}
}