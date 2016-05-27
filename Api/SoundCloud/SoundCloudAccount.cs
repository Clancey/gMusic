using System;
using SimpleAuth;
namespace SoundCloud
{
	public class SoundCloudAccount : OAuthAccount
	{
		public SoundCloudAccount()
		{
		}
		public override bool IsValid()
		{
			return !string.IsNullOrWhiteSpace(Token);
		}
	}
}

