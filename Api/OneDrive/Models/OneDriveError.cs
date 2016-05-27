using System;
namespace OneDrive
{
	public class OneDriveError
	{
		public string Code { get; set; }
		public string Message { get; set; }
		public OneDriveError InnerError { get; set; }
	}
}

