using System;
using AppKit;
using CoreAnimation;
using Foundation;
using CoreGraphics;

namespace MusicPlayer
{
	public class NSAnimatedSlider : NSControl
	{
		NSColorView progressBackground;
		NSColorView thumb;
		public Action<float> ValueChanged { get; set; }

		public NSAnimatedSlider ()
		{
			init ();
		}

		public NSAnimatedSlider (IntPtr handle) : base (handle)
		{
			init ();
		}

		void init ()
		{
			AddSubview (progressBackground = new NSColorView{ BackgroundColor = Style.Current.AccentColorHorizontal });
			AddSubview (thumb = new NSColorView{ BackgroundColor = NSColor.DarkGray });
			this.AcceptsTouchEvents = true;
		}

		public override bool IsFlipped {
			get {
				return true;
			}
		}
	
		public override void DrawRect (CGRect dirtyRect)
		{
			var context = NSGraphicsContext.CurrentContext.GraphicsPort;
			context.SetFillColor(NSColor.Clear.CGColor);
			context.FillRect (dirtyRect);
			base.DrawRect (dirtyRect);
		}
		public override Foundation.NSObject AnimationFor (Foundation.NSString key)
		{
			if (key == "Value")
				return new CABasicAnimation ();
			return base.AnimationFor (key);
		}

		float val;
		public float Value {
			get {
				return val;
			}
			set {
				if (value < 0)
					value = 0;
				else if (value > 1)
					value = 1;						
				this.val = value;
				//AnimateSizeChange ();
				this.ResizeSubviewsWithOldSize (Bounds.Size);
			}
		}

		public void SetFloat (float value)
		{
			val = value;
			AnimateSizeChange ();
		}
		const float thumbHeight = 10f;
		const float trackHeight = 2f;
		const float thumbWidth = 3f;
		public override void ResizeSubviewsWithOldSize (CoreGraphics.CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);

			var bounds = Bounds;
			var frame = bounds;
			frame.Height = trackHeight;
			frame.Width *= Value;

			progressBackground.Frame = frame;

			var x = frame.Right;
			frame.Height = thumbHeight;
			frame.Width = thumbWidth;
			frame.X = x - (thumbWidth/2);
			thumb.Frame = frame;
		}

		public void AnimateSizeChange()
		{

			var bounds = Bounds;
			var frame = bounds;
			frame.Height = trackHeight;
			frame.Width *= Value;

			NSAnimationContext.BeginGrouping ();
			NSAnimationContext.CurrentContext.Duration = .5;
			((NSView)progressBackground.Animator).Frame = frame;

			var x = frame.Right;
			frame.Height = thumbHeight;
			frame.Width = thumbWidth;
			frame.X = x - (thumbWidth/2);

			((NSView)thumb.Animator).Frame = frame;
			NSAnimationContext.EndGrouping();
		}
		public override void MouseDown (NSEvent theEvent)
		{
			var point = this.ConvertPointFromView (theEvent.LocationInWindow, this.Window.ContentView);
			SetValue (point);
		}
		public override void MouseDragged (NSEvent theEvent)
		{
			var point = this.ConvertPointFromView (theEvent.LocationInWindow, this.Window.ContentView);
			SetValue (point);
		}
		public override void MouseMoved (NSEvent theEvent)
		{
			var point = this.ConvertPointFromView (theEvent.LocationInWindow, this.Window.ContentView);
			SetValue (point);
		}
		public override void MouseUp (NSEvent theEvent)
		{
			var point = this.ConvertPointFromView (theEvent.LocationInWindow, this.Window.ContentView);
			SetValue (point);
		}

		void SetValue(CGPoint point)
		{
			var v = point.X / Bounds.Width;
			var oldValue = val;
			Value = (float)v;
			if ((oldValue - Value).IsNotZero())
				ValueChanged?.Invoke (Value);
		}
	}
}

