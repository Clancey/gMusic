using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MusicPlayer.Api;
using Xamarin.Forms;
using MusicPlayer.Managers;
using System.Linq;

namespace MusicPlayer.Forms
{
	public partial class ServicePickerPage : ContentPage
	{
		public AccountCellModel[] Services { get; set; } = ApiManager.Shared.AvailableApiServiceTypes.Select(x => new AccountCellModel { ServiceType = x }).ToArray();
		public ServicePickerPage()
		{
			InitializeComponent();
			this.ServicesList.ItemsSource = Services;
		}

		TaskCompletionSource<ServiceType> tcs = new TaskCompletionSource<ServiceType>();

		public Task<ServiceType> GetServiceTypeAsync()
		{
			return tcs.Task;
		}

		async void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
		{
			await this.Navigation.PopModalAsync();
			var model = (AccountCellModel)e.SelectedItem;
			tcs.TrySetResult(model.ServiceType);

		}
	}
}
