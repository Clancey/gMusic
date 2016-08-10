using System;
using System.Collections.Generic;
using UIKit;
using System.Linq;
using MusicPlayer.Data;
using MusicPlayer.Managers;

namespace MusicPlayer.iOS
{
	public class Style
	{
		static Style()
		{
			AvailableStyles = new List<Style>
			{
				new Style(),
				new DarkStyle(),
			};

			var currentStyle = AvailableStyles.FirstOrDefault(x => x.Id == Settings.CurrentStyle) ?? AvailableStyles[0];

			UIApplication.SharedApplication.StatusBarStyle = currentStyle.StatusBarColor;
			Styles = new Dictionary<nint, Style>
			{
				{0,currentStyle},
				{1,new CarStyle()}
			};
		}

		public static void SetStyle()
		{

			var style = AvailableStyles.FirstOrDefault(x => x.Id == Settings.CurrentStyle) ?? AvailableStyles[0];
			UIApplication.SharedApplication.SetStatusBarStyle(style.StatusBarColor, true);
			Styles[0] = style;
		}
		public static Style DefaultStyle => Styles[0];
		public string Id { get; set; } = "Default";

		public static List<Style> AvailableStyles { get; private set; }

		public static Dictionary<nint, Style> Styles { get; private set; }
		public Style()
		{

		}
		public static bool IsDeviceDark { get; set; }
		public UIColor AccentSolidColor { get; set; } = UIColor.FromRGB(255, 43, 103);
		public UIColor AccentColor => IsDeviceDark ? AccentSolidColor : AccentGradientColor;
		public UIColor AccentGradientColor { get; set; } = UIColor.FromPatternImage(UIImage.FromBundle("accentColor"));
		public UIColor AccentColorGradientHorizontal { get; set; } = UIColor.FromPatternImage(UIImage.FromBundle("accentColor").Rotate());
		public UIColor AccentColorHorizontal => IsDeviceDark ? AccentSolidColor : AccentColorGradientHorizontal;

		public UIFont HeaderTextFont { get; set; } = Fonts.NormalFont(28);
		public UIFont HeaderTextThinFont { get; set; } = Fonts.ThinFont(28);

		public UIStatusBarStyle StatusBarColor { get; set; } = UIStatusBarStyle.Default;

		UIColor headerTextColor;

		public UIColor HeaderTextColor
		{
			get { return headerTextColor ?? (headerTextColor = AccentColor); }
			set { headerTextColor = value; }
		}

		public UIFont MainTextFont { get; set; } = Fonts.NormalFont(15);
		public UIColor MainTextColor { get; set; } = UIColor.Black;

		public UIFont ButtonTextFont { get; set; } = Fonts.NormalFont(UIFont.ButtonFontSize);

		public UIFont SubTextFont { get; set; } = Fonts.NormalFont(12);
		public UIColor SubTextColor { get; set; } = UIColor.DarkGray;

		public UIFont MenuTextFont { get; set; } = Fonts.NormalFont(15);
		public UIColor MenuTextColor { get; set; } = UIColor.White;
		public UIColor MenuAccentColor { get; set; } = UIColor.White;

		public UIColor BackgroundColor { get; set; } = UIColor.White;
		public UIColor SectionBackgroundColor { get; set; } = UIColor.White;
		public UIBarStyle BarStyle { get; set; } = UIBarStyle.Default;
		public UIBlurEffectStyle BlurStyle { get; set; } = UIBlurEffectStyle.ExtraLight;
		public UIColor PlaybackControlTint { get; set; } = UIColor.Black;
	}

	public class DarkStyle : Style
	{
		public DarkStyle()
		{
			Id = "Dark Theme";
			this.BackgroundColor = UIColor.FromRGB(39, 40, 34);
			this.SectionBackgroundColor = UIColor.FromRGBA(84, 84, 84, 100);
			this.SubTextColor = UIColor.LightGray;
			this.MainTextColor = UIColor.White;
			this.BarStyle = UIBarStyle.BlackTranslucent;
			this.BlurStyle = UIBlurEffectStyle.Dark;
			this.PlaybackControlTint = UIColor.White;
			StatusBarColor = UIStatusBarStyle.LightContent;
		}
	}
	public static class StyleExtensions
	{
		public static Style GetStyle(this UIView view)
		{
			return view?.Window?.GetStyle() ?? Style.DefaultStyle;
		}
		public static Style GetStyle(this UIWindow window)
		{
			Style style;
			Style.Styles.TryGetValue(window?.Tag ?? 0, out style);
			return style ?? Style.DefaultStyle;
		}
		public static T StyleAsMainText<T>(this T label) where T : UILabel
		{
			var style = label.GetStyle();
			label.StyleAsMainText(style);
			return label;
		}
		public static T StyleAsMainText<T>(this T label, Style style) where T : UILabel
		{
			label.Font = style.MainTextFont;
			//Console.WriteLine($"{style.Id} {label.Font.PointSize}");
			label.TextColor = style.MainTextColor;
			return label;
		}


