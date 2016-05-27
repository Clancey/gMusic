using System;
using System.Collections.Generic;

namespace OneDrive
{
	public class OneDriveDeltaResponse : OneDriveResponse
	{
		public OneDriveDeltaResponse()
		{
		}

		public List<OneDriveItem> Value { get; set; }
	}
}

