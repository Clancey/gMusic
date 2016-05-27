using System;

namespace Amazon.CloudDrive
{
	public class DateTimeBetweenNodeFilter : NodeFilter
	{
		public string Field { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		#region implemented abstract members of NodeFilter

		protected override string Name
		{
			get { return Field; }
		}


		protected override string Value
		{
			get { return string.Format("[{0} TO {1}]", StartDate.ToString("O"), EndDate.ToString("O")); }
		}

		#endregion
	}
}