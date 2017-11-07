using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using MusicPlayer.Data;
using MusicPlayer.iOS;
using MusicPlayer.Models;
using MusicPlayer.Playback;
using SimpleTables;

namespace MusicPlayer.Managers
{
	internal class BackgroundDownloadManager : ManagerBase<BackgroundDownloadManager>
	{
		const string SharedContainerIdentifier = "group.com.iis.music";
		NSUrlSession session { get; set;}

		TaskCompletionSource<bool> initTask = new TaskCompletionSource<bool>(); 
		public Task Init()
		{
			return initTask.Task;
		}
		public BackgroundDownloadManager()
		{
			session = InitBackgroundSession(SharedContainerIdentifier);
		}

		public Dictionary<string,BackgroundDownloadFile> Files = new Dictionary<string, BackgroundDownloadFile>();

		public class CompletedArgs : EventArgs
		{
			public BackgroundDownloadFile File { get; set; }
		}

		public event EventHandler<CompletedArgs> FileCompleted;

		public Song PendingItemForRow(int row)
		{
			if (Files.Count <= row)
				return new Song();
			var item = Files.ElementAt(row).Value;
			return Database.Main.GetObject<Song, TempSong>(item.Track.SongId);
		}

		object locker = new object();

		public int Count
		{
			get { return Files.Count; }
		}

		static string MakeValidFileName(string name)
		{
			string invalidChars =
				System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
			string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

			return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
		}


		Task repairTask;

		async Task RepairBrokenDownloads()
		{
			await Task.Delay(1000);
			if (repairTask != null && !repairTask.IsCompleted)
			{
				await repairTask;
				return;
			}
			repairTask = Task.Run(() =>
			{
				try{
					var brokenFiles = Files.Values.ToList().Where(x => !x.IsActive && !x.IsCompleted).ToList();
					foreach (var pair in brokenFiles)
					{
						//TODO: check this out
						repairDownload(pair);
					}
				}
				catch(Exception ex){
					Console.WriteLine(ex);
				}
			});
		}

		async Task repairDownload(BackgroundDownloadFile file)
		{
			try
			{

				LogManager.Shared.Log("Repairing Download", file);
				var url = await MusicManager.Shared.GetDownloadUrl(file.Track);
				file.Url = url.Url;
				file.Headers = url.Headers;
				file.RetryCount += 1 ;
				Files.Remove(file.TrackId);
				Download(file);
			}
			catch (Exception ex)
			{
				ex.Data["MediaItem"] = file.TrackId;
				LogManager.Shared.Report(ex);
			}
		}

		public async Task ProcessFile(BackgroundDownloadFile file)
		{
			if (!(await NativeAudioPlayer.VerifyMp3(file.Destination, true)))
			{
				LogManager.Shared.Log("Failed Download",file);
				Download(file);
				return;
			}
			Files.Remove(file.TrackId);
			if (TasksDictionary.ContainsKey(file.Id))
				TasksDictionary.Remove(file.Id);
			var destination = Path.Combine(Locations.MusicDir, Path.GetFileName(file.Destination));
			if(File.Exists(destination))
				File.Delete(destination);
            File.Move(file.Destination, destination);

			LogManager.Shared.Log("File Proccessed", file);
			await OfflineManager.Shared.TrackDownloaded(file.TrackId);
			NotificationManager.Shared.ProcDownloaderStarted();
		}


		public async Task Download(Track track)
		{
			if(track == null)
				return;
			if (Files.ContainsKey(track.Id))
				return;
		
			LogManager.Shared.Log("Download Track", track);
			var url = await MusicManager.Shared.GetDownloadUrl(track);

			if (File.Exists(Path.Combine(Locations.MusicDir, track.FileName)))
			{
				await OfflineManager.Shared.TrackDownloaded(track.Id);
				return;
			}
			var filePath = Path.Combine(Locations.TempRelative, track.FileName);
			var file = new BackgroundDownloadFile
			{
				TrackId = track.Id,
				Url = url.Url,
				Headers = url.Headers,
				Destination = filePath,
			};
			if (File.Exists(filePath))
				await ProcessFile(file);
			else
				Download(file);
		}

