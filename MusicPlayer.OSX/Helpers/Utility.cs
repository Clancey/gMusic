using System;
using Foundation;
using Security;

namespace MusicPlayer.Api
{
	public static class Utility
	{
		private static NSUserDefaults prefs = NSUserDefaults.StandardUserDefaults;

		static public void SetSecured(string key,string value,string service)
		{
			AkavacheAuthStorage.Shared.SetSecured(key,value,"MusicApps",service,"");
		}
		static public string GetSecured(string id,string service)
		{
			return AkavacheAuthStorage.Shared.GetSecured(id,"MusicApps",service,"");
		}

		public static string DeviceName => Device.Name;

		public static string DeviceId
		{
			get {
				if (Device.IsSim)
					return "58B5F896-3C78-4A19-9BA4-8D98DB7D1149";
                
				var id = GetSecured("GoogleDeviceId", "GoogleMusic");
				if(string.IsNullOrWhiteSpace(id))
				{
					id = Guid.NewGuid().ToString();
					SetSecured("GoogleDeviceId", id, "GoogleMusic");
				}
				return id;
			}
		}
	}
}

