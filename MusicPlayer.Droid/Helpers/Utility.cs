using System;

namespace MusicPlayer.Api
{
	public static class Utility
	{

		static public void SetSecured(string key, string value, string service)
		{
			MusicPlayer.AkavacheAuthStorage.Shared.SetSecured(key, value, "MusicApps", service, "");
		}
		static public string GetSecured(string id, string service)
		{
			return AkavacheAuthStorage.Shared.GetSecured(id, "MusicApps", service, "");
		}

		public static string DeviceName => Android.OS.Build.Model;

		public static string DeviceId
		{
			get
			{

				var id = GetSecured("GoogleDeviceId", "GoogleMusic");
				if (string.IsNullOrWhiteSpace(id))
				{
					id = Guid.NewGuid().ToString();
					SetSecured("GoogleDeviceId", id, "GoogleMusic");
				}
				return id;
			}
		}
	}
}

