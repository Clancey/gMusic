using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace Amazon.CloudDrive
{
	public class NodeFilterGroup : NodeFilter, IEnumerable
	{
		public readonly List<Tuple<NodeFilter, NodeFilterSeperator>> Filters =
			new List<Tuple<NodeFilter, NodeFilterSeperator>>();

		public void Add(NodeFilter filter, NodeFilterSeperator seperator = NodeFilterSeperator.AND)
		{
			Filters.Add(new Tuple<NodeFilter, NodeFilterSeperator>(filter, seperator));
		}

		protected override string Name
		{
			get { return ""; }
		}

		protected override string Value
		{
			get { return ""; }
		}

		public override string ToString()
		{
			int current = 0;
			int last = Filters.Count;
			if (last == 0)
				return "";
			var data = string.Join(" ", Filters.Select(x =>
			{
				current++;
				//Dont add the seperator to the last one...
				return current == last ? x.Item1.ToString() : string.Format("{0} {1}", x.Item1, x.Item2);
			}));
			return data; //string.Format ("({0})", data);
		}

		#region IEnumerable implementation

		public IEnumerator GetEnumerator()
		{
			return Filters.GetEnumerator();
		}

		#endregion
	}
}