using System;
using System.Collections.Generic;
using Newtonsoft.Json;
namespace SoundCloud
{
	public class SApiResponse<T>
	{
		[JsonPropertyAttribute("collection")]
		public List<T> Items { get; set; }

		[JsonProperty("next_href")]	
		public string NextUrl { get; set; }
	}
}

