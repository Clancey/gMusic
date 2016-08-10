using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.Api;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using SimpleAuth;
using Xamarin;
using Utility = MusicPlayer.Api.Utility;
using System.Runtime.CompilerServices;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using MusicPlayer.Playback;

namespace MusicPlayer.Data
{
	public static class Settings
	{
		static ISettings AppSettings { get; } = CrossSettings.Current;

		static string GetString(string defaultValue = "",[CallerMemberName] string memberName = "")
		{
			return AppSettings.GetValueOrDefault(memberName,defaultValue);
		}

		static void SetString (string value,[CallerMemberName] string memberName = "")
		{
			AppSettings.AddOrUpdateValue<string>(memberName, value);
		}
		static int GetInt(int defaultValue = 0, [CallerMemberName] string memberName = "")
		{
			return AppSettings.GetValueOrDefault(memberName, defaultValue);
		}

		static void SetInt(int value, [CallerMemberName] string memberName = "")
		{
			AppSettings.AddOrUpdateValue<int>(memberName, value);
		}

		static long GetLong(long defaultValue = 0, [CallerMemberName] string memberName = "")
		{
			return AppSettings.GetValueOrDefault(memberName, defaultValue);
		}

		static void SetLong(long value, [CallerMemberName] string memberName = "")
		{
			AppSettings.AddOrUpdateValue<long>(memberName, value);
		}

		static bool GetBool(bool defaultValue = false, [CallerMemberName] string memberName = "")
		{
			return AppSettings.GetValueOrDefault(memberName, defaultValue);
		}

		static void SetBool(bool value, [CallerMemberName] string memberName = "")
		{
			AppSettings.AddOrUpdateValue<bool>(memberName, value);
		}


		static T Get<T>(T defaultValue, [CallerMemberName] string memberName = "")
		{
			return AppSettings.GetValueOrDefault(memberName, defaultValue);
		}

		static void Set<T>(T value, [CallerMemberName] string memberName = "")
		{
			AppSettings.AddOrUpdateValue<T>(memberName, value);
		}



		const string shuffle = "Shuffle";

		public static bool ShuffleSongs
		{
			get { return AppSettings.GetValueOrDefault(shuffle, false); }
			set
			{
				AppSettings.AddOrUpdateValue(shuffle, value);
				NotificationManager.Shared.ProcShuffleChanged(value);
			}
		}

		const string repeat = "Repeat";

		public static RepeatMode RepeatMode
		{
			get { return (RepeatMode) AppSettings.GetValueOrDefault(repeat, 0); }
			set
			{
				AppSettings.AddOrUpdateValue(repeat, (int) value);
				NotificationManager.Shared.ProcRepeatChanged(value);
			}
		}

		public static bool IncludeIpod
		{
            get{ return GetBool(true); }
			set{ SetBool(value); }
		}

		public static bool IPodOnly {
			get { return GetBool (false); }
			set { SetBool (value); }
		}

		public static bool ExcludeFileSystem
		{
			get{ return GetBool(); }
			set{ SetBool(value); }
		}

		public static DateTime? LastFilesystemSync
		{
			get { 
				var x = GetLong ();
				if (x <= 0)
					return null;
				return new DateTime (x);
			}
			set{
				SetLong (value.HasValue ? value.Value.Ticks : 0);
			}
		}

		public static bool ThubsUpOnLockScreen
		{
			get{ return GetBool(true); }
			set{ SetBool(value); }
		}

		public static int CurrentMenuIndex
		{
			get { return GetInt(2); }
			set { SetInt(value); }
		}

		const string currentSongId = "CurrentSong";

		public static string CurrentSong
		{
			get { return AppSettings.GetValueOrDefault(currentSongId, ""); }
			set { AppSettings.AddOrUpdateValue(currentSongId, value); }
		}

		const string currentTrackId = "CurrentTrackId";

		public static string CurrentTrackId
		{
			get { return AppSettings.GetValueOrDefault(currentTrackId, ""); }
			set { AppSettings.AddOrUpdateValue(currentTrackId, value); }
		}

