using System;
using System.Threading.Tasks;
using MusicPlayer.iOS;
using SimpleAuth;

namespace TunezApi
{
	public class TunezAccount : Account
	{
		public Uri BaseAddress {
			get { return new Uri (Url); }
		}

		public string Url { get; set;}
	}

	public class TunezApi : AuthenticatedApi
	{
		public new TunezAccount CurrentAccount {
			get { return (TunezAccount) base.CurrentAccount; }
			set { base.CurrentAccount = value; }
		}

		public TunezApi (string identifier, System.Net.Http.HttpMessageHandler handler)
			:  base (identifier, null, handler)
		{
			CurrentAccount = GetAccount<TunezAccount> (identifier);
		}

		protected override async Task<Account> PerformAuthenticate ()
		{
			string address;
			try {
				address = await PopupManager.Shared.GetTextInput ("Enter Tunez server address", "http://test.com:51986");
			} catch (OperationCanceledException) {
				CurrentAccount = null;
				return null;
			}

			CurrentAccount = new TunezAccount {
				Identifier = this.Identifier,
				Url = address,
			};
			SaveAccount (CurrentAccount);
			return CurrentAccount;
		}

		protected override Task<bool> RefreshAccount (Account account)
		{
			return Task.FromResult (true);
		}
	}
}

