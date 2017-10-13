using System;
using System.Net;
using System.Linq;
namespace MusicPlayer.Server
{
	public static class RequestExtensions
	{
		public static (long Start, long End) GetRange(this HttpListenerRequest request)
		{
			try
			{
				var rangeString = request.Headers["Range"]?.Replace("bytes=", "");
				if (string.IsNullOrWhiteSpace(rangeString))
					return (0, 0);
				var ranges = rangeString.Split('-');
				return (long.Parse(ranges[0]), long.Parse(ranges[1]));
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return (0, 0);
			}
		}

		public static bool HasRange(this HttpListenerRequest request) => request.Headers.AllKeys.Any(x => x == "Range");
	}
}
