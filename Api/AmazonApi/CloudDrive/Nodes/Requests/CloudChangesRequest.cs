using System;
using Newtonsoft.Json;

namespace Amazon.CloudDrive
{
	public class CloudChangesRequest
	{
		[JsonProperty("checkpoint", NullValueHandling = NullValueHandling.Ignore)]
		public string Checkpoint { get; set; }

		[JsonProperty("chunkSize", NullValueHandling = NullValueHandling.Ignore)]
		public int? ChunkSize { get; set; }

		[JsonProperty("maxNodes", NullValueHandling = NullValueHandling.Ignore)]
		public int? MaxNodes { get; set; }

		[JsonProperty("includePurged", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IncludePurged { get; set; }

		public bool IsEmpty()
		{
			return string.IsNullOrEmpty(Checkpoint) &&
					ChunkSize == null &&
					MaxNodes == null &&
					IncludePurged == null;
		}
	}
}