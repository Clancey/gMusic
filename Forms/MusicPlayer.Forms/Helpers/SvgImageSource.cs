using System;
using Xamarin.Forms;
namespace MusicPlayer.Forms
{
	public class SvgImageSource : ImageSource
	{
		public SvgImageSource()
		{
		}
		public string SvgName { get; set; }
		public Size Size { get; set; }
	}
}
