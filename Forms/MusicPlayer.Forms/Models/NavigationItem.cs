using System;
using Xamarin.Forms;
namespace MusicPlayer.Forms
{
	public class NavigationHeaderItem : NavigationItem
	{
		public NavigationHeaderItem(string name) : base(name, "", null)
		{
			SaveIndex = false;
		}
	}
	public class NavigationItem
	{
		public enum NavigationItemType
		{
			Item,
			Header
		}

		public NavigationItem(string name, string svg, Page page) : this(name,svg,28, page)
		{

		}
		public NavigationItem(string name, string svg, double size, Page page) 
		{
			if(page != null)
				Page = new NavigationPage(page) { Title = page.Title };
			Title = name;
			Image = svg;
			ImageSize = size;
		}
		public bool HasImage => !string.IsNullOrWhiteSpace(Image);
		public NavigationItemType ItemType { get; set; } = NavigationItemType.Item;
		public Page Page { get; set; }
		public string Image { get; set; }
		public double ImageSize { get; set; }
		public string Title { get; set; }
		public bool SaveIndex { get; set; } = true;

		public ImageSource ImageSource => string.IsNullOrWhiteSpace(Image) ? null : new SvgImageSource { Size = new Size(ImageSize, ImageSize), SvgName = Image };
	}
}
