using System;
using Newtonsoft.Json;

namespace OneDrive
{
	public class OneDriveResponse
	{
		public OneDriveResponse()
		{
		}
		public OneDriveError Error { get; set; }


		[JsonProperty("@odata.nextLink")]
		public string NextLink { get; set; }

		[JsonProperty("@delta.token")]
		public string DeltaToken { get; set; }
	}
}

