using System;

using Xamarin.Forms;
using MusicPlayer.ViewModels;

namespace MusicPlayer.Forms
{
	public class BasePage : ContentPage
	{
		protected override void OnAppearing()
		{
			(BindingContext as BaseViewModel)?.OnViewAppearing();
			base.OnAppearing();
		}
		protected override void OnDisappearing()
		{
			(BindingContext as BaseViewModel)?.OnViewDissapearing();
			base.OnDisappearing();
		}
	}
}

