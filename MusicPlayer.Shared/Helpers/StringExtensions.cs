using System;
using System.Linq;

namespace iPadPos
{
	public static class StringExtensions
	{
		public static string UppercaseAllWords(this string s)
		{
			// Check for empty string.
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}

			return string.Join(" ", s.Split(' ').Select(x => x.UppercaseFirst()));
		}

		public static string UppercaseFirst(this string s)
		{
			// Check for empty string.
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			// Return char and concat substring.
			return char.ToUpper(s[0]) + s.Substring(1).ToLower();
		}
	}
}