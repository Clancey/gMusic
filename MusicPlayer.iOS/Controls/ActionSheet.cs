using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;

namespace MusicPlayer.iOS.Controls
{
	public class ActionSheet : IDisposable, IEnumerable
	{
		UIAlertController controller;
		UIActionSheet sheet;
		string title;
		string message;

		Dictionary<int, Action> dict = new Dictionary<int, Action>();

		public ActionSheet(string title, string message = "")
		{
			this.title = title;
			this.message = message;
			controller = UIAlertController.Create(title, message, UIAlertControllerStyle.ActionSheet);
			if (controller == null)
				SetupActionSheet();
		}

		void SetupActionSheet()
		{
			sheet = new UIActionSheet(title);
			sheet.Clicked += SheetOnClicked;
		}

		async void SheetOnClicked(object sender, UIButtonEventArgs e)
		{
			await Task.Delay(10);
			Action a;
			if (dict.TryGetValue((int) e.ButtonIndex, out a))
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
				var index = (int) sheet.AddButton(text);
				dict[index] = action;
				if (isCancel)
				{
					sheet.CancelButtonIndex = index;
				}
			}
		}

		public void Show(UIViewController inController, UIView fromView)
		{
			if (controller != null)
			{
				var pop = controller.PopoverPresentationController;
				if (pop != null)
				{
					pop.SourceView = fromView;
					pop.SourceRect = fromView.Bounds;
				}
				inController.PresentViewControllerAsync(controller, true);
			}
			else
			{
				var rect = fromView.Bounds;
				rect.Location = fromView.ConvertPointToView(rect.Location, inController.View);
				sheet.ShowFrom(rect, inController.View, true);
			}
		}

		public void Dispose()
		{
			dict = null;
			sheet?.Dispose();
			controller?.Dispose();
		}

		public IEnumerator GetEnumerator()
		{
			if(controller != null)
				return controller.Actions.GetEnumerator();
			else
				return dict.GetEnumerator();
		}
	}
}