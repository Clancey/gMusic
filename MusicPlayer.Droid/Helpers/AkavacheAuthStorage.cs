using System;
using Akavache;
using System.Reactive.Linq;

namespace MusicPlayer
{
	public class AkavacheAuthStorage : SimpleAuth.IAuthStorage
	{
		static AkavacheAuthStorage shared;
		public static AkavacheAuthStorage Shared {
			get {
				return shared ?? (shared = new AkavacheAuthStorage());
			}
			set {
				shared = value;
			}
		}
		public AkavacheAuthStorage ()
		{
		}

		#region IAuthStorage implementation

		public void SetSecured (string identifier, string value, string clientId, string clientSecret, string sharedGroup)
		{
			var key = $"{clientId}-{identifier}-{clientSecret}";
			BlobCache.LocalMachine.Insert (key, System.Text.Encoding.UTF8.GetBytes (value)).Wait ();
		
		}

		public string GetSecured (string identifier, string clientId, string clientSecret, string sharedGroup)
		{
			var key = $"{clientId}-{identifier}-{clientSecret}";
			try {
				var result = System.Text.Encoding.UTF8.GetString (BlobCache.LocalMachine.Get (key).Wait ());
				return result;
			} catch (Exception ex) {

				Console.WriteLine(ex);
			}
			return "";
		}

		#endregion
	}
}

