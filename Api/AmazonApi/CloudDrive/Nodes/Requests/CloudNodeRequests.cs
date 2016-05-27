using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using MusicPlayer.Api;
using Newtonsoft.Json;
using SimpleAuth;

namespace Amazon.CloudDrive
{
	public class CloudNodeResponse : ApiResponse
	{
		public long Count { get; set; }

		public string NextToken { get; set; }

		public List<CloudNode> Data { get; set; }
	}
}