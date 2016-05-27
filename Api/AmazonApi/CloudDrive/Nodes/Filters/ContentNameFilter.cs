using System;

namespace Amazon.CloudDrive
{
	public class ContentNameFilter : NodeFilter
	{
		public string FileName { get; set; }

		#region implemented abstract members of NodeFilter

		protected override string Name
		{
			get { return "name"; }
		}

		protected override string Value
		{
			get { return EscapeValue(FileName); }
		}

		#endregion
	}
}