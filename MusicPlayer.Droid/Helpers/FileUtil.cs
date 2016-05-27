using System;
using Android.Content;
using Java.IO;

namespace FFMpeg
{
	public static class FFMpegUtil
	{
		const String ffmpegFileName = "ffmpeg";
		static int DEFAULT_BUFFER_SIZE = 1024 * 4;
		static int EOF = -1;

		public static bool CopyBinaryFromAssetsToData(Context context, String fileNameFromAssets, String outputFileName) {

			// create files directory under /data/data/package name
			File filesDirectory = getFilesDirectory(context);


			try {
				using (var assets = context.Assets.Open (fileNameFromAssets)) {
					using (var dest = System.IO.File.Create (System.IO.Path.Combine(filesDirectory.AbsolutePath,outputFileName))) {
						assets.CopyTo (dest);
					}
				}
				return true;

			} catch (Exception e) {
				System.Console.WriteLine (e);
				//Log.e("issue in coping binary from assets to data. ", e);
			}
			return false;
		}

		public static File getFilesDirectory(Context context) {
			// creates files directory under data/data/package name
			return context.FilesDir;
		}

		public static String getFFmpeg(Context context) {
			return getFilesDirectory(context).AbsolutePath + Java.IO.File.Separator + ffmpegFileName;
		}

		public static bool SetupFFMpeg(Context context)
		{
			var ffmpegFile = new File(getFFmpeg(context));

			if (!ffmpegFile.Exists()) {
				var isFileCopied = CopyBinaryFromAssetsToData(context,ffmpegFileName,
					ffmpegFileName);

				// make file executable
				if (isFileCopied) {
					if(!ffmpegFile.CanExecute()) {
						System.Console.Write("FFmpeg is not executable, trying to make it executable ...");
						if (ffmpegFile.SetExecutable(true)) {
							return true;
						}
					} else {
						System.Console.Write("FFmpeg is executable");
						return true;
					}
				}
			}

			return ffmpegFile.Exists() && ffmpegFile.CanExecute();
		}

//		static String getFFmpeg(Context context, Map<String,String> environmentVars) {
//			String ffmpegCommand = "";
//			if (environmentVars != null) {
//				for (Map.Entry<String, String> var : environmentVars.entrySet()) {
//					ffmpegCommand += var.getKey()+"="+var.getValue()+" ";
//				}
//			}
//			ffmpegCommand += getFFmpeg(context);
//			return ffmpegCommand;
//		}
	}
}

