using System;
using AppKit;
using ITSwitch;
using CoreGraphics;

namespace MusicPlayer
{
	public class MenuSwitchElement : MenuImageElement
	{
		public bool Value{ get; set; }
		public string Subtext {get;set;}
		public Action<bool> ValueChanged { get; set; }

		public MenuSwitchElement ()
		{
		}

		public override NSView GetView (NSTableView tableView, Foundation.NSObject sender)
		{
			var cell = tableView.MakeView (MenuSwitchCell.Key, sender) as MenuSwitchCell ?? new MenuSwitchCell ();
			cell.Element = this;
			return cell;
		}

		public class MenuSwitchCell : NSView
		{
			public const string Key = "MenuSwitchCell";
			TwoLabelView textView;
			NSImageView imageView;
			ITSwitchView switchView;

			public MenuSwitchCell ()
			{
				Identifier = Key;
				AddSubview (imageView = new NSImageView (new CGRect (0, 0, imageWidth, imageWidth)));
				AddSubview (textView = new TwoLabelView ());
				AddSubview (switchView = new ITSwitchView(new CGRect(20,103,32,20)) {
					TintColor = Style.Current.AccentColor,
				});
				switchView.OnSwitchChanged += (object sender, EventArgs e) => Element?.ValueChanged?.Invoke (switchView.IsOn);
			}

			public override bool IsFlipped {
				get {
					return true;
				}
			}

			WeakReference _element;

			public MenuSwitchElement Element {
				get {
					return _element?.Target as MenuSwitchElement;
				}
				set {
					_element = new WeakReference (value);
					UpdateValues ();
				}
			}

			public void UpdateValues ()
			{
				var element = Element;
				if (element == null)
					return;
				textView.TopLabel.StringValue = element?.Text ?? "";
				textView.BottomLabel.StringValue = element?.Subtext ?? "";
				switchView.IsOn = element?.Value ?? false;
				imageView.LoadSvg (element?.Svg, NSColor.ControlText);
			}

			const float padding = 8;
			const float imageWidth = 16;

			public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
			{
				base.ResizeSubviewsWithOldSize (oldSize);
				var bounds = Bounds;

				var frame = imageView.Frame;
				frame.Y = (bounds.Height - frame.Height) / 2;
				imageView.Frame = frame;

				var x = frame.Right + padding;

				frame = switchView.Frame;
				frame.Y = (bounds.Height - frame.Height) / 2;
				frame.X = bounds.Width - frame.Width - padding;
				switchView.Frame = frame;

				var right = frame.Left;
				textView.Frame = new CGRect (x, 0, right - padding - x, bounds.Height);


			}
		}
	}
}

