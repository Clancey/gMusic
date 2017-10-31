using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.CompilerServices;
using MusicPlayer.Managers;
using System.Linq;

namespace MusicPlayer
{
	internal static class EventHandlerExtensions
	{
		public static void InvokeOnMainThread<T>(this EventHandler<T> handler, object sender, T args, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0) where T : EventArgs
		{
			App.RunOnMainThread(() =>
			{

				using (new EventLogger(memberName))
					handler.Invoke(sender, args);
			}
			);
		}

		public static void InvokeOnMainThread(this EventHandler handler, object sender, EventArgs args, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			App.RunOnMainThread(() =>
			{

				using (new EventLogger(memberName))
					handler.Invoke(sender, args);
			});
		}

		public static void InvokeOnMainThread<T>(this EventHandler<SimpleTables.EventArgs<T>> handler, object sender, T args, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			App.RunOnMainThread(() =>
			{

				using (new EventLogger(memberName))
					handler.Invoke(sender, new SimpleTables.EventArgs<T>(args));
			});
		}

		public static void InvokeOnMainThread(this EventHandler handler, object sender, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			handler.InvokeOnMainThread(sender, EventArgs.Empty, memberName, sourceFilePath,sourceLineNumber);
		}

		public static void InvokeOnMainThread(this Action action, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			App.RunOnMainThread(() =>
			{
				using (new EventLogger(memberName))
					action.Invoke();
			});
		}

		public static void InvokeOnMainThread<T>(this Action<T> action, T t, [CallerMemberName] string memberName = "",
							   [CallerFilePath] string sourceFilePath = "",
							   [CallerLineNumber] int sourceLineNumber = 0)
		{
			App.RunOnMainThread(() =>
			{
				using(new EventLogger(memberName))
					action.Invoke(t);
			});
		}

		class EventLogger : IDisposable
		{
			string name;
			static string[] IgnoredMessages = {
				"ProcCurrentTrackPositionChanged",
				"ProcConsoleChanged"
			};
			public EventLogger(string name)
			{
				this.name = name;
				if (IgnoredMessages.Contains(name))
					return;
				Console.WriteLine($"Started Event: {name}");
				//LogManager.Shared.Log($"Started Event: {name}");
			}
			public void Dispose()
			{
				if (IgnoredMessages.Contains(name))
					return;
				Console.WriteLine($"Finished Event: {name}");
					//LogManager.Shared.Log($"Finished Event: {name}");
			}
		}
	}
}