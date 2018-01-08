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
		SimpleButton iflButton;
		public RadioStationView ()
		{
			iflButton = new SimpleButton
			{
				Clicked = async (obj) =>
				{
					await PlaybackManager.Shared.Play(new RadioStation("I'm Feeling Lucky")
					{
						Id = "IFL",
					});
				},
				Title = "I'm Feeling Luck",

			};
			AddSubview(iflButton);
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
		public override bool IsFlipped => true;
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			iflButton.SizeToFit();
			var bounds = Bounds;
			bounds.Y += iflButton.Frame.Bottom;
			bounds.Height -= iflButton.Frame.Bottom;
			tabView.Frame = bounds;
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

