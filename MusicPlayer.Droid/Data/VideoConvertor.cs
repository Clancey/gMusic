//using System;
//using System.Threading.Tasks;
//using Android.Media;
//using Java.Nio;
//using FFMpeg;
//using System.IO;


//namespace MusicPlayer
//{
//	public static partial class VideoConvertor
//	{
//		public static async Task<bool> StripVideo (Song song,bool keepvideo, string newFilePath = "")
//		{
//			Xamarin.Insights.Track("Convert to audio");
//			FFMpegUtil.SetupFFMpeg (App.Context);

//			var tempFile = song.TempFile;
//			if (File.Exists (tempFile))
//				File.Delete (tempFile);
//			var cmd = new []{
//				FFMpegUtil.getFFmpeg (App.Context), 
//				"-i",
//				song.File,
//				"-acodec",
//				"copy",
//				"-vn",
//				tempFile
//			};
//			var success = new ShellCommand().RunWaitFor(cmd);
//			Console.WriteLine (success.Output);
//			try {
//				if (success.Success) {
//					if(keepvideo)
//					{
//						var newSong = new Song(tempFile){
//							Title = song.Title,
//							Artist = song.Artist,
//							Album = song.Album,
//							AlbumArtist = song.AlbumArtist,
//							AlbumArtistId = song.AlbumArtistId,
//							AlbumArtUrl = song.AlbumArtUrl,
//							AlbumId = song.AlbumId,
//							ArtistId = song.ArtistId,
//							BeatsPerMinute = song.BeatsPerMinute,
//							BitRate = song.BitRate,
//							Disc = song.Disc,
//							Duration = song.Duration,
//							Genre = song.Genre,
//							GenreId = song.GenreId,
//							IndexCharacter = song.IndexCharacter,
//							IsLocal = true,
//							IsMovie = false,
//							PlayCount = song.PlayCount,
//							TitleNorm = song.TitleNorm,
//							TotalDiscs = song.TotalDiscs,
//							Track = song.Track,
//							Rating = song.Rating,
//							Year = song.Year,
//						};
//						Database.Main.AddSong(newSong,true,tempFile,true);
//					}
//					else{
//						song.IsMovie = false;
//						File.Delete(song.File);
//						Database.Main.AddSong(song,true,tempFile,true);
//					}
//					return true;
//				} else {
//					Console.WriteLine (success.Output);
//					return false;
//				}
//			} catch (Exception ex) {
//				Console.WriteLine (ex);
//			}

//			return false;
//		}
//	}
//}