		public static T StyleAsSubText<T>(this T label) where T : UILabel
		{
			var style = label.GetStyle();
			label.StyleAsSubText(style);
			return label;
		}
		public static T StyleAsSubText<T>(this T label, Style style) where T : UILabel
		{
			label.Font = style.SubTextFont;
			label.TextColor = style.SubTextColor;
			return label;
		}

		public static T StylePlaybackControl<T>(this T button) where T : UIView
		{
			var style = button.GetStyle();
			button.TintColor = style.AccentColor;
			button.Layer.ShadowColor = style.AccentColor.CGColor;
			button.Layer.ShadowRadius = 10f;
			button.Layer.ShadowOpacity = .75f;
			return button;
		}


		public static T StyleNowPlayingButtons<T>(this T button) where T : UIButton
		{
			var style = button.GetStyle();
			button.TintColor = style.PlaybackControlTint;
			return button;
		}


		public static T StyleActivatedButton<T>(this T button, bool activated) where T : UIButton
		{
			var style = button.GetStyle();
			button.TintColor = activated ? style.AccentColor : style.PlaybackControlTint;
			return button;
		}

		public static T StyleAsMenuElement<T>(this T cell) where T : UITableViewCell
		{
			var style = cell.GetStyle();
			cell.TextLabel.Font = style.MenuTextFont;
			cell.TextLabel.TextColor = style.MenuTextColor;
			cell.TintColor = style.MenuAccentColor;
			if (cell.DetailTextLabel != null)
			{
				cell.DetailTextLabel.TextColor = style.MenuTextColor;
				cell.DetailTextLabel.Font = style.SubTextFont;
			}
			cell.BackgroundColor = UIColor.Clear;
			return cell;
		}
		public static T StyleAsMenuHeaderElement<T>(this T cell) where T : UITableViewCell
		{
			var style = cell.GetStyle();
			cell.TextLabel.Font = style.HeaderTextThinFont;
			cell.TextLabel.TextColor = style.MenuTextColor;
			cell.TintColor = style.MenuAccentColor;
			cell.BackgroundColor = UIColor.Clear;
			return cell;
		}

		public static T StyleSwitch<T>(this T sw) where T : UISwitch
		{
			var style = sw.GetStyle();
			var color = style.AccentColor; // UIColor.FromPatternImage(image);
										   //sw.TintColor = color;
			sw.OnTintColor = color;
			//			sw.ThumbTintColor = style.Equalizer.SwitchOnThumbColor.Value;
			sw.BackgroundColor = UIColor.DarkGray;
			sw.Layer.CornerRadius = 16;
			return sw;
		}

		public static T StyleAsBorderedButton<T>(this T button) where T : UIButton
		{
			//var style = button.GetStyle();
			var color = UIColor.White;
			button.SetTitleColor(color, UIControlState.Normal);
			button.Layer.BorderColor = color.CGColor;
			button.Layer.CornerRadius = 5;
			button.Layer.BorderWidth = .5f;


			return button;
		}

		public static T StyleAsTextButton<T> (this T button) where T : UIButton
		{
			//var style = button.GetStyle();
			var color = UIColor.White;
			button.SetTitleColor (color, UIControlState.Normal);

			return button;
		}


		public static T StyleSectionHeader<T>(this T header) where T : UITableViewHeaderFooterView
		{
			var style = header.GetStyle();
			header.TextLabel.TextColor = style.MainTextColor;
			header.BackgroundView.BackgroundColor = style.SectionBackgroundColor;
			return header;
		}

		public static T StyleViewController<T>(this T vc) where T : UIViewController
		{
			var style = vc.View.GetStyle();
			if (vc.NavigationController != null)
				vc.NavigationController.NavigationBar.BarStyle = style.BarStyle;
			vc.View.BackgroundColor = style.BackgroundColor;
			return vc;
		}

		internal static T StyleBlurView<T>(this T blurView) where T : BluredView
		{
			var style = blurView.GetStyle();
			blurView.UpdateStyle(style.BlurStyle);
			return blurView;
		}

		internal static T StyleBlurredImageView<T>(this T blurView) where T : BlurredImageView
		{
			var style = blurView.GetStyle();
			blurView.UpdateStyle(style.BlurStyle);
			return blurView;
		}

	}
}