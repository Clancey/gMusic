using System;
using System.Collections.Generic;
using MusicPlayer.Api;
using SimpleAuth;

namespace Amazon.CloudDrive
{
	public class CloudQuotaResponse : ApiResponse
	{
		public List<string> Plans { get; set; }
		public long Quota { get; set; }
		public string LastCalculated { get; set; }
		//		public List<object> grants { get; set; }
		//		public List<Benefit> Benefits { get; set; }
		public long Available { get; set; }
	}
}