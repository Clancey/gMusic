using System;
using System.Collections.Generic;
using System.Text;
using AppKit;

namespace MusicPlayer
{
	internal static class Fonts
	{
		public static string NormalFontName => "SFUIText-Regular";

		public static NSFont NormalFont(nfloat size)
		{
			return NSFont.FromFontName(NormalFontName, size);
		}

		public static string ThinFontName => "SFUIDisplay-Thin";

		public static NSFont ThinFont(nfloat size)
		{
			return NSFont.FromFontName(ThinFontName, size);
		}
	}
}