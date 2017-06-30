using System;
using Foundation;
using Security;

namespace MusicPlayer.Api
{
	public static class Utility
	{
		private static NSUserDefaults prefs = NSUserDefaults.StandardUserDefaults;

		public static void SetSecured(string key, string value, string service)
		{
			var s = new SecRecord(SecKind.GenericPassword)
			{
				Service = $"MusicApps-{key}-{service}",
			};

			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord(s, out res);
			if (res == SecStatusCode.Success)
			{
				var remStatus = SecKeyChain.Remove(s);
			}

			s.ValueData = NSData.FromString(value);
			var err = SecKeyChain.Add(s);
		}
		public static string GetSecured(string id, string service)
		{
			var rec = new SecRecord(SecKind.GenericPassword)
			{
				Service = $"MusicApps-{id}-{service}",
			};

			SecStatusCode res;
			var match = SecKeyChain.QueryAsRecord(rec, out res);
			if (res == SecStatusCode.Success)
				return match.ValueData.ToString();
			return "";
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

