using System;
using MonoTouch.Dialog;
using MusicPlayer.Managers;
using System.Linq;
using MusicPlayer.Api;
using System.Threading.Tasks;

namespace MusicPlayer.iOS
{
	public class ServicePickerViewController : DialogViewController
	{
		ServiceType? selectedService;
		public ServicePickerViewController () : base(null)
		{
			Root = new RootElement("Services") {
				new Section(){
					ApiManager.Shared.AvailableApiServiceTypes.Select(x=> new AccountCell(x,()=>{
						this.DismissModalViewController(true);
						selectedService = x;
					}))
				}
			};
			this.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Cancel, (sender, e) => {
				this.DismissModalViewController (true);
				tcs.TrySetCanceled ();
			});;
		}
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			if (selectedService.HasValue)
				tcs.TrySetResult (selectedService.Value);
		}
		TaskCompletionSource<ServiceType> tcs = new TaskCompletionSource<ServiceType> ();
		public Task<ServiceType> GetServiceTypeAsync ()
		{
			return tcs.Task;
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);

			this.StyleViewController ();
		}
	}
}
