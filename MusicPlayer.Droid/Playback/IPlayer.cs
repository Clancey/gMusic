using Android.Support.V4.Media.Session;

namespace MusicPlayer.Droid.Playback
{
	internal interface IPlayer
	{
		int State { get; set;}
		bool IsConnected { get; }
		bool IsPlaying { get; }
		int CurrentStreamPosition { get; set; }
		string CurrentMediaId { get; set; }
		void Start();
		void Stop(bool notifyListeners);
		void UpdateLastKnownStreamPosition();
		void Pause();
		void Seek(int position);
		void SetCallback(IPlayerCallBack callBack);
	}

	internal interface IPlayerCallBack
	{
		void Completed();
		void PlaybackStatusChanged(int state);
		void OnError(string error);
		void SetCurrentMediaId(string id);
	}
}