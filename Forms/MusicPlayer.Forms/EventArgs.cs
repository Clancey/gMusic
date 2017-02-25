using System;

namespace MusicPlayer
{
	public class EventArgs<T> : EventArgs
	{
		// Property variable
		private readonly T p_EventData;
		
		// Constructor
		public EventArgs(T data)
		{
			p_EventData = data;
		}
		
		// Property for EventArgs argument
		public T Data
		{
			get { return p_EventData; }
		}
	}

}

