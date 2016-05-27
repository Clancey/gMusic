using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace MusicPlayer.Data
{
	internal class SlideQueue<T> : IEnumerable<T>
	{
		protected Queue<T> Queue;
		public int MaxCount { get; private set; }
		public Action<T> Removed { get; set; }

		public SlideQueue(int maxCount)
		{
			this.MaxCount = maxCount;
			Queue = new Queue<T>(maxCount);
		}

		public void Add(T item)
		{
			if (Queue.Count == MaxCount)
			{
				var oldItem = Queue.Dequeue();
				Removed?.Invoke(oldItem);
			}
			Queue.Enqueue(item);
		}

		public IEnumerator<T> GetEnumerator()
		{
			return Queue.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public int Count => Queue.Count;

		public T Dequeue()
		{
			return Queue.Count == 0 ? default(T) : Queue.Dequeue();
		}
	}
}