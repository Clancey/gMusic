using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using MusicPlayer.Data;
using Plugin.Connectivity;
using System.Net;

namespace MusicPlayer.Managers
{
	internal class DownloadManager : ManagerBase<DownloadManager>
	{
		readonly FixedSizeDictionary<string, DownloadHelper> downloads = new FixedSizeDictionary<string, DownloadHelper>(3);
		string currentId;
		Task downloadPollerTask;
		string fileInUses = "";

		public DownloadManager()
		{

			Directory.GetFiles(Locations.TmpDir, "*.tmp").ToList().ForEach(File.Delete);
			downloads.OnDequeue = (KeyValuePair<string, DownloadHelper> obj) =>
			{
				var helper = obj.Value;
				if (helper == null)
					return;
				var helperState = helper.State;
				var fileName = helper.FilePath;
				helper.Cancel();
				helper.Dispose();
				if(File.Exists(fileName))
					File.Delete(fileName);
				RunPoller();
			};
			CrossConnectivity.Current.ConnectivityChanged += (sender, args) =>
			{
				if (CrossConnectivity.Current.IsConnected)
					RunPoller();
			};
		}


		public void Finish(string trackId)
		{
			if (string.IsNullOrWhiteSpace(trackId))
				return;
			if (trackId == fileInUses)
				return;
			downloads.Remove(trackId);
		}

		public void RunPoller()
		{
			if (downloadPollerTask == null || downloadPollerTask.IsCompleted)
			{
				downloadPollerTask = Task.Run(async () =>
				{
					while (HasWork())
					{
						try
						{
							await Task.Delay(1000);

							DownloadHelper helper;
							if (!string.IsNullOrWhiteSpace(currentId) && downloads.TryGetValue(currentId, out helper))
							{
								if (helper.State == DownloadHelper.DownloadState.Downloading)
									continue;
								if (helper.State != DownloadHelper.DownloadState.Completed)
								{
									await helper.StartDownload();
									continue;
								}
							}
							var next = downloads.FirstOrDefault();
							if (string.IsNullOrWhiteSpace(next.Key))
								continue;
							helper = next.Value;
							if (helper.State == DownloadHelper.DownloadState.Downloading)
								continue;
							if (helper.State != DownloadHelper.DownloadState.Completed)
							{
								await helper.StartDownload();
							}
						}
						catch (Exception ex)
						{
							LogManager.Shared.Report(ex);
						}
					}
				});
			}
		}

		bool HasWork()
		{
			if (!CrossConnectivity.Current.IsConnected)
				return false;
			return downloads.Any(x => x.Value?.State != DownloadHelper.DownloadState.Completed);
		}

		public async Task QueueTrack(string trackId)
		{
			if (downloads.ContainsKey(trackId))
				return;
			var helper = downloads[trackId] = CreateHelper(trackId);
			RunPoller();

			await helper.StartDownload();
		}

		public async Task<DownloadHelper> DownloadNow(string trackId, Uri uri = null)
		{
			fileInUses = trackId;
			Finish(currentId);
			currentId = trackId;
			DownloadHelper helper;
			if (!downloads.TryGetValue(trackId, out helper))
				downloads[trackId] = helper = CreateHelper(trackId, uri);

			RunPoller();
			await helper.StartDownload();

			return helper;
		}

		DownloadHelper CreateHelper(string trackId, Uri uri = null)
		{
			var helper = new DownloadHelper
			{
				TrackId = trackId,
				Uri = uri
			};
			helper.StateChanged = (state)=>
			{
				RunPoller();
			};
			return helper;
		}
	}

	public class DownloadHelper : Stream
	{
		public enum DownloadState
		{
			Stopped,
			Paused,
			Downloading,
			Canceled,
			Error,
			Completed
		}
		public bool IsDisposed { get => Stream?.IsDisposed ?? false; }
		public string MimeType { get; set; }
		const int MaxTryCount = 5;
		readonly HttpClient client = new HttpClient();
		CancellationTokenSource cancelSource;
		Stream DownloadStream;
		Task downloadTask;
		HttpResponseMessage response;
		DownloadState state = DownloadState.Stopped;
		public int TryCount;
		QueueStream Stream { get; set; } = new QueueStream();
		public string FilePath => Stream.FilePath;
		public string TrackId { get; set; }
		public string SongId { get; set; }
		public long TotalLength => Math.Max(Stream.FinalLength, Stream.EstimatedLength);
		public long CurrentSize => Stream.CurrentSize;
		public long CurrentDownloaded => Stream.WritePosition;
		public float Percent => (float) CurrentDownloaded/TotalLength;
		public Uri Uri { get; set; }

