using System;
using System.Runtime.Serialization;

namespace MusicPlayer
{
	public static partial class EnumExtensions
	{
		public static string GetEnumMember<T>(this T enumerationValue) where T : struct, IConvertible
		{
			var type = enumerationValue.GetType();
			if (!type.IsEnum)
			{
				throw new ArgumentException("EnumerationValue must be of Enum type", "enumerationValue");
			}
			var memberInfo = type.GetMember(enumerationValue.ToString());
			if (memberInfo != null && memberInfo.Length > 0)
			{
				var attrs = memberInfo[0].GetCustomAttributes(typeof(EnumMemberAttribute), false);
				if (attrs != null && attrs.Length > 0)
				{
					return ((EnumMemberAttribute)attrs[0]).Value;
				}
			}
			return enumerationValue.ToString();
		}
	}
}
