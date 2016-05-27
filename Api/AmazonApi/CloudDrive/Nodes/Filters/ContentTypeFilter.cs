using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudDrive
{
	public class ContentTypeFilter : NodeFilter, IEnumerable
	{
		public const string Images = "image*";
		public const string Videos = "video*";

		public static string[] KnownTypes =
		{
			Images,
			Videos,
		};

		public ContentTypeFilter()
		{
			Types = new List<string>();
		}

		public List<string> Types { get; set; }

		public void Add(string type)
		{
			Types.Add(type);
		}

		protected override string Name
		{
			get { return "contentProperties.contentType"; }
		}

		protected override string Seperator
		{
			get { return ":"; }
		}

		protected override string Value
		{
			get
			{
				if (Types.Count == 1)
					return Types.First();
				return string.Format("({0})", string.Join(" OR ", Types));
			}
		}

		#region IEnumerable implementation

		public IEnumerator GetEnumerator()
		{
			return Types.GetEnumerator();
		}

		#endregion
	}
}