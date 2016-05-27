using System;

namespace Amazon.CloudDrive
{
	public class NodeOrderBy
	{
		public string Property { get; set; }

		public NodeOrderByDirection Direction { get; set; }

		public override string ToString()
		{
			return string.Format("\"{0} {1}\"", NodeFilter.EscapeValue(Property), Direction);
		}
	}
}