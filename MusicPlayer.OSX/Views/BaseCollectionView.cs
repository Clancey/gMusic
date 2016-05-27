using System;
using AppKit;
using SimpleTables;
using CoreGraphics;
using Foundation;
using ObjCRuntime;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public class BaseCollectionView<TVM,T> : NSView, ILifeCycleView where TVM : TableViewModel<T> where T: class
	{
		public  NSCollectionView CollectionView{get{ return container.DocumentView as NSCollectionView; }}
		CollectionView container;
		TVM model;
		public TVM Model {
			get {
				return model;
			}
			set {
				model = value;
				if (this.Superview != null) {
					Source = new BaseCollectionSource<T> (value);
					CollectionView.DataSource = Source;
					CollectionView.Delegate = Source;
					CollectionView.ReloadData ();
				}
			}
		}
		protected BaseCollectionSource<T> Source { get; set; }
		public BaseCollectionView ()
		{
			NSArray array;
			NSBundle.MainBundle.LoadNibNamed ("CollectionView", this, out array);
			for (nuint i = 0; i < array.Count; i++) {
				try{
					container = Runtime.GetNSObject<CollectionView> (array.ValueAt (i));
					break;
				}
				catch(Exception) {

				}
			}

			CollectionView.CollectionViewLayout = new NSCollectionViewFlowLayout {
				ItemSize = new CGSize (175, 215),
				MinimumLineSpacing = 20f,
				MinimumInteritemSpacing = 5f,
			};
			CollectionView.Selectable = true;
			AddSubview (container);
		}
		public override void ResizeSubviewsWithOldSize (CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			container.Frame = Bounds;
		}

		#region ILifeCycleView implementation

		public virtual async void ViewWillAppear ()
		{
			if (CollectionView.Delegate == null) {
				Source = new BaseCollectionSource<T> (Model);
				CollectionView.DataSource = Source;
				CollectionView.Delegate = Source;
			}
			await Task.Delay(10);
			CollectionView.ReloadData ();
		}

		public virtual void ViewWillDissapear ()
		{
			
		}

		#endregion
	}
}

