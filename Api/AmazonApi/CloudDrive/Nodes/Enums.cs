using System;

namespace Amazon.CloudDrive
{
	public enum NodeAssetMapping
	{
		NONE,
		ALL,
	}

	public enum NodeFilterSeperator
	{
		OR,
		AND,
	}

	public enum NodeOrderByDirection
	{
		DESC,
		ASC,
	}

	public enum NodeType
	{
		FILE,
		FOLDER,
		ASSET,
	}

	public enum DateTimeFilterComparison
	{
		GreaterThan,
		GreaterThanOrEqualTo,
		LessThan,
		LessThanOrEqualTo,
	}

	public enum CloudNodeStatus
	{
		AVAILABLE,
		TRASH,
	}
}