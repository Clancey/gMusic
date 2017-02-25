using System;
using System.Collections.Generic;
using System.Text;
#if !FORMS
using SimpleTables;
#endif

namespace MusicPlayer
{
	internal static class EventHandlerExtensions
	{
		public static void InvokeOnMainThread<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
		{
			App.RunOnMainThread(() => handler.Invoke(sender, args));
		}

		public static void InvokeOnMainThread(this EventHandler handler, object sender, EventArgs args)
		{
			App.RunOnMainThread(() => handler.Invoke(sender, args));
		}

		public static void InvokeOnMainThread<T>(this EventHandler<EventArgs<T>> handler, object sender, T args)
		{
			App.RunOnMainThread(() => handler.Invoke(sender, new EventArgs<T>(args)));
		}

		public static void InvokeOnMainThread(this EventHandler handler, object sender)
		{
			handler.InvokeOnMainThread(sender, EventArgs.Empty);
		}

		public static void InvokeOnMainThread(this Action action)
		{
			App.RunOnMainThread(action.Invoke);
		}

		public static void InvokeOnMainThread<T>(this Action<T> action, T t)
		{
			App.RunOnMainThread(() => action.Invoke(t));
		}
	}
}