using System;

namespace MusicPlayer
{
	public static class DateTimeUtils
	{
		public static DateTime ToDateTime(this Java.Util.Date date)
		{
			var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddMilliseconds(date.Time);
		}
	}
}

