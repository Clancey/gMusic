using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Amazon.CloudDrive
{
	public class CloudNode
	{
		[JsonProperty("Parents", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Parents { get; set; }

		[JsonProperty("Kind", NullValueHandling = NullValueHandling.Ignore)]
		public NodeType? Kind { get; set; }

		[JsonProperty("Version", NullValueHandling = NullValueHandling.Ignore)]
		public int? Version { get; set; }

		[JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore)]
		public string Id { get; set; }

		[JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
		public string Name { get; set; }

		[JsonProperty("CreatedDate", NullValueHandling = NullValueHandling.Ignore)]
		public string CreatedDate { get; set; }

		[JsonProperty("eTagResponse", NullValueHandling = NullValueHandling.Ignore)]
		public string eTagResponse { get; set; }

		[JsonProperty("Status", NullValueHandling = NullValueHandling.Ignore)]
		public CloudNodeStatus? Status { get; set; }

		[JsonProperty("Labels", NullValueHandling = NullValueHandling.Ignore)]
		public List<string> Labels { get; set; }

		[JsonProperty("Restricted", NullValueHandling = NullValueHandling.Ignore)]
		public bool? Restricted { get; set; }

		[JsonProperty("ModifiedDate", NullValueHandling = NullValueHandling.Ignore)]
		public string ModifiedDate { get; set; }

		[JsonProperty("CreatedBy", NullValueHandling = NullValueHandling.Ignore)]
		public string CreatedBy { get; set; }

		[JsonProperty("IsShared", NullValueHandling = NullValueHandling.Ignore)]
		public bool? IsShared { get; set; }

		[JsonProperty("TempLink", NullValueHandling = NullValueHandling.Ignore)]
		public string TempLink { get; set; }

		[JsonProperty("ContentProperties", NullValueHandling = NullValueHandling.Ignore)]
		public ContentNodeProperties ContentProperties { get; set; }

		[JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
		public Dictionary<string, Dictionary<string, string>> Properties { get; set; }

		[JsonProperty("Transforms", NullValueHandling = NullValueHandling.Ignore)]
		//TODO: Unknown data type
		public List<object> Transforms { get; set; }

		[JsonProperty("ParentMap", NullValueHandling = NullValueHandling.Ignore)]
		public ParentMap ParentMap { get; set; }
	}
}