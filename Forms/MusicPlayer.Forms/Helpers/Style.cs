using System;
using System.Collections.Generic;
using System.Linq;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using Xamarin.Forms;
using MusicPlayer.Forms;

namespace MusicPlayer
{
	public enum BlurStyle
	{
		ExtraLight,
		Light,
		Dark
	}
	public class Style
	{

		public static string NormalFontName => "SFUIText-Regular";
		public static string ThinFontName => "SFUIDisplay-Thin";

		static Style()
		{
			AvailableStyles = new List<Style>
			{
				new Style(),
				new DarkStyle(),
			};

			var currentStyle = AvailableStyles.FirstOrDefault(x => x.Id == Settings.CurrentStyle) ?? AvailableStyles[0];

			//UIApplication.SharedApplication.StatusBarStyle = currentStyle.StatusBarColor;
			Styles = new Dictionary<int, Style>
			{
				{0,currentStyle},
				//{1,new CarStyle()}
			};
		}

		public static void SetStyle()
		{

			var style = AvailableStyles.FirstOrDefault(x => x.Id == Settings.CurrentStyle) ?? AvailableStyles[0];
			//UIApplication.SharedApplication.SetStatusBarStyle(style.StatusBarColor, true);
			Styles[0] = style;
		}
		public static Style DefaultStyle => Styles[0];
		public string Id { get; set; } = "Default";

		public static List<Style> AvailableStyles { get; private set; }

		public static Dictionary<int, Style> Styles { get; private set; }
		public Style()
		{

		}
		public static bool IsDeviceDark { get; set; }
		public Color AccentSolidColor { get; set; } = Color.FromRgb(255, 43, 103);
		public Color AccentColor => AccentSolidColor;//IsDeviceDark ? AccentSolidColor : AccentGradientColor;
													 //public Color AccentGradientColor { get; set; } = Color.FromPatternImage(UIImage.FromBundle("accentColor"));
													 //public Color AccentColorGradientHorizontal { get; set; } = Color.FromPatternImage(UIImage.FromBundle("accentColor").Rotate());
		public Color AccentColorHorizontal => AccentSolidColor;// IsDeviceDark ? AccentSolidColor : AccentColorGradientHorizontal;

		public float HeaderTextFontSize { get; set; } = 28;
		public string HeaderTextFont { get; set; } = NormalFontName;
		public float HeaderTextThinFontSize { get; set; } = 28;
		public string HeaderTextThinFont { get; set; } = ThinFontName;

		//public UIStatusBarStyle StatusBarColor { get; set; } = UIStatusBarStyle.Default;

		Color? headerTextColor;

		public Color HeaderTextColor
		{
			get { return (headerTextColor ?? (headerTextColor = AccentColor)).Value; }
			set { headerTextColor = value; }
		}

		public float MainTextFontSize = 15;
		public string MainTextFont { get; set; } = NormalFontName;
		public Color MainTextColor { get; set; } = Color.Black;

		public static float ButtonFontSize =
#if __IOS__
			(float)UIKit.UIFont.ButtonFontSize;
#else
			25;
#endif
		public string ButtonTextFont { get; set; } = NormalFontName;
		public static Color LightGray = Color.FromRgb(2 / 3, 2 / 3, 2 / 3);
		public static Color DarkGray = Color.FromRgb(1 / 3, 1 / 3, 1 / 3);

		public float SubTextFontSize { get; set; } = 12;
		public string SubTextFont { get; set; } = NormalFontName;
		public Color SubTextColor { get; set; } = DarkGray;

		public float MenuTextFontSize { get; set; } = 15;
		public string MenuTextFont { get; set; } = NormalFontName;
		public Color MenuTextColor { get; set; } = Color.White;
		public Color MenuAccentColor { get; set; } = Color.White;

		public Color BackgroundColor { get; set; } = Color.White;
		public Color SectionBackgroundColor { get; set; } = Color.White;
		//public UIBarStyle BarStyle { get; set; } = UIBarStyle.Default;
		public BlurStyle BlurStyle { get; set; } = BlurStyle.ExtraLight;
		public Color PlaybackControlTint { get; set; } = Color.Black;
	}

	public class DarkStyle : Style
	{
		public DarkStyle()
		{
			Id = "Dark Theme";
			this.BackgroundColor = Color.FromRgb(39, 40, 34);
			this.SectionBackgroundColor = Color.FromRgba(84, 84, 84, 100);
			this.SubTextColor = LightGray;
			this.MainTextColor = Color.White;
			//this.BarStyle = UIBarStyle.BlackTranslucent;
			this.BlurStyle = BlurStyle.Dark;
			this.PlaybackControlTint = Color.White;
			//StatusBarColor = UIStatusBarStyle.LightContent;
		}
	}
	public static class StyleExtensions
	{
		public static Style GetStyle(this View view)
		{
			Style style;
			Style.Styles.TryGetValue( 0, out style);
			return style ?? Style.DefaultStyle;
		}
		//public static Style GetStyle(this UIWindow window)
		//{
		//	Style style;
		//	Style.Styles.TryGetValue(window?.Tag ?? 0, out style);
		//	return style ?? Style.DefaultStyle;
		//}
		public static T StyleAsMainText<T>(this T label) where T : Label
		{
			var style = label.GetStyle();
			label.StyleAsMainText(style);
			return label;
		}
		public static T StyleAsMainText<T>(this T label, Style style) where T : Label
		{
			label.FontSize = style.MainTextFontSize;
			label.FontFamily = style.MainTextFont;
			label.TextColor = style.MainTextColor;
			return label;
		}


