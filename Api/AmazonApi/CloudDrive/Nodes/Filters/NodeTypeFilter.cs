using System;

namespace Amazon.CloudDrive
{
	public class NodeTypeFilter : NodeFilter
	{
		#region implemented abstract members of NodeFilter

		public NodeType Type { get; set; }

		protected override string Name
		{
			get { return "kind"; }
		}

		protected override string Value
		{
			get { return Type.ToString(); }
		}

		#endregion
	}
}