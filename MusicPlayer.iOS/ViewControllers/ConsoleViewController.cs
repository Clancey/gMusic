using System;
using MusicPlayer.iOS.ViewControllers;
using UIKit;
using MusicPlayer.Managers;
namespace MusicPlayer.iOS
{
	public class ConsoleViewController : BaseViewController
	{
		UITextView TextView => (UITextView)View;
		public ConsoleViewController()
		{
			Title = "Console";
		}
		public override void LoadView()
		{
			View = new UITextView() { Editable = false };
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			NotificationManager.Shared.ConsoleChanged += Shared_ConsoleChanged;
			TextView.Text = InMemoryConsole.Current.ToString();
		}
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			NotificationManager.Shared.ConsoleChanged -= Shared_ConsoleChanged;
		}

		void Shared_ConsoleChanged(object sender, EventArgs e)
		{
			TextView.Text = InMemoryConsole.Current.ToString();
		}
	}
}
