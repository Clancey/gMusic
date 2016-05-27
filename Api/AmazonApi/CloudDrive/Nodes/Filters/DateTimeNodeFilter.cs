using System;

namespace Amazon.CloudDrive
{
	public class DateTimeNodeFilter : NodeFilter
	{
		public DateTimeFilterComparison Comparison { get; set; }

		public string Field { get; set; }

		public DateTime Date { get; set; }

		#region implemented abstract members of NodeFilter

		protected override string Name
		{
			get { return Field; }
		}

		protected override string Value
		{
			get
			{
				switch (Comparison)
				{
					case DateTimeFilterComparison.GreaterThan:
					case DateTimeFilterComparison.GreaterThanOrEqualTo:
						return string.Format("[* {0}]", Date.ToString("O"));
					case DateTimeFilterComparison.LessThan:
					case DateTimeFilterComparison.LessThanOrEqualTo:
						return string.Format("[{0}*]", Date.ToString("O"));
				}
				;
				return "";
			}
		}

		#endregion
	}
}