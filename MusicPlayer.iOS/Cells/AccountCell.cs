using System;
using MonoTouch.Dialog;
using MusicPlayer.Api;
using MusicPlayer.Managers;
using Foundation;
using UIKit;
namespace MusicPlayer.iOS
{
	public class AccountCell : StyledStringElement, IElementSizing
	{
		public MusicProvider Provider { get; private set;}
		public AccountCell (ServiceType serviceType, Action tapped) : base ("", tapped)
		{
			this.style = UIKit.UITableViewCellStyle.Subtitle;
			ServiceType = serviceType;
			BackgroundColor = UIColor.Clear;
		}
		public AccountCell(MusicProvider provider, Action tapped) : this(provider.ServiceType, tapped)
		{
			Provider = provider;
		}

		public override UIKit.UITableViewCell GetCell(UIKit.UITableView tv)
		{
			Value = Provider?.Email ?? "";
			if (string.IsNullOrWhiteSpace(Value))
				Caption = $"Sign in to {ApiManager.Shared.DisplayName(ServiceType)}";
			else
				Caption = "Logout";

			var cell = base.GetCell(tv);
			cell.ImageView.LoadSvg(ApiManager.Shared.GetSvg(ServiceType),new NGraphics.Size(30,30));


			cell.StyleAsMenuElement();
			var style = tv.GetStyle();
			cell.TextLabel.TextColor = style.MainTextColor;
			if (cell.DetailTextLabel != null)
			{
				cell.DetailTextLabel.TextAlignment = UIKit.UITextAlignment.Center;
				cell.DetailTextLabel.TextColor = style.SubTextColor;
			}
			cell.TextLabel.TextAlignment = UIKit.UITextAlignment.Center;
			return cell;

		}
		protected override string GetKey(int style)
		{
			return "AccountCell";
		}

		public ServiceType ServiceType { get; set; }

		public nfloat GetHeight(UITableView tv, NSIndexPath path)
		{
			return 44;
		}
	}
}

