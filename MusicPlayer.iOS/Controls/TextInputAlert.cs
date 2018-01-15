using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using Localizations;

namespace MusicPlayer.iOS.Controls
{
	public class TextInputAlert : IDisposable
	{
		readonly string text;
		readonly string defaultText;
		readonly string buttonText;
		UIAlertController alertController;
		UIAlertView alertView;

		public TextInputAlert(string text, string defaultText = "", string buttonText = "Ok")
		{
			this.text = text;
			this.defaultText = defaultText;
			this.buttonText = buttonText;
			alertController = UIAlertController.Create(text, "", UIAlertControllerStyle.Alert);
			if (alertController != null)
				setupAlertController();
			else
			{
				setupAlertView();
			}
		}

		void setupAlertController()
		{
			UITextField textField = null;
			var cancel = UIAlertAction.Create(Strings.Nevermind, UIAlertActionStyle.Cancel, (alert) => { tcs.TrySetCanceled(); });
			var ok = UIAlertAction.Create(buttonText, UIAlertActionStyle.Default, a => { tcs.TrySetResult(textField.Text); });

			alertController.AddTextField(field =>
			{
				field.Text = defaultText;
				textField = field;
			});
			alertController.AddAction(ok);
			alertController.AddAction(cancel);
		}

		void setupAlertView()
		{
			alertView = new UIAlertView(text, "", null, Strings.Nevermind, buttonText)
			{
				AlertViewStyle = UIAlertViewStyle.PlainTextInput
			};
			alertView.Clicked += AlertViewOnClicked;
		}

		void AlertViewOnClicked(object sender, UIButtonEventArgs e)
		{
			if (e.ButtonIndex == alertView.CancelButtonIndex)
			{
				tcs.TrySetCanceled();
			}
			else
			{
				tcs.TrySetResult(alertView.GetTextField(0)?.Text);
			}
			alertView.Clicked -= AlertViewOnClicked;
		}

		TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();

		public Task<string> GetText(UIViewController fromController)
		{
			if (alertController != null)
			{
				fromController.PresentViewControllerAsync(alertController, true);
			}
			else
			{
				alertView.Show();
			}
			return tcs.Task;
		}

		public void Dispose()
		{
			alertController?.Dispose();
			alertView?.DismissWithClickedButtonIndex(alertView.CancelButtonIndex, true);
			alertView?.Dispose();
			tcs?.TrySetCanceled();
			tcs = null;
		}
	}
}