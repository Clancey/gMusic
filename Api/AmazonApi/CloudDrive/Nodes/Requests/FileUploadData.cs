using System;
using Newtonsoft.Json;

namespace Amazon.CloudDrive
{
	public class FileUploadData
	{
		public FileUploadData(string name, NodeType type = NodeType.FILE)
		{
			Name = name;
			Kind = type;
		}

		string name;

		/// <summary>
		/// Gets or sets the name.
		/// It is limited to 256 characters by amazon
		/// </summary>
		/// <value>The name.</value>
		/// 
		[Newtonsoft.Json.JsonProperty("name")]
		public string Name
		{
			get { return name; }
			set
			{
				if (!string.IsNullOrEmpty(value) && value.Length > 256)
					throw new Exception("Invalid value, Must be less than 256 characters");
				name = value;
			}
		}

		[Newtonsoft.Json.JsonProperty("kind"), JsonConverter(typeof (Newtonsoft.Json.Converters.StringEnumConverter))]
		public NodeType Kind { get; set; }

		[Newtonsoft.Json.JsonProperty("labels", NullValueHandling = NullValueHandling.Ignore)]
		public string[] Labels { get; set; }

		[Newtonsoft.Json.JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
		public ContentNodeProperties Properties { get; set; }

		[Newtonsoft.Json.JsonProperty("parents", NullValueHandling = NullValueHandling.Ignore)]
		public string[] Parents { get; set; }
	}
}