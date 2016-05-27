using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudDrive
{
	public class ContentExtensionFilter : NodeFilter, IEnumerable
	{
		public ContentExtensionFilter()
		{
			Extensions = new List<string>();
		}

		public List<string> Extensions { get; set; }

		public void Add(string type)
		{
			//TODO: Remove this when Amazon fixes the bug
			if (Extensions.Count > 1)
				Console.WriteLine("WARNING: Amazon does not support multiple extensions yet");
			Extensions.Add(type);
		}

		protected override string Name
		{
			get { return "contentProperties.extension"; }
		}

		protected override string Value
		{
			get
			{
				if (Extensions.Count == 1)
					return Extensions.First();
				return string.Format("({0})", string.Join(" OR ", Extensions));
			}
		}

		#region IEnumerable implementation

		public IEnumerator GetEnumerator()
		{
			return Extensions.GetEnumerator();
		}

		#endregion
	}
}