using System;
using System.Collections.Generic;
namespace OneDrive
{
	public class OneDriveIdentitySet
	{
		public OneDriveIdentity Application { get; set; }

		public OneDriveIdentity Device { get; set; }

		public OneDriveIdentity User { get; set; }


		public Dictionary<string, object> AdditionalData { get; set; }
	}
}

