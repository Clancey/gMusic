using System;
namespace MusicPlayer
{
	internal static partial class ApiConstants
	{
		internal const string InsightsApiKey = "d83993859d287c6a57666d86a482272932a78a65";
#if APPSTORE
		internal const string MobileCenterApiKey = "82394013-5db8-4cd7-920d-ccefe5ea6185";
#else
		internal const string MobileCenterApiKey = "e1552973-f10e-43a7-8529-fa1cf8082bd8";
#endif
	}
}

