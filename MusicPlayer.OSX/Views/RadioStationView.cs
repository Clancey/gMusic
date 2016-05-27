using System;
using AppKit;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using Foundation;
using System.Threading.Tasks;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class RadioStationView :  NSView, ILifeCycleView
	{
		NSTabView tabView;
		RadioStationListView recent;
		RadioStationListView stations;
		public RadioStationView ()
		{
			tabView = new NSTabView ();

			tabView.Add (new NSTabViewItem ((NSString)"Recent") {
				Label = "Recent",
				ToolTip = "Recent radio stations",
				View = (recent = new RadioStationListView {
					IsIncluded = false,
				}),
			});
			tabView.Add (new NSTabViewItem ((NSString)"My Stations") {
				Label = "My Stations",
				ToolTip = "My Radio Stations",
				View = (stations = new RadioStationListView {
					IsIncluded = true,
				}),
			});
			tabView.DidSelect += async (object sender, NSTabViewItemEventArgs e) => {
				await Task.Delay(1);
				recent.CollectionView.ReloadData ();
				stations.CollectionView.ReloadData ();
			};
			AddSubview (tabView);
		}
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			tabView.Frame = Bounds;
		}
		public override async void ViewWillMoveToSuperview (NSView newSuperview)
		{
			base.ViewWillMoveToSuperview (newSuperview);

			await Task.Delay(10);
		}

		#region ILifeCycleView implementation

		public void ViewWillAppear ()
		{
			recent.ViewWillAppear();
			stations.ViewWillAppear();
			NotificationManager.Shared.RadioDatabaseUpdated += NotificationManager_Shared_RadioDatabaseUpdated;
		}

		void NotificationManager_Shared_RadioDatabaseUpdated (object sender, EventArgs e)
		{
			recent.CollectionView.ReloadData();
			stations.CollectionView.ReloadData();
		}

		public void ViewWillDissapear ()
		{
			recent.ViewWillDissapear();
			stations.ViewWillDissapear();
			NotificationManager.Shared.RadioDatabaseUpdated -= NotificationManager_Shared_RadioDatabaseUpdated;
		}

		#endregion
	}
}