		public DownloadState State
		{
			get { return state; }
			set
			{
				if (State == value)
					return;
				state = value;
				StateChanged?.Invoke(value);
			}
		}

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
			get { return true; }
		}

		public override long Length => TotalLength;

		public override long Position
		{
			get { return Stream.Position; }
			set { Stream.Position = value; }
		}

		public Action<DownloadState> StateChanged;

		protected override void Dispose(bool disposing)
		{
			//Kill all events
			StateChanged = null;

			if (disposing)
			{
				Stream?.Dispose();
				DownloadStream?.Dispose();
				response?.Dispose();
				client?.Dispose();
				;
			} 
			base.Dispose(disposing);
		}

		public async Task<bool> StartDownload()
		{
			try
			{
				if (State == DownloadState.Completed || State == DownloadState.Downloading)
					return true;
				State = DownloadState.Downloading;
				cancelSource = new CancellationTokenSource();
				if (string.IsNullOrWhiteSpace(SongId))
				{
					SongId = MusicManager.Shared.GetSongId(TrackId);
				}
				if (downloadTask == null || downloadTask.IsCompleted)
				{
					downloadTask = Task.Run(() => realDownload());
				}
				return true;
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
			return false;
		}

		public async Task<bool> WaitForMimeType()
		{
			var success = await Task.Run(async () =>
			{
				var shouldChec = true;
				int checkCount = 0;
				while (shouldChec)
				{
					Console.WriteLine(MimeType);
					if (string.IsNullOrWhiteSpace(MimeType))
						await Task.Delay(1000);
					Console.WriteLine(MimeType);
					shouldChec = string.IsNullOrWhiteSpace(MimeType) && checkCount < 60;
					checkCount ++;
				}
				return !string.IsNullOrWhiteSpace(MimeType);
			});
			return success;
		}

		public async Task WaitForComplete()
		{
			if (State == DownloadState.Completed || State == DownloadState.Downloading)
				return;
			await StartDownload();
			await downloadTask;
		}

		async Task realDownload()
		{
			try {
				Console.WriteLine ("Starting Download {0}", TrackId);
				var finished = false;
				while (TryCount < MaxTryCount && !finished) {
					try
					{
						State = DownloadState.Downloading;
						cancelSource.Token.ThrowIfCancellationRequested();
						var success = await OpenConnection();
						if (success)
						{
							finished = await ProccessStream();
							break;
						}
						else
						{
							TryCount++;
						}
					}
					catch (TaskCanceledException)
					{
						State = DownloadState.Canceled;
						break;
					}
					catch (WebException webEx)
					{
						Console.WriteLine("Error downloading song {0} - {1}", TrackId, webEx);
						response = null;
						DownloadStream = null;
						TryCount++;
					} catch (Exception ex) {
						Console.WriteLine ("Error downloading song {0} - {1}", TrackId, ex);
						Uri = null;
						TryCount++;
					}
					await Task.Delay (1000);
				}
				if (finished)
				{
					State = DownloadState.Completed;
					var newFilePath = TempFileManager.Shared.Add(TrackId, FilePath);
					Stream.SetNewFile(newFilePath);
				}
				else if (MaxTryCount == TryCount) {
					State = DownloadState.Error;
				}
				Console.WriteLine ("Finished Downloading: {0} {1}", State, TrackId);
			} catch (Exception ex) {
				Console.WriteLine (ex);
			}
		}

		async Task<bool> OpenConnection()
		{
			Uri url = null;
			try
			{
				Console.WriteLine($"Opening Connection {TrackId}");
				State = DownloadState.Downloading;
				cancelSource.Token.ThrowIfCancellationRequested();
				if (DownloadStream?.CanRead ?? false)
					return true;
				if (response?.IsSuccessStatusCode ?? false)
					return true;
				Console.WriteLine($"Requesting Playback Url {TrackId}");
				url = Uri ?? await MusicManager.Shared.GeTrackPlaybackUri(TrackId);
				if (url == null)
				{
					NotificationManager.Shared.ProcFailedDownload(SongId);
					return false;
				}
				else
					Uri = url;
				var request = new HttpRequestMessage(HttpMethod.Get, url);
				if (Stream.WritePosition > 0)
					request.Headers.Range = new RangeHeaderValue(Stream.WritePosition, Stream.FinalLength);

				var time = TimeSpan.FromSeconds(30);
				var cancelationSource = new CancellationTokenSource();

				var respTask = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancelationSource.Token);
				if (await Task.WhenAny(respTask, Task.Delay(time)) != respTask)
					throw new TimeoutException();
				response = respTask.Result;
				if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
				{
					NotificationManager.Shared.ProcFailedToDownloadTrack(TrackId);
					return false;
				}
				response.EnsureSuccessStatusCode();

				if (Stream.FinalLength == 0)
					Stream.FinalLength = response?.Content?.Headers?.ContentLength ?? 0;
				if(Stream.FinalLength == 0){
					IEnumerable<string> estimated = null;
					if(response?.Content?.Headers.TryGetValues("x-estimated-content-length",out estimated) == true)
					{
						Stream.EstimatedLength = long.Parse(estimated.First());
					}
				}
				if (string.IsNullOrWhiteSpace(MimeType))
				{
					MimeType = response?.Content?.Headers?.ContentType?.MediaType;
					if (string.IsNullOrEmpty(MimeType) || MimeType.Contains("octet"))
						MimeType = "audio/mpeg";
				}

				DownloadStream = await response.Content.ReadAsStreamAsync();
				return true;
			}
			catch (OperationCanceledException ex)
			{
				Console.WriteLine(ex);
				return false;
			}
			catch (Exception ex)
			{
				if (url != null)
					ex.Data["Url"] = url.AbsoluteUri;
				if (!(ex is TimeoutException))
					LogManager.Shared.Report(ex);
				else
					Console.WriteLine(ex);
			}
			return false;
		}

