using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace MusicPlayer.iOS
{
	internal static class Fonts
	{
		public static string NormalFontName => "SFUIText-Regular";

		public static UIFont NormalFont (nfloat size)
		{
			return UIFont.FromName (NormalFontName, size) ?? UIFont.SystemFontOfSize (size);
		}

		public static string ThinFontName => "SFUIDisplay-Thin";

		public static UIFont ThinFont (nfloat size)
		{
			return UIFont.FromName (ThinFontName, size) ?? UIFont.SystemFontOfSize (size);
		}
	}
}