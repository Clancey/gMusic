//using System;
//using Android.Content;
//using Android.Preferences;
//using System.Collections.Generic;

//namespace MusicPlayer
//{
//	public static partial class Settings
//	{
//		public static float ScreenScale = 1;
//		static Dictionary<string,object> defaults = new Dictionary<string,object> {
//			{"TorrentPortNumber", 62348},
//			{"TorrentMaxUploadSpeed", 1},
//			{"RenameFiles", true},
//			{"CurrentUser",1},
//			{"LightTheme", true},
//			{"CurrentTab",8},
//			{"YouTubeEnabled",true},
//		};
//		static ISharedPreferences prefs;
//		public static void Init(Context context)
//		{
//			prefs = PreferenceManager.GetDefaultSharedPreferences(context); 
//		}

//		static void SetBool(bool value,string key)
//		{
//			var editor = prefs.Edit();
//			editor.PutBoolean(key,value);
//			editor.Apply();
//		}

//		static bool GetBool(string key)
//		{
//			var def = false;
//			object obj;
//			if (defaults.TryGetValue (key, out obj)) {
//				def = (bool)obj;
//			}
//			return prefs.GetBoolean(key,def);
//		}

//		static void SetInt(int value,string key)
//		{
//			var editor = prefs.Edit();
//			editor.PutInt(key,value);
//			editor.Apply();
//		}
//		static int GetInt(string key)
//		{
//			var def = 0;
//			object obj;
//			if (defaults.TryGetValue (key, out obj)) {
//				def = (int)obj;
//			}
//			return prefs.GetInt (key,def);
//		}
//		public static void SetString(string value,string key)
//		{
//			var editor = prefs.Edit();
//			editor.PutString(key,value);
//			editor.Apply();
//		}
//		public static string GetString(string key)
//		{
//			var def = "";
//			object obj;
//			if (defaults.TryGetValue (key, out obj)) {
//				def = (string)obj;
//			}
//			return prefs.GetString (key,def);
//		}

//		static void SetFloat(float value,string key)
//		{
//			var editor = prefs.Edit();
//			editor.PutFloat(key,value);
//			editor.Apply();
//		}
//		static float GetFloat(string key)
//		{
//			var def = 0;
//			object obj;
//			if (defaults.TryGetValue (key, out obj)) {
//				def = (int)obj;
//			}
//			return prefs.GetFloat (key,def);
//		}
//	}
//}

