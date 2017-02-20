using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MusicPlayer.Managers;

namespace MusicPlayer
{
	public class InMemoryConsole : TextWriter
	{
		public static InMemoryConsole Current { get; set; } = new InMemoryConsole();
		public FixedSizedQueue<Tuple<DateTime,string>> ConsoleOutput { get; set; } = new FixedSizedQueue<Tuple<DateTime, string>>(1000);
		public InMemoryConsole()
		{
			
		}
		public void Activate()
		{
			Console.SetOut(this);
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void WriteLine(string value)
		{
			if (!string.IsNullOrWhiteSpace(currentLine))
			{
				ConsoleOutput.Enqueue(new Tuple<DateTime, string>(DateTime.Now, currentLine));
				currentLine = "";
			}

			if (!string.IsNullOrWhiteSpace(value))
			{
				ConsoleOutput.Enqueue(new Tuple<DateTime, string>(DateTime.Now, value));
				NotificationManager.Shared.ProcConsoleChangedd();
			}
			Debug.WriteLine(value);
		}

		string currentLine = "";
		public override void Write(string value)
		{
			base.Write(value);
			currentLine += value;
		}

		public override string ToString()
		{
			return ConsoleOutput.ToString((x) => $"{x.Item1} : {x.Item2}");
		}

		public class FixedSizedQueue<T>
		{
			private readonly object privateLockObject = new object();

			readonly ConcurrentQueue<T> queue = new ConcurrentQueue<T>();

			public int Count => queue.Count;

			public T this[int i] => queue.ElementAt(i);

			public int Size { get; private set; }

			public FixedSizedQueue(int size)
			{
				Size = size;
			}

			public void Enqueue(T obj)
			{
				queue.Enqueue(obj);

				lock (privateLockObject)
				{
					while (queue.Count > Size)
					{
						T outObj;
						queue.TryDequeue(out outObj);
					}
				}
			}

			public override string ToString()
			{
				return string.Join(Environment.NewLine, queue.Reverse());
			}
			public string ToString(Func<T, string> format)
			{
				return string.Join(Environment.NewLine, queue.Select(x=> format(x)).Reverse());
			}
		}
	}

}
