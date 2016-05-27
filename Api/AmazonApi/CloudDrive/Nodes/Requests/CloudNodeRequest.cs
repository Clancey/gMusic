using System;
using System.Collections.Generic;
using System.Linq;

namespace Amazon.CloudDrive
{
	public class CloudNodeRequest
	{
		public string StartToken { get; set; }

		long limit;

		/// <summary>
		/// Gets or sets the limit.
		/// Current Limit is 200 as an API restriction
		/// </summary>
		/// <value>The limit.</value>
		public long Limit
		{
			get { return limit; }
			set
			{
				if (value > 200)
					throw new Exception("Limit exceed 200");
				limit = value;
			}
		}

		public bool IncludeLinks { get; set; }

		public NodeAssetMapping AssetMapping { get; set; }

		public NodeFilter Filter { get; set; }

		public List<NodeOrderBy> OrderBy { get; set; }

		public CloudNodeRequest()
		{
			OrderBy = new List<NodeOrderBy>();
		}

		public override string ToString()
		{
			var filter = Filter == null ? "" : string.Format("filters={0}", Filter.ToString());
			var orderBy = OrderBy == null || OrderBy.Count == 0 ? "" : string.Format("sort=[{0}]", string.Join(",", OrderBy));
			var start = string.IsNullOrWhiteSpace(StartToken) ? "" : string.Format("startToken={0}", StartToken);
			var limit = Limit <= 0 ? "" : string.Format("limit={0}", Limit);
			var links = IncludeLinks ? "tempLink=true" : "";
			var assest = AssetMapping == NodeAssetMapping.NONE ? "" : string.Format("assetMapping={0}", AssetMapping);

			var dataPoints = new[] {filter, orderBy, start, limit, links, assest}.Where(x => !string.IsNullOrWhiteSpace(x));
			var data = string.Join("&", dataPoints);
			return string.IsNullOrWhiteSpace(data) ? "" : string.Format("?{0}", data);
		}
	}
}