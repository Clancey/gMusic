using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace UIKit
{
	internal static class CGRectHelpers
	{
		public static CGPoint GetCenter(this CGRect rect)
		{
			return new CGPoint(rect.GetMidX(), rect.GetMidY());
		}

		public static CGRect WithHeight(this CGRect rect, nfloat height)
		{
			var frame = rect;
			frame.Height = height;
			return frame;
		}

		public static CGRect WithWidth(this CGRect rect, nfloat width)
		{
			var frame = rect;
			frame.Width = width;
			return frame;
		}

		public static CGRect WithX(this CGRect rect, nfloat x)
		{
			var frame = rect;
			frame.X = x;
			return frame;
		}

		public static CGRect WithY(this CGRect rect, nfloat y)
		{
			var frame = rect;
			frame.Y = y;
			return frame;
		}
	}
}