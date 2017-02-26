using System;
using MusicPlayer.Api;
using MusicPlayer.Managers;

namespace MusicPlayer.Forms
{
	public class AccountCellModel
	{
		public MusicProvider Provider { get; private set; }
		public bool Name { get; set; }
		public ServiceType ServiceType { get; set; }
		public AccountCellModel()
		{
		}
		public SvgImageSource ImageSource => new SvgImageSource{SvgName = ApiManager.Shared.GetSvg(ServiceType), Size= new Xamarin.Forms.Size(30,30)};

		public string DetailText => Provider?.Email ?? "";
		public string Text => string.IsNullOrWhiteSpace(DetailText) ? $"Sign in to {ApiManager.Shared.DisplayName(ServiceType)}" : "Logout";
	}
}
