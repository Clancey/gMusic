using System;

namespace Amazon.CloudDrive
{
	public class ContentNodeProperties
	{
		public string ContentDate { get; set; }

		public string Md5 { get; set; }

		public string ContentType { get; set; }

		public ContentNodeVideoProperties Video { get; set; }

		public ContentNodeImageProperties image { get; set; }

		public int Version { get; set; }

		public int Size { get; set; }

		public string Extension { get; set; }
	}
}