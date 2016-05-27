using System;
using AppKit;

namespace MusicPlayer
{
	public class Style
	{
		public static Style Current { get; } = new Style();

		public Style()
		{
		}

		public NSColor AccentColor => NSColor.FromPatternImage(NSImage.ImageNamed("accentColor"));
		public NSColor AccentColorHorizontal => NSColor.FromPatternImage(NSImage.ImageNamed("accentColorHorizontal"));

		public NSFont HeaderTextFont => Fonts.NormalFont(28);
		public NSFont HeaderTextThinFont => Fonts.ThinFont(28);
		public NSColor HeaderTextColor => AccentColor;

		public NSFont MainTextFont => Fonts.NormalFont(15);
		public NSColor MainTextColor = NSColor.LabelColor;

		public NSFont ButtonTextFont => Fonts.NormalFont(NSFont.LabelFontSize);

		public NSFont SubTextFont => Fonts.NormalFont(12);
		public NSColor SubTextColor = NSColor.SecondaryLabelColor;

		public NSFont MenuTextFont => Fonts.NormalFont(15);
		public NSColor MenuTextColor = NSColor.White;
		public NSColor MenuAccentColor => NSColor.White;
	}

	public static class StyleExtensions
	{
		public static T StyleAsMainTextCentered<T> (this T label) where T : NSTextField
		{
			
			label.StyleAsMainText();
			label.Alignment = NSTextAlignment.Center;
			return label;
		}

		public static T StyleAsHeaderText<T>(this T label) where T : NSTextField
		{
			label.StyleAsLabel ();
			label.Font = Style.Current.HeaderTextFont;
			label.TextColor = Style.Current.HeaderTextColor;
			return label;
		}
		public static T StyleAsMainText<T>(this T label) where T : NSTextField
		{
			label.StyleAsLabel ();
			label.Font = Style.Current.MainTextFont;
			return label;
		}

		public static T StyleAsLabel<T> (this T label) where T: NSTextField
		{
			label.TextColor = Style.Current.MainTextColor;
			label.Enabled = false;
			label.Bezeled = false;
			label.DrawsBackground = false;
			label.Editable = false;
			label.Selectable = false;
			return label;
		}


		public static T StyleAsSubTextCentered<T> (this T label) where T : NSTextField
		{
			label.StyleAsSubText();
			label.Alignment = NSTextAlignment.Center;
			return label;
		}
		public static T StyleAsSubText<T>(this T label) where T : NSTextField
		{
			label.Font = Style.Current.SubTextFont;
			label.TextColor = Style.Current.SubTextColor;
			label.Enabled = false;
			label.Bezeled = false;
			label.DrawsBackground = false;
			label.Editable = false;
			label.Selectable = false;
			return label;
		}

		public static T StylePlaybackControl<T>(this T button) where T : NSView
		{
			button.WantsLayer = true;
			//button.TintColor = Style.Current.AccentColor;
			button.Layer.ShadowColor = Style.Current.AccentColor.CGColor;
			button.Layer.ShadowRadius = 10f;
			button.Layer.ShadowOpacity = .75f;
			return button;
		}


//		public static T StyleAsMenuElement<T>(this T cell) where T : UITableViewCell
//		{
//			cell.TextLabel.Font = Style.Current.MenuTextFont;
//			cell.TextLabel.TextColor = Style.Current.MenuTextColor;
//			cell.TintColor = Style.Current.MenuAccentColor;
//			if (cell.DetailTextLabel != null) {
//				cell.DetailTextLabel.TextColor = Style.Current.MenuTextColor;
//				cell.DetailTextLabel.Font = Style.Current.SubTextFont;
//			}
//			cell.BackgroundColor = NSColor.Clear;
//			return cell;
//		}
//		public static T StyleAsMenuHeaderElement<T>(this T cell) where T : UITableViewCell
//		{
//			cell.TextLabel.Font = Style.Current.HeaderTextThinFont;
//			cell.TextLabel.TextColor = Style.Current.MenuTextColor;
//			cell.TintColor = Style.Current.MenuAccentColor;
//			cell.BackgroundColor = NSColor.Clear;
//			return cell;
//		}

//		public static T StyleSwitch<T>(this T sw) where T : UISwitch
//		{
//			var color = Style.Current.AccentColor;
//			sw.OnTintColor = color;
//			sw.BackgroundColor = NSColor.DarkGray;
//			sw.Layer.CornerRadius = 16;
//			return sw;
//		}
//
//		public static T StyleAsBorderedButton<T> (this T button) where T: UIButton
//		{
//			var color = NSColor.White;
//			button.SetTitleColor(color,UIControlState.Normal);
//			button.Layer.BorderColor = color.CGColor;
//			button.Layer.CornerRadius = 5;
//			button.Layer.BorderWidth = .5f;
//			
//
//			return button;
//		}


	}
}