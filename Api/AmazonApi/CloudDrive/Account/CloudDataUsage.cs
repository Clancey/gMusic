using System;

namespace Amazon.CloudDrive
{
	public class CloudDataUsage
	{
		public CloudDataTotals Total { get; set; }
		public CloudDataTotals Billable { get; set; }
	}
}