		const string currentPlaybackPercent = "CurrentPlaybackPercent";

		public static float CurrentPlaybackPercent
		{
			get {
				var val = AppSettings.GetValueOrDefault<float>(currentPlaybackPercent); 
				if (float.IsInfinity (val) || float.IsNaN (val))
					return 0;
				return val;
			}
			set { 
				if (float.IsInfinity (value) || float.IsNaN(value))
					value = 0;
				AppSettings.AddOrUpdateValue(currentPlaybackPercent, value);
			}
		}

		const string currentPlaybackContext = "CurrentPlaybackContext";

		public static PlaybackContext CurrentPlaybackContext
		{
			get
			{
				var json = AppSettings.GetValueOrDefault(currentPlaybackContext, "");
				if (string.IsNullOrWhiteSpace(json))
					return new PlaybackContext();
				return json.ToObject<PlaybackContext>();
			}
			set { AppSettings.AddOrUpdateValue(currentPlaybackContext, value.ToJson()); }
		}

		const string currentPlaybackIsVideo = "CurrentPlaybackIsVideo";
		public static bool CurrentPlaybackIsVideo
		{
			get { return AppSettings.GetValueOrDefault(currentPlaybackIsVideo, false); }
			set { AppSettings.AddOrUpdateValue(currentPlaybackIsVideo, value); }
		}

		const string lastFmEnabled = "LastFmEnabled";

		public static bool LastFmEnabled
		{
			get { return AppSettings.GetValueOrDefault(lastFmEnabled, false); }
			set { AppSettings.AddOrUpdateValue(lastFmEnabled, value); }
		}

		public static bool TwitterEnabled
		{
			get { return GetBool();}
			set { SetBool(value); }
		}

		public static string TwitterAccount
		{
			get { return GetString();}
			set { SetString(value);}
		}
		public static string TwitterDisplay
		{
			get { return GetString();}
			set { SetString(value);}
		}

		const string filterExplicit = "FilterExplicit";

		public static bool FilterExplicit
		{
			get { return AppSettings.GetValueOrDefault(filterExplicit, false); }
			set { AppSettings.AddOrUpdateValue(filterExplicit, value); }
		}

		const string mobileStreamQuality = "MobileStreamQuality";

		public static StreamQuality MobileStreamQuality
		{
			get { return (StreamQuality) AppSettings.GetValueOrDefault(mobileStreamQuality, (int)StreamQuality.Medium); }
			set { AppSettings.AddOrUpdateValue(mobileStreamQuality, (int) value); }
		}

		const string wifiStreamQuality = "WifiStreamQuality";

		public static StreamQuality WifiStreamQuality
		{
			get { return (StreamQuality)AppSettings.GetValueOrDefault(wifiStreamQuality, (int)StreamQuality.High); }
			set { AppSettings.AddOrUpdateValue(wifiStreamQuality, (int)value); }
		}

		const string downloadStreamQuality = "DownloadStreamQuality";

		public static StreamQuality DownloadStreamQuality
		{
			get { return (StreamQuality)AppSettings.GetValueOrDefault(downloadStreamQuality, (int)StreamQuality.High); }
			set { AppSettings.AddOrUpdateValue(downloadStreamQuality, (int)value); }
		}

		public static StreamQuality VideoStreamQuality
		{
			get { return (StreamQuality)GetInt((int)StreamQuality.High); }
			set { SetInt((int)value); }
		}

		const string equalizerPreset = "EqualizerPreset";

		public static int EqualizerPreset
		{
			get { return AppSettings.GetValueOrDefault(equalizerPreset, 0); }
			set { AppSettings.AddOrUpdateValue(equalizerPreset, value); }
		}

		const string equalizerEnabled = "EqualizerEnabled";

		public static bool EqualizerEnabled
		{
			get { return AppSettings.GetValueOrDefault(equalizerEnabled, true); }
			set { 
				if (AppSettings.AddOrUpdateValue (equalizerEnabled, value))
					NotificationManager.Shared.ProcEqualizerEnabledChanged ();
			}
		}

