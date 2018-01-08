using System;
using AppKit;
namespace AppKit
{
	public class SimpleButton : NSButton
	{
		public Action<SimpleButton> Clicked { get; set; }
		public SimpleButton()
		{
			//button.Image = svg.LoadImageFromSvg(new NGraphics.Size(25, 25));
			Bordered = false;
			Activated += (sender, e) => Clicked?.Invoke(this);
			//button.ImagePosition = NSCellImagePosition.ImageOnly;
		}

	}
}
