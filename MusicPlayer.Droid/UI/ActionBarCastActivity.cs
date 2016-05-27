using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace MusicPlayer.Droid.UI
{
	public class ActionBarCastActivity : AppCompatActivity, FragmentManager.IOnBackStackChangedListener, DrawerLayout.IDrawerListener, NavigationView.IOnNavigationItemSelectedListener
	{
		const string TAG = "ActionBarCastActivity";

		//private VideoCastManager mCastManager;
		DrawerLayout drawerLayout;
		ActionBarDrawerToggle drawerToggle;
		IMenuItem mediaRouteMenuItem;
		Toolbar toolbar;

		bool toolBarInitialized;
		int itemToOpenWhenDrawerCloses = -1;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			// Ensure that Google Play Service is available.
			//VideoCastManager.checkGooglePlayServices(this);

			//mCastManager = ((UAMPApplication)Application).getCastManager(this);
			//mCastManager.reconnectSessionIfPossible();
		}

		protected override void OnStart()
		{
			base.OnStart();
			if(!toolBarInitialized)
				throw new Exception("You must initilize toolbar at the end of your oncreate method");
		}

		protected override void OnPostCreate(Bundle savedInstanceState)
		{
			base.OnPostCreate(savedInstanceState);
			drawerToggle?.SyncState();
		}

		protected override void OnResume()
		{
			base.OnResume();

			//mCastManager.addVideoCastConsumer(mCastConsumer);
			//mCastManager.incrementUiCounter();

			// Whenever the fragment back stack changes, we may need to update the
			// action bar toggle: only top level screens show the hamburger-like icon, inner
			// screens - either Activities or fragments - show the "Up" icon instead.
			FragmentManager.AddOnBackStackChangedListener(this);
		}

		public override void OnConfigurationChanged(Configuration newConfig)
		{
			base.OnConfigurationChanged(newConfig);
			drawerToggle?.OnConfigurationChanged(newConfig);
		}

		protected override void OnPause()
		{
			base.OnPause();

			//mCastManager.RemoveVideoCastConsumer(mCastCnsumer);
			//mCastManager.DecrementUiCounter();
            FragmentManager.RemoveOnBackStackChangedListener(this);
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			base.OnCreateOptionsMenu(menu);
			MenuInflater.Inflate(Resource.Menu.main,menu);
			//mediaRouteMenuItem = mCastManager.AddMEdiaRouterButton(menu, Resource.Id.media_route_menu_item);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			if (drawerToggle != null && drawerToggle.OnOptionsItemSelected(item))
				return true;

			if (item != null && item.ItemId == Android.Resource.Id.Home)
			{
				OnBackPressed();
				return true;
			}
			return base.OnOptionsItemSelected(item);
		}

		public override void OnBackPressed()
		{
			if (drawerLayout?.IsDrawerOpen((int)GravityFlags.Start) ?? false)
			{
				drawerLayout.CloseDrawers();
				return;
			}

			var manager = FragmentManager;
			if(manager.BackStackEntryCount > 1)
				manager.PopBackStack();
			else
				base.OnBackPressed();
		}

		protected virtual void InitializeToolbar()
		{
			toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);

			if(toolbar == null)
				throw new Exception("Layout is required to include a Toolbar with id Toolbar");
			toolbar.InflateMenu(Resource.Menu.main);
			drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			this.SetSupportActionBar(toolbar);
			toolBarInitialized = true;

			if (drawerLayout == null)
			{
				return;
			}
			
			var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
			if(navView == null)
				throw new Exception("A layout with a drawerLAyout is required to include a ListView with id 'drawerList'");

			drawerToggle = new ActionBarDrawerToggle(this,drawerLayout,toolbar,Resource.String.open_content_drawer,Resource.String.close_content_drawer);
			drawerLayout.SetDrawerListener(this);
			//drawerLayout.SetStatusBarBackgroundColor(ResourceHelper.);
			populateDrawerItems(navView);
			UpdateDrawerToggle();

		}

		void populateDrawerItems(NavigationView navView)
		{
			navView.SetNavigationItemSelectedListener(this);
			//	if (MusicPlayerActivity.class.isAssignableFrom(getClass())) {
			//	navigationView.setCheckedItem(R.id.navigation_allmusic);
			//} else if (PlaceholderActivity.class.isAssignableFrom(getClass())) {
			//	navigationView.setCheckedItem(R.id.navigation_playlists);
			//}
		}
		protected virtual void UpdateDrawerToggle()
		{
			if (drawerToggle == null)
				return;
			var isRoot = FragmentManager.BackStackEntryCount == 0;
			drawerToggle.DrawerIndicatorEnabled = isRoot;
			SupportActionBar.SetDisplayShowHomeEnabled(!isRoot);
			SupportActionBar.SetDisplayHomeAsUpEnabled(!isRoot);
			SupportActionBar.SetHomeButtonEnabled(!isRoot);
			if(isRoot)
				drawerToggle.SyncState();
		}

		void showFtu()
		{
			var menu = toolbar.Menu;
			var view = menu.FindItem(Resource.Id.media_route_menu_item).ActionView;
			//if (view != null && view instanceof MediaRouteButton) {
			//	IntroductoryOverlay overlay = new IntroductoryOverlay.Builder(this)
			//			.setMenuItem(mMediaRouteMenuItem)
			//			.setTitleText(R.string.touch_to_cast)
			//			.setSingleTime()
			//			.build();
			//	overlay.show();
			//}
		}

		public void OnBackStackChanged()
		{
			UpdateDrawerToggle();
		}

		public void OnDrawerClosed(View drawerView)
		{
			drawerToggle?.OnDrawerClosed(drawerView);
			if (itemToOpenWhenDrawerCloses < 0)
				return;
			var extras =
				ActivityOptions.MakeCustomAnimation(this, Resource.Animation.fade_in, Resource.Animation.fade_out).ToBundle();
			Type activityClass = null;
			//TODO: clean up this navigation hell
			//Class activityClass = null;
			//switch (mItemToOpenWhenDrawerCloses)
			//{
			//	case R.id.navigation_allmusic:
			//		activityClass = MusicPlayerActivity.class;
   //                     break;
   //                 case R.id.navigation_playlists:
   //                     activityClass = PlaceholderActivity.class;
   //                     break;
   //             }
   //             if (activityClass != null) {
   //                 startActivity(new Intent(ActionBarCastActivity.this, activityClass), extras);
   //                 finish();
	//}
		}

		public void OnDrawerOpened(View drawerView)
		{
			drawerToggle?.OnDrawerOpened(drawerView);
			SupportActionBar?.SetTitle(Resource.String.home_title);
		}

		public void OnDrawerSlide(View drawerView, float slideOffset)
		{
			drawerToggle?.OnDrawerSlide(drawerView,slideOffset);
		}

		public void OnDrawerStateChanged(int newState)
		{
			drawerToggle?.OnDrawerStateChanged(newState);
		}

		public bool OnNavigationItemSelected(IMenuItem menuItem)
		{

			menuItem.SetChecked(true);
			itemToOpenWhenDrawerCloses = menuItem.ItemId;
			drawerLayout.CloseDrawers();
			return true;
		}
	}
}