using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace System
{
	public static class ArrayExtensions
	{
		public static int IndexOf(this Array array, object item)
		{
			return Array.IndexOf(array, item);
		}

		public static void ForEach<T>(this T[] array, Action<T> action)
		{
			foreach (var a in array)
			{
				action(a);
			}
		}

		public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
		{
			foreach (T item in enumeration)
			{
				action(item);
			}
		}
	}
}