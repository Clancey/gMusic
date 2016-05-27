using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace MusicPlayer.Data
{
	public class QueueStream : Stream
	{
		Stream writeStream;
		Stream readStream;
		long size;
		bool done;
		object plock = new object();

		public string FilePath { get; private set; }

		public QueueStream() : this(Path.GetTempFileName())
		{
		}

		public QueueStream(string file)
		{
			FilePath = file;
			writeStream = new FileStream(file, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096);
			readStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096);
		}

		public long WritePosition => size;

		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return true; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}

		public override long Length
		{
			get { return FinalLength > 0 ? FinalLength : readStream.Length; }
		}

		public long FinalLength { get; set; }

		public long EstimatedLength { get; set; }

		public override long Position
		{
			get { return readStream.Position; }
			set { readStream.Position = value; }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			lock (plock)
			{
				while (true)
				{
					if (Position < size)
					{
						int n = readStream.Read(buffer, offset, count);
						return n;
					}
					else if (done)
						return 0;

					try
					{
						Debug.WriteLine("Queue Stream: Waiting for data");
						Monitor.Wait(plock);
						Debug.WriteLine("Queue Stream: Waking up, data available");
					}
					catch
					{
					}
				}
			}
		}

		public long CurrentSize => size;

		public void Push(byte[] buffer, int offset, int count)
		{
			lock (plock)
			{
				writeStream.Write(buffer, offset, count);
				size += count;
				writeStream.Flush();
				Monitor.Pulse(plock);
			}
		}

		public void Done()
		{
			lock (plock)
			{
				Monitor.Pulse(plock);
				done = true;
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				readStream.Close();
				readStream.Dispose();
				writeStream.Close();
				writeStream.Dispose();
			}
			base.Dispose(disposing);
		}

		#region non implemented abstract members of Stream

		public override void Flush()
		{
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			lock (plock)
			{
				long newOffset = 0;
				switch (origin)
				{
					case SeekOrigin.Begin:
						newOffset = offset;
						break;
					case SeekOrigin.Current:
						newOffset = Position + offset;
						break;
					case SeekOrigin.End:
						newOffset = Length - offset;
						break;
				}
				if (newOffset == Position)
					return newOffset;

				while (true)
				{
					if (newOffset < readStream.Length)
					{
						var n = readStream.Seek(newOffset, SeekOrigin.Begin);
						return n;
					}
					else if (done)
						return 0;

					try
					{
						Debug.WriteLine("Queue Stream: SEEK Waiting for data");
						Monitor.Wait(plock);
						Debug.WriteLine("Queue Stream: SEEK Waking up, data available");
					}
					catch
					{
					}
				}
			}
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}