		public static T StyleAsSubText<T>(this T label) where T : Label
		{
			var style = label.GetStyle();
			label.StyleAsSubText(style);
			return label;
		}
		public static T StyleAsSubText<T>(this T label, Style style) where T : Label
		{
			label.FontSize = style.SubTextFontSize;
			label.FontFamily = style.SubTextFont;
			label.TextColor = style.SubTextColor;
			return label;
		}

		//public static T StylePlaybackControl<T>(this T button) where T : UIView
		//{
		//	var style = button.GetStyle();
		//	button.TintColor = style.AccentColor;
		//	button.Layer.ShadowColor = style.AccentColor.CGColor;
		//	button.Layer.ShadowRadius = 10f;
		//	button.Layer.ShadowOpacity = .75f;
		//	return button;
		//}


		//public static T StyleNowPlayingButtons<T>(this T button) where T : UIButton
		//{
		//	var style = button.GetStyle();
		//	button.TintColor = style.PlaybackControlTint;
		//	return button;
		//}


		//public static T StyleActivatedButton<T>(this T button, bool activated) where T : UIButton
		//{
		//	var style = button.GetStyle();
		//	button.TintColor = activated ? style.AccentColor : style.PlaybackControlTint;
		//	return button;
		//}

		//public static T StyleAsMenuElement<T>(this T cell) where T : UITableViewCell
		//{
		//	var style = cell.GetStyle();
		//	cell.TextLabel.Font = style.MenuTextFont;
		//	cell.TextLabel.TextColor = style.MenuTextColor;
		//	cell.TintColor = style.MenuAccentColor;
		//	if (cell.DetailTextLabel != null)
		//	{
		//		cell.DetailTextLabel.TextColor = style.MenuTextColor;
		//		cell.DetailTextLabel.Font = style.SubTextFont;
		//	}
		//	cell.BackgroundColor = Color.Clear;
		//	return cell;
		//}
		//public static T StyleAsMenuHeaderElement<T>(this T cell) where T : UITableViewCell
		//{
		//	var style = cell.GetStyle();
		//	cell.TextLabel.Font = style.HeaderTextThinFont;
		//	cell.TextLabel.TextColor = style.MenuTextColor;
		//	cell.TintColor = style.MenuAccentColor;
		//	cell.BackgroundColor = Color.Clear;
		//	return cell;
		//}

		//public static T StyleSwitch<T>(this T sw) where T : UISwitch
		//{
		//	var style = sw.GetStyle();
		//	var color = style.AccentColor; // Color.FromPatternImage(image);
		//								   //sw.TintColor = color;
		//	sw.OnTintColor = color;
		//	//			sw.ThumbTintColor = style.Equalizer.SwitchOnThumbColor.Value;
		//	sw.BackgroundColor = Color.DarkGray;
		//	sw.Layer.CornerRadius = 16;
		//	return sw;
		//}

		public static T StyleAsBorderedButton<T>(this T button) where T : Button
		{
			//var style = button.GetStyle();
			var color = Color.White;
			button.BorderColor = button.TextColor = color;
			button.BorderWidth = .5f;
			button.BorderRadius = 5;

			return button;
		}

		public static T StyleAsTextButton<T> (this T button) where T : Button
		{
			//var style = button.GetStyle();
			var color = Color.White;
			button.TextColor = color;

			return button;
		}


		//public static T StyleSectionHeader<T>(this T header) where T : UITableViewHeaderFooterView
		//{
		//	var style = header.GetStyle();
		//	header.TextLabel.TextColor = style.MainTextColor;
		//	header.BackgroundView.BackgroundColor = style.SectionBackgroundColor;
		//	return header;
		//}

		//public static T StyleViewController<T>(this T vc) where T : UIViewController
		//{
		//	var style = vc.View.GetStyle();
		//	if (vc.NavigationController != null)
		//		vc.NavigationController.NavigationBar.BarStyle = style.BarStyle;
		//	vc.View.BackgroundColor = style.BackgroundColor;
		//	return vc;
		//}

		//internal static T StyleBlurView<T>(this T blurView) where T : BlurView
		//{
		//	var style = blurView.GetStyle();
		//	blurView.UpdateStyle(style.BlurStyle);
		//	return blurView;
		//}

		//internal static T StyleBlurredImageView<T>(this T blurView) where T : BlurredImageView
		//{
		//	var style = blurView.GetStyle();
		//	blurView.UpdateStyle(style.BlurStyle);
		//	return blurView;
		//}

	}
}