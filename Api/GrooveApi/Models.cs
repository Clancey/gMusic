using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Groove
{
	public class TrackActionResult
	{
		[JsonProperty("Error")]
		public string Error { get; set; }

		[JsonProperty("InputId")]
		public string InputId { get; set; }

		[JsonProperty("Id")]
		public string Id { get; set; }
	}

	public class TrackActionResponse
	{
		[JsonProperty("TrackActionResults")]
		public IList<TrackActionResult> TrackActionResults { get; set; }
	}

	public class TrackActionRequest
	{
		[JsonProperty("TrackIds")]
		public string[] TrackIds { get; set; }
	}

	public class StreamResponse
	{

		[JsonProperty("Url")]
		public string Url { get; set; }

		[JsonProperty("ContentType")]
		public string ContentType { get; set; }

		[JsonProperty("ExpiresOn")]
		public DateTime ExpiresOn { get; set; }
	}


}
