using System;
using AppKit;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class ConsoleViewController : NSViewController
	{

		NSTextView textView;
		public ConsoleViewController()
		{
			var scroll =new NSScrollView(new CoreGraphics.CGRect(0,0,600, 400))
			{
				HasVerticalScroller = true,
				HasHorizontalScroller = true,
				//BorderType = NSBorderType.NoBorder,
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
			};
			var contentSize = scroll.ContentSize;
			textView = new NSTextView
			{
				Frame = new CoreGraphics.CGRect(CoreGraphics.CGPoint.Empty,contentSize),
				VerticallyResizable = true,
				HorizontallyResizable = true,
				AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable,
				MinSize = new CoreGraphics.CGSize(0,contentSize.Height),
				MaxSize = new CoreGraphics.CGSize(nfloat.MaxValue,nfloat.MaxValue),
				TextContainer = {
					ContainerSize = new CoreGraphics.CGSize(nfloat.MaxValue,nfloat.MaxValue),
					//WidthTracksTextView = true,
					//HeightTracksTextView = true,
				},
			};
			scroll.DocumentView = textView;
			View = scroll;

		}
		public override void ViewDidAppear()
		{
			base.ViewDidAppear();
			textView.Value = InMemoryConsole.Current.ToString();
			NotificationManager.Shared.ConsoleChanged += Shared_ConsoleChanged;

		}

		void Shared_ConsoleChanged(object sender, EventArgs e)
		{
			textView.Value = InMemoryConsole.Current.ToString();
		}
		public override void ViewDidLayout()
		{
			base.ViewDidLayout();
			textView.MinSize = View.Bounds.Size;

		}
		public override void ViewDidDisappear()
		{
			base.ViewDidDisappear();
			NotificationManager.Shared.ConsoleChanged -= Shared_ConsoleChanged;
		}
	}
}
