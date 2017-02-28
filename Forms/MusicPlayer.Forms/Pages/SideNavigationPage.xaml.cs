using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MusicPlayer.Forms
{
	public partial class SideNavigationPage : ContentPage
	{
		public SideNavigationPage()
		{
			Title = "gMusic";
			InitializeComponent();
		}

		void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
		{
			var item = e.SelectedItem as NavigationItem;
			if (item == null)
				return;
			(BindingContext as RootPage).Navigate(item);
			(sender as ListView).SelectedItem = null;
		}
	}
}
