using System;
using AppKit;

namespace MusicPlayer
{
	public class BaseSplitView<T> : NSSplitView,ILifeCycleView ,INSSplitViewDelegate where T:NSView
	{
		public float MaxSideBarWidth { get; set;} = 250;
		public float MinSideBarWidth { get; set;} = 100;
		public T SideBar { get; private set; }

		NSView contentView;

		NSView currentView;
		public NSView CurrentView {
			get {
				return currentView;
			}
			set {
				SetContent (value);
			}
		}

		public BaseSplitView(T sidebar)
		{
			this.DividerStyle = NSSplitViewDividerStyle.Thin;
			this.IsVertical = true;
			sidebar.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;
			AddSubview (SideBar = sidebar);

			AddSubview (contentView = new NSView {
				Frame = new CoreGraphics.CGRect(0,0,500,1000),
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
			});

			this.Delegate = this;
		}


		public void SetContent(NSView view)
		{

			var life = currentView as ILifeCycleView;
			life?.ViewWillDissapear ();
			currentView?.RemoveFromSuperview ();
			view.Frame = contentView.Bounds;
			currentView = view;
			view.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;


			life = view as ILifeCycleView;
			life?.ViewWillAppear ();

			contentView.AddSubview (view);
		}


		[Foundation.Export ("splitView:constrainSplitPosition:ofSubviewAt:")]
		public System.nfloat ConstrainSplitPosition (AppKit.NSSplitView splitView, System.nfloat proposedPosition, System.nint subviewDividerIndex)
		{
			return NMath.Min(MaxSideBarWidth,  NMath.Max (proposedPosition, MinSideBarWidth));
		}

		public override void ViewWillMoveToSuperview (NSView newSuperview)
		{
			base.ViewWillMoveToSuperview (newSuperview);
			AdjustSubviews ();
		}

		#region ILifeCycleView implementation

		public virtual void ViewWillAppear ()
		{
			
		}

		public virtual void ViewWillDissapear ()
		{
			
		}

		#endregion
	}
}