		async Task<bool> ProccessStream()
		{
			var buffer = new byte[4096];
			float lastUpdatePercent = 0;
			bool hasData = true;
			while (Stream.WritePosition < Stream.FinalLength || (Stream.WritePosition < Stream.EstimatedLength && hasData) || (Length == 0 && hasData))
			{
				if (cancelSource.IsCancellationRequested)
				{
					return false;
				}
				var bytesRead = await DownloadStream.ReadAsync(buffer, 0, buffer.Length);
				hasData = bytesRead > 0;
				if (!hasData)
					continue;
				Stream.Push(buffer, 0, bytesRead);
				var current = Stream.WritePosition;
				var total = Length;
				var percent = (float)current / (float)total;
				if (!(percent >= (lastUpdatePercent + .05f)) && !(percent >= .99f) || float.IsNaN(percent) || float.IsInfinity(percent)) continue;

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				Task.Run(() => NotificationManager.Shared.ProcSongDownloadPulsed(SongId, percent));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
				lastUpdatePercent = percent;
				if (Stream.FinalLength == 0)
					Stream.FinalLength = response?.Content?.Headers?.ContentLength ?? 0;
#if DEBUG
				await Task.Delay(500);
#endif
			}
			var success = Stream.FinalLength == Stream.CurrentSize && Stream.FinalLength > 0 || (Math.Abs (Stream.WritePosition - Stream.EstimatedLength) < 200000 && !hasData)  || (Length == 0 && Stream.WritePosition > 0 && !hasData);
			if (success && Stream.FinalLength == 0) {
				Stream.FinalLength = Stream.WritePosition;
				Stream.EstimatedLength = 0;
#pragma warning disable 4014
				Task.Run(() => NotificationManager.Shared.ProcSongDownloadPulsed(SongId,1f));
#pragma warning restore 4014
			}
			Console.WriteLine ($"Total length: {Stream.FinalLength}");
			
			Console.WriteLine($"Finished processing stream {TrackId} - {success}");
			return success;
		}

		public override void Flush()
		{
			Stream.Flush();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			try
			{
				return Stream.Seek(offset, origin);
			}
			catch (ObjectDisposedException ex)
			{
				Stream = new QueueStream();
				Cancel();
				StartDownload();
				return Stream.Seek(offset, origin);
			}
		}

		public override void SetLength(long value)
		{
			Stream.SetLength(value);
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return Stream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			Stream.Write(buffer, offset, count);
		}

		public void Cancel()
		{
			try
			{

				if(this.State != DownloadState.Completed)
					Console.WriteLine("Canceling Download: {0}",TrackId);
				DownloadStream?.Close();
				DownloadStream?.Dispose();
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			response?.Dispose();
			client?.Dispose();
			cancelSource.Cancel();
		}
	}
}