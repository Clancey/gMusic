using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UIKit;
using System.Collections;

namespace MusicPlayer.iOS
{
	class AlertView : IDisposable, IEnumerable
	{
		UIAlertController controller;
		UIAlertView sheet;
		string title;
		string message;

		Dictionary<int, Action> dict = new Dictionary<int, Action>();

		public AlertView(string title, string message = "")
		{
			this.title = title;
			this.message = message;
			controller = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
			if (controller == null)
				SetupActionSheet();
		}

		void SetupActionSheet()
		{
			sheet = new UIAlertView(title,message,null,"Ok");
			sheet.Clicked += SheetOnClicked;
		}

		async void SheetOnClicked(object sender, UIButtonEventArgs e)
		{
			await Task.Delay(10);
			Action a;
			if (dict.TryGetValue((int)e.ButtonIndex, out a))
				a?.Invoke();
			sheet.Clicked -= SheetOnClicked;
		}

		public void Add(string text, Action action, bool isCancel = false)
		{
			if (controller != null)
			{
				controller.AddAction(UIAlertAction.Create(text, isCancel ? UIAlertActionStyle.Cancel : UIAlertActionStyle.Default,
					alert => { action?.Invoke(); }));
			}
			else
			{
				var index = (int)sheet.AddButton(text);
				dict[index] = action;
				if (isCancel)
				{
					sheet.CancelButtonIndex = index;
				}
			}
		}

		public void Show(UIViewController inController)
		{
			if (controller != null)
			{
				inController.PresentViewControllerAsync(controller, true);
			}
			else
			{
				sheet.Show();
			}
		}

		public void Dispose()
		{
			dict = null;
			sheet?.Dispose();
			controller?.Dispose();
		}

		#region IEnumerable implementation

		public IEnumerator GetEnumerator ()
		{
			return dict.GetEnumerator ();
		}

		#endregion
	}
}