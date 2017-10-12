using System;
using System.Runtime.CompilerServices;
namespace Plugin.Settings.Abstractions
{
	public static class AppSettingsExtensions
	{
		public static void Set(this ISettings settings, string value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, bool value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, long value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, decimal value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, int value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, float value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, Guid value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);
		public static void Set(this ISettings settings, Double value, [CallerMemberName] string memberName = "") => settings.AddOrUpdateValue(memberName, value);

		public static string GetString(this ISettings settings, string defaultValue = null, [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static bool GetBool(this ISettings settings, bool defaultValue = default(bool), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static long GetLong(this ISettings settings, long defaultValue = default(long), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static decimal GetDecimal(this ISettings settings, decimal defaultValue = default(decimal), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static int GetInt(this ISettings settings, int defaultValue = default(int), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static float GetFloat(this ISettings settings, float defaultValue = default(float), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static DateTime GetDateTime(this ISettings settings, DateTime defaultValue = default(DateTime), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static Guid GetGuid(this ISettings settings, Guid defaultValue = default(Guid), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);
		public static Double GetDouble(this ISettings settings, Double defaultValue = default(double), [CallerMemberName] string memberName = "") => settings.GetValueOrDefault(memberName, defaultValue);

	}
}