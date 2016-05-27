using System;
using AppKit;
using MusicPlayer.Models;
using MusicPlayer.ViewModels;
using System.Linq;
using Foundation;
using System.Threading.Tasks;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class AlbumView :  NSSplitView, ILifeCycleView
	{
		AlbumDetailView albumDetailsView;
		BaseCollectionView<AlbumViewModel,Album> CollectionView;
		public AlbumView ()
		{
			this.DividerStyle = NSSplitViewDividerStyle.Thin;
			this.IsVertical = false;
			AddSubview (CollectionView = new BaseCollectionView<AlbumViewModel, Album>(){
				Frame = new CoreGraphics.CGRect(0,0,500,1000),
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
				Model = new AlbumViewModel
				{
					GroupInfo = {
						GroupBy = "",
					}
				}
			});

			var vc = new AlbumDetailViewController ();
			albumDetailsView = vc.View;
		}

		public void ShowAlbumDetails()
		{
			var height = albumDetailsView.GetHeight ();
			albumDetailsView.Frame = new CoreGraphics.CGRect (0, 0, 500, height);
			if (albumDetailsView.Superview == null) {
				AddSubview (albumDetailsView);
			}
			AdjustSubviews ();
		}

		public void HideAlbumDetails()
		{

			if (albumDetailsView.Superview == null)
				return;
			albumDetailsView.RemoveFromSuperview ();
			AdjustSubviews ();
		}

		#region ILifeCycleView implementation

		public void ViewWillAppear ()
		{
			AdjustSubviews ();
			CollectionView.ViewWillAppear ();
			CollectionView.Model.ItemSelected += Model_ItemSelected;
			NotificationManager.Shared.SongDatabaseUpdated += NotificationManager_Shared_SongDatabaseUpdated;
		}

		void NotificationManager_Shared_SongDatabaseUpdated (object sender, EventArgs e)
		{
			this.CollectionView.CollectionView.ReloadData ();
		}

		async void Model_ItemSelected (object sender, SimpleTables.EventArgs<Album> e)
		{
			Console.WriteLine(e.Data);
			albumDetailsView.Album = e.Data;
			ShowAlbumDetails ();
			//await Task.Delay (5000);
			//HideAlbumDetails ();
		}

		public void ViewWillDissapear ()
		{
			CollectionView.ViewWillDissapear ();
			CollectionView.Model.ItemSelected -= Model_ItemSelected;
			NotificationManager.Shared.SongDatabaseUpdated -= NotificationManager_Shared_SongDatabaseUpdated;
		}
		[Export ("splitView:constrainSplitPosition:ofSubviewAt:")]
		public System.nfloat ConstrainSplitPosition (AppKit.NSSplitView splitView, System.nfloat proposedPosition, System.nint subviewDividerIndex)
		{

			var height = albumDetailsView.GetHeight ();
			//albumDetailsView.Frame = new CoreGraphics.CGRect (0, 0, 500, height);
			return NMath.Max (proposedPosition, height);
		}

		#endregion
	}
}

