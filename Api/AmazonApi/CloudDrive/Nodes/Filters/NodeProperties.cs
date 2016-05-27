using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleAuth;

namespace Amazon.CloudDrive
{
	//[JsonConverter(typeof(NodePropertiesConverter))]
	public class NodeProperties
	{
		public Dictionary<string, Dictionary<string, string>> Properties { get; set; }
		//public string OwnerAppId { get; set; }

		//public Dictionary<string, string> Properties { get; set; }
	}

	//public class NodePropertiesConverter : JsonConverter
	//{
	//	public override bool CanConvert(Type objectType)
	//	{
	//		return objectType == typeof(NodeProperties);
	//	}
	//	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	//	{
	//		var test = serializer.Deserialize<JObject>(reader);

	//		foreach (var prop in test.Properties())
	//		{
	//			//should only have one prop
	//			var node = new NodeProperties
	//			{
	//				OwnerAppId = prop.Name,
	//				Properties = prop.Value.ToObject<Dictionary<string, string>>(),
	//			};
	//			return node;

	//		}
	//		return null;
	//	}

	//	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	//	{
	//		//var node = value as NodeProperties;
	//		//var val = new KeyValuePair<string, Dictionary<string, string>>(node.OwnerAppId, node.Properties);

	//		//var jobj = new JObject();
	//		//jobj.Add(node.OwnerAppId, JToken.FromObject(node.Properties));
	//		//jobj.WriteTo(writer);
	//	}
	//}
}