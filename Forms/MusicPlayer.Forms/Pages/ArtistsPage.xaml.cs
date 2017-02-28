using System;
using System.Collections.Generic;

using Xamarin.Forms;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Forms
{
	public partial class ArtistsPage : ContentPage
	{
		public ArtistsPage()
		{
			BindingContext = new ArtistSongsViewModel();
			InitializeComponent();
		}
	}
}
