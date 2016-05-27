using System;
using AppKit;
using System.Collections.Generic;
using CoreGraphics;

namespace MusicPlayer
{
	public class NSNavigationController : NSView, ILifeCycleView
	{
		TitleBar Toolbar;
		NSView MainContentView;
		NSView currentView;
		Stack<NSView> BackStack = new Stack<NSView>();
		public NSNavigationController(NSView currentView) : this()
		{
			Push (currentView);
		}
		public NSNavigationController ()
		{
			AddSubview (Toolbar = new TitleBar ());
			AddSubview(MainContentView = new NSView ());
		}

		public void Push(NSView view)
		{
			BackStack.Push (view);
			SwitchContent (view);
		}

		public void Pop()
		{
			BackStack.Pop ();

			var next = BackStack.Peek ();
			SwitchContent (next);
		}

		protected void SwitchContent(NSView view)
		{

			var life = currentView as ILifeCycleView;
			life?.ViewWillDissapear ();
			currentView?.RemoveFromSuperview ();
			view.Frame = MainContentView.Bounds;
			currentView = view;
			view.AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable;

			life = view as ILifeCycleView;
			life?.ViewWillAppear ();

			var navItem = view as INavigationItem;
			Toolbar.Title = navItem?.Title ?? "";
			if (navItem != null) {
				navItem.NavigationController = this;
			}
			Toolbar.BackButtonHidden = BackStack.Count <= 1;

			MainContentView.AddSubview (view);
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}

		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var bounds = Bounds;
			var frame = Toolbar.Frame;
			frame.Width = bounds.Width;
			Toolbar.Frame = frame;

			frame.Y = frame.Bottom;
			frame.Height = bounds.Height - frame.Y;
			MainContentView.Frame = frame;
		}

		#region ILifeCycleView implementation
		public void ViewWillAppear ()
		{
			var life = currentView as ILifeCycleView;
			life?.ViewWillAppear ();
			Toolbar.BackButtonPressed = Pop;
		}
		public void ViewWillDissapear ()
		{
			var life = currentView as ILifeCycleView;
			life?.ViewWillDissapear ();
			Toolbar.BackButtonPressed = null;
		}
		#endregion

		class TitleBar : NSColorView
		{
			public string Title {
				get {
					return titleField.StringValue;
				}
				set {
					titleField.StringValue = value;
					ResizeSubviewsWithOldSize (Bounds.Size);
				}
			}

			public bool BackButtonHidden {
				get {
					return backButton.Hidden;
				}
				set {
					backButton.Hidden = value;
				}
			}

			public Action BackButtonPressed {get;set;}

			NSTextField titleField;
			NSButton backButton;
			public TitleBar () : base(new CGRect(9,0,100,44))
			{
				AddSubview(titleField = new NSTextField().StyleAsHeaderText());
				AddSubview(backButton = new NSButton(){Title = "Back"});
				backButton.Activated += (object sender, EventArgs e) => BackButtonPressed?.Invoke();

			}
			public override bool IsFlipped {
				get {
					return true;
				}
			}

			public override void ResizeSubviewsWithOldSize (CGSize oldSize)
			{
				base.ResizeSubviewsWithOldSize (oldSize);
				var bounds = Bounds;

				titleField.SizeToFit ();
				var frame = titleField.Frame;
				frame.X = (bounds.Width - frame.Width) / 2;
				frame.Y = (bounds.Height - frame.Height) / 2;

				titleField.Frame = frame;

				backButton.SizeToFit ();
				frame = backButton.Frame;
				frame.X = 10f;
				frame.Y = (bounds.Height - frame.Height) / 2;
				backButton.Frame = frame;

			}
		}
	}
}