		const string showOfflineOnly = "ShowOfflineOnly";

		public static bool ShowOfflineOnly
		{
			get { return AppSettings.GetValueOrDefault(showOfflineOnly, false); }
			set
			{
				AppSettings.AddOrUpdateValue(showOfflineOnly, value);
				NotificationManager.Shared.ProcOfflineChanged();
			}
		}

		const string preferVideos = "PreferVideos";

		public static bool PreferVideos
		{
			get { return AppSettings.GetValueOrDefault(preferVideos, false); }
			set { AppSettings.AddOrUpdateValue(preferVideos, value); }
		}
		static Dictionary<int, ApiModel> currentApiModels;

		public static ApiModel[] CurrentApiModels
		{
			get
			{
				LoadModels();
				lock(currentApiModels)
				{
					return currentApiModels.Values.ToArray();
				}
			}
		}

		const string currentApiModelString = "CurrentApiModels";

		static void LoadModels()
		{
			if (currentApiModels != null)
				return;
			var data = Utility.GetSecured(currentApiModelString, "mediaPlayer");
			try
			{
				currentApiModels = string.IsNullOrWhiteSpace(data)
					? new Dictionary<int, ApiModel>()
					: data.ToObject<Dictionary<int, ApiModel>>();
				if (!IsFirstRun) return;
				lock(currentApiModels)
				{
					currentApiModels?.ForEach(x => x.Value.ExtraData = "");
				}
				SaveCurrentApiModels();
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
				currentApiModels = new Dictionary<int, ApiModel>();
			}
		}

		static bool IsFirstRun
		{
			get
			{
				var firstRun = AppSettings.GetValueOrDefault("isFirstRun", true);

				AppSettings.AddOrUpdateValue("isFirstRun", false);
				return firstRun;
			}
		}

		public static void DeleteApiModel(ApiModel model)
		{
			LoadModels();
			lock(currentApiModels)
			{
				if (!currentApiModels.ContainsKey(model.Id))
					return;
				currentApiModels.Remove(model.Id);
			}
			SaveCurrentApiModels();
		}

		public static void AddApiModel(ApiModel model)
		{
			LoadModels();
			lock(currentApiModels)
			{
				currentApiModels[model.Id] = model;
			}
			SaveCurrentApiModels();
		}

		public static int GetNextApiId()
		{
			LoadModels();
			lock(currentApiModels)
			{
				return currentApiModels.Count == 0 ? 1 : currentApiModels.Values.Max(x => x.Id) + 1;
			}
		}

		static void SaveCurrentApiModels()
		{
			lock(currentApiModels)
			{
				Utility.SetSecured(currentApiModelString, currentApiModels.ToJson(), "mediaPlayer");
			}
		}

		public static void ResetApiModes ()
		{
			LoadModels ();
			lock(currentApiModels)
			{
				currentApiModels.ForEach(x => x.Value.ExtraData = "");
			}
			SaveCurrentApiModels ();
		}

		public static UserDetails CurrentUserDetails
		{
			get { 
				var val = GetString();
				return string.IsNullOrWhiteSpace (val) ? null : val.ToObject<UserDetails> ();
			}
			set { SetString(value.ToJson()); }
		}

		public static bool EnableGaplessPlayback
		{
			get { return GetBool(true);}
			set { SetBool(value); }
		}

		public static float CurrentVolume
		{
			get { return Get<float>(1); }
			set { 
				Set<float>(value);
				NotificationManager.Shared.ProcVolumeChanged();
			}
		}

		public static bool DisableAllAccess
		{
			get{ return GetBool (); }
			set { SetBool(value); }
		}

		public static string CurrentStyle
		{
			get{ return GetString (defaultValue:"Default"); }
			set { 
				SetString(value);
				NotificationManager.Shared.ProcStyleChanged();
			}
		}
	}
}