using System;
using Localizations;
using Xamarin.Forms;

namespace MusicPlayer.Forms
{
	public class IntroPage : ContentPage
	{
		BlurView blurView;
		public IntroPage()
		{
			BackgroundImage = "launchBg";
			var layout = new RelativeLayout();
			var topImage = new Image { Source = new FileImageSource { File = "headphones" } };
			var headerLabel = new Label { TextColor = Color.White,
				FontFamily = MusicPlayer.Style.DefaultStyle.HeaderTextThinFont,
				FontSize = MusicPlayer.Style.DefaultStyle.HeaderTextFontSize,
				Text = Strings.WelcomToGmusic,
				HorizontalTextAlignment = TextAlignment.Center
			};
			var loginButton = new Button { Text = Strings.Login }.StyleAsBorderedButton();
			var skipButton = new Button { Text = Strings.Skip }.StyleAsTextButton();

			Func<double> getImageWidth = () => Math.Min(layout.Width / 2 * .6, 512);
			Func<double> getSkipButtonHeight = () => skipButton.Measure(layout.Width, layout.Height).Request.Height;
			Func<double> getLoginButtonHeight = () =>
			{
				return loginButton.Measure(layout.Width, layout.Height).Request.Height;
			};

			layout.Children.Add(topImage,
								Constraint.RelativeToParent((parent) => parent.Width / 2 - getImageWidth() / 2),
								Constraint.RelativeToParent((parent) => parent.Height / 4),
								Constraint.FromExpression(() => getImageWidth()),
								Constraint.FromExpression(() => getImageWidth())
							   );
			layout.Children.Add(headerLabel, yConstraint: Constraint.RelativeToView(topImage, (l, v) => v.Y + v.Height + 20),
								widthConstraint: Constraint.RelativeToParent((arg) => arg.Width));


			layout.Children.Add(skipButton,
								xConstraint: Constraint.RelativeToParent((parent) => parent.Width / 4),
			                    yConstraint: Constraint.RelativeToParent((parent) => (parent.Y + parent.Height) - getSkipButtonHeight() - 30),
								widthConstraint: Constraint.RelativeToParent((parent) => parent.Width/2));


			layout.Children.Add(loginButton,
								xConstraint: Constraint.RelativeToParent((parent) => parent.Width / 4),
			                    yConstraint: Constraint.RelativeToView(skipButton, (l, v) =>  v.Y - getLoginButtonHeight() - 10),
								widthConstraint: Constraint.RelativeToParent((arg) => arg.Width / 2));

			Content = blurView =new BlurView { Content = layout, BlurStyle = BlurStyle.Light };
		}
	}
}