		readonly Dictionary<string, NSUrlSessionTask> TasksDictionary = new Dictionary<string, NSUrlSessionTask>();

		async void Download(BackgroundDownloadFile file, int tryCount = 0)
		{
			try{
				if (Files.ContainsKey(file.TrackId))
					return;
				if (session == null || session.Configuration == null) {
					LogManager.Shared.Log("Null Session!!!!");
				}
				if(tryCount > 5)
				{
					LogManager.Shared.Report(new Exception("Cannot download track. DownloadTask is null"));					
					return;
				}
				file.SessionId = session.Configuration.Identifier;
				using (var request = new NSMutableUrlRequest(new NSUrl(file.Url)))
				{
					//request.CachePolicy = NSUrlRequestCachePolicy.ReloadIgnoringLocalAndRemoteCacheData;
					if (file.Headers != null) {
						var headers = file.Headers.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value)).ToList();
						request.Headers = NSDictionary.FromObjectsAndKeys(headers.Select(x => (NSString)x.Value).ToArray(),
							headers.Select(x => (NSString)x.Key).ToArray());
					} else {
						LogManager.Shared.Log("Null Headers");
					}
					var downloadTask = session.CreateDownloadTask(request);
					if (downloadTask == null) {
						session.InvalidateAndCancel();
						session = InitBackgroundSession(SharedContainerIdentifier);
						await Task.Delay(5000);
						Download(file,tryCount ++);
						return;
					}
					downloadTask.TaskDescription = Newtonsoft.Json.JsonConvert.SerializeObject(file);
					file.TaskId = (int) downloadTask.TaskIdentifier;
					TasksDictionary[file.Id] = downloadTask;
					downloadTask.Resume();
					Files[file.TrackId] = file;
				}
				NotificationManager.Shared.ProcDownloaderStarted();
			}
			catch(Exception ex) {
				ex.Data["Try Count"] = tryCount;
				ex.Data["BackgroundDownloadFile"] = Newtonsoft.Json.JsonConvert.SerializeObject(file);
				LogManager.Shared.Report(ex);
			}
		}

		public void Cancel(BackgroundDownloadFile file)
		{
			App.ShowNotImplmented();
		}

		public void Resume(BackgroundDownloadFile file)
		{
			App.ShowNotImplmented();
		}

		#region iOS Backgrounding code
		NSUrlSessionConfiguration configuration;
		NSUrlSession InitBackgroundSession(string identifier)
		{
			Console.WriteLine("InitBackgroundSession");
			configuration = Device.IsIos8
					? NSUrlSessionConfiguration.CreateBackgroundSessionConfiguration(identifier)
				: NSUrlSessionConfiguration.BackgroundSessionConfiguration(identifier);
			configuration.AllowsCellularAccess = true;
			var ses = NSUrlSession.FromConfiguration(configuration, new UrlSessionDelegate(), null);
			ses.GetTasks2((data, upload, downloads) => { restoreTasks(ses, data, upload, downloads); });
			return ses;

		}

		Dictionary<string, NSUrlSession> backgroundSessions = new Dictionary<string, NSUrlSession>();
		Dictionary<string, Action> backgroundSessionCompletion = new Dictionary<string, Action>();

		public async void RepairFromBackground(string sessionIdentifier, Action action)
		{
			//REset the files so they load from the disc if it was restored from a group session.
			if (!backgroundSessions.ContainsKey(sessionIdentifier))
			{
				backgroundSessions[sessionIdentifier] = InitBackgroundSession(sessionIdentifier);
				backgroundSessionCompletion[sessionIdentifier] = action;
			}
		}

		async void restoreTasks(NSUrlSession ses, NSUrlSessionTask[] sessions, NSUrlSessionTask[] uploads,
			NSUrlSessionTask[] downloads)
		{
			await Task.Run(async() =>
			{
				foreach (var d in downloads)
				{
					if(string.IsNullOrWhiteSpace(d.TaskDescription))
						continue;
					var download = Newtonsoft.Json.JsonConvert.DeserializeObject<BackgroundDownloadFile>(d.TaskDescription);
					download.IsActive = true;
					TasksDictionary[download.Id] = d;
					download.TaskId = (int) d.TaskIdentifier;
					download.IsCompleted = d.State == NSUrlSessionTaskState.Completed;
					Console.WriteLine("Downloader State: {0}", d.State);
					if (d.State == NSUrlSessionTaskState.Completed)
					{
						download.Status = BackgroundDownloadFile.FileStatus.Completed;
						download.Error = null;
						LogManager.Shared.Log("Download completed",download);
						await ProcessFile(download);
						var completed = FileCompleted;
						completed?.Invoke(download, new CompletedArgs {File = download});
					}
					else if (d.State == NSUrlSessionTaskState.Suspended)
					{
						d.Resume();
					}
					Files[download.TrackId] = download;
				}
				Console.WriteLine("Files Count: {0}",Files.Count);
				initTask.TrySetResult(true);
				NotificationManager.Shared.ProcDownloaderStarted();
			});
		}

		public void UpdateProgress(NSUrlSessionDownloadTask downloadTask, nfloat progress)
		{
			try
			{
				var file = Load(downloadTask);

				file.Percent = (float) progress;
				file.LastUpdate = DateTime.UtcNow;
				NotificationManager.Shared.ProcSongDownloadPulsed(file.Track.SongId,file.Percent);
				//Save(file);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		static List<long> CanceledItems = new List<long>();

		BackgroundDownloadFile Load(NSUrlSessionTask task, bool createIfNotExist = true)
		{
			var id = (long) task.TaskIdentifier;
			if (CanceledItems.Contains(id))
			{
				task.Cancel();
				return null;
			}
			var file = Files.Values.FirstOrDefault(x => x.TaskId == id);
			if (file != null || !createIfNotExist)
				return file;

			file = Newtonsoft.Json.JsonConvert.DeserializeObject<BackgroundDownloadFile>(task.TaskDescription);
			file.TaskId = id;
			Files[file.TrackId] = file;
			return file;
		}

		public async void Completed(NSUrlSessionTask downloadTask, NSUrl location)
		{
			NSFileManager fileManager = NSFileManager.DefaultManager;
			NSError errorCopy = null;

			var file = Load(downloadTask);
			LogManager.Shared.Log("Download Complete",file);
			file.IsCompleted = true;
			file.Status = BackgroundDownloadFile.FileStatus.Temporary;
			file.Percent = 1;
			//if (!AutoProcess)
			//{
			//	var sharedFolder = fileManager.GetContainerUrl(SharedContainerIdentifier);
			//	fileManager.CreateDirectory(sharedFolder, true, null, out errorCopy);
			//	var fileName = Path.GetFileName(file.Destination);
			//	var newTemp = Path.Combine(sharedFolder.RelativePath, fileName);

			//	var success1 = fileManager.Copy(location, NSUrl.FromFilename(newTemp), out errorCopy);
			//	Console.WriteLine("Success: {0} {1}", success1, errorCopy);
			//	file.TempLocation = newTemp;
			//	return;
			//}
			var originalURL = downloadTask.OriginalRequest.Url;
			var dest = Path.Combine(Locations.BaseDir, file.Destination);
			NSUrl destinationURL = NSUrl.FromFilename(dest);
			NSError removeCopy;

			fileManager.Remove(destinationURL, out removeCopy);
			var success = fileManager.Copy(location, destinationURL, out errorCopy);
			if (success)
				file.Status = BackgroundDownloadFile.FileStatus.Completed;
			else
				LogManager.Shared.Log("Error copying file", key: "Error", value:errorCopy?.LocalizedDescription ?? "");
			Console.WriteLine("Success: {0} {1}", success, errorCopy);
			file.Status = BackgroundDownloadFile.FileStatus.Completed;
			file.Destination = dest;
			await ProcessFile(file);
			FileCompleted?.InvokeOnMainThread(downloadTask, new CompletedArgs {File = file});
		}

		NSObject invoker = new NSObject();

		void CompletBackgroundSession(string identifier)
		{
			backgroundSessions.Remove(identifier);
			backgroundSessionCompletion.Remove(identifier);
		}

		public void Failed(NSUrlSession session, NSUrlSessionTask task, NSError error)
		{
			if (error == null)
				return;
			Console.WriteLine(error.LocalizedDescription);
			LogManager.Shared.Log("Download Failed",key:"Error",value:error.LocalizedDescription);

			nfloat progress = task.BytesReceived/(nfloat) task.BytesExpectedToReceive;
			var file = Load(task, false);
			if (file == null)
				return;
			file.RetryCount++;
			if (file.RetryCount > 5)
			{
				if(Files.ContainsKey(file.TrackId))
					Files.Remove(file.TrackId);
				return;
			}
			file.Status = BackgroundDownloadFile.FileStatus.Error;
			file.Percent = (float) progress;
			file.LastUpdate = DateTime.UtcNow;
			file.Error = error.LocalizedDescription;
			file.IsActive = false;
			RepairBrokenDownloads();
		}

		#endregion

		#region Download Delegate

		//Download DELEGATE

		public class UrlSessionDelegate : NSUrlSessionDownloadDelegate
		{
			public UrlSessionDelegate()
			{
			}

			public override void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten,
				long totalBytesWritten, long totalBytesExpectedToWrite)
			{
				try
				{
					nfloat progress = totalBytesWritten/(nfloat) totalBytesExpectedToWrite;
					Debug.WriteLine(string.Format("DownloadTask: {0}  progress: {1}", downloadTask.Handle.ToString(), progress));
					Shared.UpdateProgress(downloadTask, progress);
				}
				catch (Exception ex)
				{
					
				}
			}

			public override void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
			{
				if (downloadTask.Error == null)
					Shared.Completed(downloadTask, location);
				else
					LogManager.Shared.Log("Error Downloading",value:"Error",key:downloadTask.Error.LocalizedDescription);
			}

			public override void DidCompleteWithError(NSUrlSession session, NSUrlSessionTask task, NSError error)
			{
				Console.WriteLine("DidCompleteWithError");
				if (error != null)
				{
					LogManager.Shared.Log("Error File downloaded",key:"Error",value:error.LocalizedDescription);
					Console.WriteLine(error.LocalizedDescription);
					Shared.Failed(session, task, error);
				}
				else
				{
					Console.WriteLine("False positive");
				}
			}

			//			public override void DidReceiveChallenge (NSUrlSession session, NSUrlSessionTask task, NSUrlAuthenticationChallenge challenge, Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
			//			{
			//				Console.WriteLine ("Authentication");
			//			}
			public override void DidBecomeInvalid(NSUrlSession session, NSError error)
			{
				//Logger.LogBadRequest(new Exception(error.LocalizedDescription),session.task
			}

			public override void DidResume(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long resumeFileOffset,
				long expectedTotalBytes)
			{
				Console.WriteLine("DidResume");
				var file = Shared.Load(downloadTask);
				file.Percent = (float) resumeFileOffset/expectedTotalBytes;
				file.LastUpdate = DateTime.UtcNow;
				file.IsActive = false;
				LogManager.Shared.Log("Resumed Download",file);
			}

			public override void DidFinishEventsForBackgroundSession(NSUrlSession session)
			{
				Console.WriteLine("All tasks are finished");
				Action action;
				if (Shared.backgroundSessionCompletion.TryGetValue(session.Configuration.Identifier, out action) && action != null)
					action();
				Shared.CompletBackgroundSession(session.Configuration.Identifier);
			}

//			public override void DidReceiveChallenge(NSUrlSession session, NSUrlSessionTask task,
//				NSUrlAuthenticationChallenge challenge,
//				Action<NSUrlSessionAuthChallengeDisposition, NSUrlCredential> completionHandler)
//			{
//				Console.WriteLine("Authentication!!!!");
//				//task.CurrentRequest.Headers
//				//base.DidReceiveChallenge(session, task, challenge, completionHandler);
//			}
		}

		#endregion  //Download Delegate
	}
}