using System;
using MusicPlayer.Managers;
using UIKit;
using CoreGraphics;
using System.Threading.Tasks;
using Localizations;

namespace MusicPlayer.iOS
{
	public class IntroViewController : UIViewController
	{
		public IntroViewController()
		{
			Title = "Welcome";
			//EdgesForExtendedLayout = UIRectEdge.None;
			this.NavigationItem.RightBarButtonItem = new UIBarButtonItem(UIBarButtonSystemItem.Add, async (s, e) =>
			{
				try
				{
					var api = ApiManager.Shared.CreateApi(MusicPlayer.Api.ServiceType.Google);
					api.ResetData();
					var account = await api.Authenticate();
					if (account == null)
						return;
					ApiManager.Shared.AddApi(api);
					ApiManager.Shared.CreateYouTube();
					ApiManager.Shared.GetMusicProvider(Api.ServiceType.YouTube).SyncDatabase();
					var manager = ApiManager.Shared.GetMusicProvider(api.Identifier);
					using (new Spinner("Syncing Database"))
					{
						await manager.Resync();
					}
					await this.DismissViewControllerAsync(true);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex);
				}
			});
		}
		public async Task Login()
		{
			try
			{
				var api = ApiManager.Shared.CreateApi(MusicPlayer.Api.ServiceType.Google);
				var account = await api.Authenticate();
				if (account == null)
					return;
				ApiManager.Shared.AddApi(api);
				var manager = ApiManager.Shared.GetMusicProvider(api.Identifier);
				using (new Spinner("Syncing Database"))
				{
					await manager.SyncDatabase();
				}
				await this.DismissViewControllerAsync(true);
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		IntroView view;
		public override void LoadView()
		{
			View = view = new IntroView();
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			view.login.Tapped = (b)=> Login();
        }
		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			view.login.Tapped = null;
		}

		class IntroView : UIView
		{
			UITextView textView;
			UITextView headerText;
			UIImageView image;
			public SimpleButton login;
			UIButton signinLater;
			UIView blurView;
            public IntroView()
			{
				BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle("launchBg"));
				Add(blurView = new BluredView(UIBlurEffectStyle.Light));
				Add(image = new UIImageView(UIImage.FromBundle("headphones")));
				if (!Device.IsIos8)
				{
					blurView.Alpha = 0;
					image.Alpha = 0;
				}

				Add(headerText = new UITextView
				{
					Text = Strings.WelcomToGmusic,
					TextAlignment = UITextAlignment.Center,
					TextColor = UIColor.White,
					Font = Style.DefaultStyle.HeaderTextThinFont,
					BackgroundColor = UIColor.Clear,
				});
				//Add(textView = new UITextView {
				//	Text = Text,
				//	TextAlignment = UITextAlignment.Center,
				//	TextColor = UIColor.White,
				//	BackgroundColor = UIColor.Clear,
				//});

				Add(login = new SimpleButton {
					Text = Strings.Login,
				}.StyleAsBorderedButton());
				login.SizeToFit();
				//Add(signinLater = new SimpleButton
				//{
				//	Text = "Login",
				//	Tapped = Cancel,
				//});
			}
		
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				var bounds = Bounds;
				var x = bounds.Width/4;
				var width = x*2;
				var y = bounds.Height / 4;
				blurView.Frame = bounds;

				var imageWidth =  Math.Min(image.Image.Size.Width,width *.6);
				var imageX = (bounds.Width - imageWidth)/2;
				image.Frame = new CGRect(imageX,y,imageWidth,imageWidth);

				y = image.Frame.Bottom + 10f;

				var size = headerText.SizeThatFits(new CGSize(width,1000));

				headerText.Frame = new CGRect(x,y,width,size.Height);

				y = headerText.Frame.Bottom + 10;

				var frame = login.Frame;
				frame.Width = width;
				frame.X = x;
				frame.Y = bounds.Bottom - frame.Height - 30f;
				login.Frame = frame;

				//textView.Frame = new CGRect(x,y,width,frame.Y - y);


			}
		}
	}
}