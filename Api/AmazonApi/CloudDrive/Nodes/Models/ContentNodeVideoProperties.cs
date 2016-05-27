using System;

namespace Amazon.CloudDrive
{
	public class ContentNodeVideoProperties : ContentNodeImageProperties
	{
		public int Rotate { get; set; }

		public double Duration { get; set; }

		public string AudioCodec { get; set; }

		public int AudioChannels { get; set; }

		public double VideoFrameRate { get; set; }

		public double VideoBitrate { get; set; }

		public string Title { get; set; }

		public double Bitrate { get; set; }

		public double AudioSampleRate { get; set; }

		public string VideoCodec { get; set; }

		public string AudioChannelLayout { get; set; }

		public double AudioBitrate { get; set; }
	}
}