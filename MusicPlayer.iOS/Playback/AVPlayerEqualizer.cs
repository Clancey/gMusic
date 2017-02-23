using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using AudioToolbox;
using AudioUnit;
using AVFoundation;
using CoreMedia;
using Foundation;
using MediaToolbox;
using MusicPlayer.Data;
using MusicPlayer.Managers;
using MusicPlayer.Playback;
using ObjCRuntime;

namespace MusicPlayer.iOS.Playback
{
	internal class AVPlayerEqualizer : ManagerBase<AVPlayerEqualizer>, IEqualizer
	{
		#region IEqualizer implementation

		protected Equalizer.Band[] Bands = new Equalizer.Band[0];

		AudioTapProcessor processor;


		public async System.Threading.Tasks.Task ApplyEqualizer(Equalizer.Band[] bands)
		{
			await ApplyEqualizer(bands, PlaybackManager.Shared.NativePlayer.CurrentItem);
		}

		public async System.Threading.Tasks.Task ApplyEqualizer(Equalizer.Band[] bands, AVPlayerItem item)
		{
			Bands = bands ?? new Equalizer.Band[0];
			if (item == null)
				return;
			if (item.Tracks.Length == 0)
				await item.WaitLoadTracks();
			var audio = item.Tracks.FirstOrDefault(x => x.AssetTrack.HasMediaCharacteristic(AVMediaCharacteristic.Audible));
			if (audio == null)
				return;
			if (processor == null || processor?.audioAssetTrack != audio.AssetTrack)
			{
				processor?.Dispose ();
				processor = new AudioTapProcessor (audio.AssetTrack) {
					Parent = this,
					IsBandpassFilterEnabled = Active,
				};
			}

			processor.IsBandpassFilterEnabled = StateManager.Shared.EqualizerEnabled;
			item.AudioMix = processor.AudioMix;
			for (int i = 0; i < Bands.Count(); i++)
			{
				UpdateBand(i, Bands[i].Gain);
			}
		}

		public Task ApplyEqualizer()
		{
			return ApplyEqualizer(Equalizer.Shared.Bands);
		}

		public async Task ApplyEqualizer(AVPlayerItem item)
		{
#if iPod
			var curEq = await Equalizer.GetPreset(AudioPlayer.Shared.CurrentSong) ?? await Equalizer.GetDefault(AudioPlayer.Shared.CurrentSong) ?? Equalizer.Shared.CurrentPreset; 
			Equalizer.Shared.ApplyPreset(curEq);
			Equalizer.Shared.CurEqId = curEq.GlobalId;
#endif
			await ApplyEqualizer(Equalizer.Shared.Bands, item);
		}

		public void UpdateBand(int band, float gain)
		{
			if (processor == null)
			{
				return;
			}
			processor.SetBand(band, gain);
		}

		public void Clear()
		{
			if (processor == null)
				return;
			for (int i = 0; i < Bands.Count(); i++)
			{
				UpdateBand(i, 0);
			}
		}

		public bool Active
		{
			get { return Settings.EqualizerEnabled; }
			set
			{
				Settings.EqualizerEnabled = value;
				#if __IOS__ || __OSX__
				if (processor != null)
					processor.IsBandpassFilterEnabled = value;

				#endif
			}
		}

		#endregion

		struct AVAudioTapProcessorContext
		{
			public bool SupportedTapProcessingFormat;

			public bool IsNonInterleaved;

			public double SampleRate;

			public double SampleCount;

			public float LeftChannelVolume;

			public float RightChannelVolume;

			public AudioUnit.AudioUnit AudioUnit;
		}
		public class AudioTapProcessor : NSObject
		{
			public readonly AVAssetTrack audioAssetTrack;
			public AVPlayerEqualizer Parent { get; set; }
			MTAudioProcessingTap audioProcessingTap;
			AVAudioTapProcessorContext context;

			public bool IsBandpassFilterEnabled { get; set; }


			AVAudioMix audioMix;

			public AVAudioMix AudioMix
			{
				get { return audioMix = audioMix ?? CreateAudioMix(); }
			}

			public AudioTapProcessor(AVAssetTrack audioTrack)
			{
				if (audioTrack == null)
					throw new ArgumentNullException("audioTrack");

				if (audioTrack.MediaType != AVMediaType.Audio)
					throw new ArithmeticException("MediaType is not AVMediaType.Audio");

				audioAssetTrack = audioTrack;
			}

			unsafe AVAudioMix CreateAudioMix()
			{
				AVMutableAudioMix audioMix = AVMutableAudioMix.Create();
				AVMutableAudioMixInputParameters audioMixInputParameters =
					AVMutableAudioMixInputParameters.FromTrack(audioAssetTrack);
				var callbacks = new MTAudioProcessingTapCallbacks(TapProcess)
				{
					Initialize = TapInitialization,
					Finalize = Finalize,
					Prepare = TapPrepare,
					Unprepare = Unprepare,
				};

				audioProcessingTap = new MTAudioProcessingTap(callbacks, MTAudioProcessingTapCreationFlags.PreEffects);
				audioMixInputParameters.AudioTapProcessor = audioProcessingTap;

				audioMix.InputParameters = new AVAudioMixInputParameters[] {audioMixInputParameters};

				return audioMix;
			}

			#region Update parameters

			public void SetBand(int index, float gain)
			{
				if (context.AudioUnit == null)
					return;

				var bypass = context.AudioUnit.SetParameter(AudioUnitParameterType.AUNBandEQGain + index, gain * 1.5f,
					AudioUnitScopeType.Global);
				//Console.WriteLine(bypass);
			}

			#endregion

			#region MTAudioProcessingTap Callbacks

			unsafe void TapProcess(MTAudioProcessingTap tap, nint numberFrames, MTAudioProcessingTapFlags flags,
				AudioBuffers bufferList,
				out nint numberFramesOut,
				out MTAudioProcessingTapFlags flagsOut)
			{
				numberFramesOut = 0;
				flagsOut = (MTAudioProcessingTapFlags) 0;

				// Skip processing when format not supported.
				if (!context.SupportedTapProcessingFormat)
				{
					Console.WriteLine("Unsupported tap processing format.");
					return;
				}

				if (IsBandpassFilterEnabled)
				{
					// Apply bandpass filter Audio Unit.
					if (context.AudioUnit != null)
					{
						var audioTimeStamp = new AudioTimeStamp
						{
							SampleTime = context.SampleCount,
							Flags = AudioTimeStamp.AtsFlags.SampleTimeValid
						};

						var f = (AudioUnitRenderActionFlags) 0;
						var status = context.AudioUnit.Render(ref f, audioTimeStamp, 0, (uint) numberFrames, bufferList);
						if (status != AudioUnitStatus.NoError)
						{
							Console.WriteLine("AudioUnitRender(): {0}", status);
							return;
						}

						// Increment sample count for audio unit.
						context.SampleCount += numberFrames;

						// Set number of frames out.
						numberFramesOut = numberFrames;
					}
				}
				else
				{
					// Get actual audio buffers from MTAudioProcessingTap (AudioUnitRender() will fill bufferListInOut otherwise).
					CMTimeRange tr;
					var status = tap.GetSourceAudio(numberFrames, bufferList, out flagsOut, out tr, out numberFramesOut);
					if (status != MTAudioProcessingTapError.None)
					{
						Console.WriteLine("MTAudioProcessingTapGetSourceAudio: {0}", status);
						return;
					}
				}

				UpdateVolumes(bufferList, numberFrames);
			}

			unsafe void TapInitialization(MTAudioProcessingTap tap, out void* tapStorage)
			{
				context = new AVAudioTapProcessorContext
				{
					SupportedTapProcessingFormat = false,
					IsNonInterleaved = false,
					SampleRate = double.NaN,
					SampleCount = 0,
					LeftChannelVolume = 0,
					RightChannelVolume = 0
				};

				// We don't use tapStorage we store all data within context field
				tapStorage = (void*) IntPtr.Zero;
			}

			unsafe void Finalize(MTAudioProcessingTap tap)
			{
			}

			unsafe void TapPrepare(MTAudioProcessingTap tap, nint maxFrames, ref AudioStreamBasicDescription processingFormat)
			{
				// Store sample rate for CenterFrequency property
				context.SampleRate = processingFormat.SampleRate;

				/* Verify processing format (this is not needed for Audio Unit, but for RMS calculation). */
				VerifyProcessingFormat(processingFormat);

				if (processingFormat.FormatFlags.HasFlag(AudioFormatFlags.IsNonInterleaved))
					context.IsNonInterleaved = true;

				/* Create bandpass filter Audio Unit */

				var audioComponentDescription = AudioComponentDescription.CreateEffect(AudioTypeEffect.NBandEq);
				// TODO: https://trello.com/c/GZUGUyH0
				var audioComponent = AudioComponent.FindNextComponent(null, ref audioComponentDescription);
				if (audioComponent == null)
					return;

				var error = AudioUnitStatus.NoError;
				var audioUnit = audioComponent.CreateAudioUnit();
				try
				{
					audioUnit.SetFormat(processingFormat, AudioUnitScopeType.Input);
					audioUnit.SetFormat(processingFormat, AudioUnitScopeType.Output);
				}
				catch (AudioUnitException)
				{
					error = AudioUnitStatus.FormatNotSupported;
				}

				if (error == AudioUnitStatus.NoError)
					error = audioUnit.SetRenderCallback(Render, AudioUnitScopeType.Input);

				if (error == AudioUnitStatus.NoError)
					error = audioUnit.SetMaximumFramesPerSlice((uint) maxFrames, AudioUnitScopeType.Global);

				if (error == AudioUnitStatus.NoError)
					error = (AudioUnitStatus) audioUnit.Initialize();

				if (error != AudioUnitStatus.NoError)
				{
					audioUnit.Dispose();
					audioUnit = null;
				}

				context.AudioUnit = audioUnit;
				uint value = (uint)Parent.Bands.Length;
				uint size = sizeof (uint);
				var stat = AudioUnitSetProperty(audioUnit.Handle, AUNGraphicParams.NumberOfBands, AudioUnitScopeType.Global, 0,
					ref value, size);

				for (var i = 0; i < Parent.Bands.Length; i++)
				{
					var band = Parent.Bands[i];
					var freq = context.AudioUnit.SetParameter(AudioUnitParameterType.AUNBandEQFrequency + i, band.Center,
						AudioUnitScopeType.Global);
					var bypass = context.AudioUnit.SetParameter(AudioUnitParameterType.AUNBandEQBypassBand + i, 0,
						AudioUnitScopeType.Global);
					SetBand(i, band.Gain);
					Console.WriteLine(freq);
				}
			}

			enum AUNGraphicParams
			{
				NumberOfBands = 2200
			}

			[DllImport(Constants.AudioUnitLibrary)]
			static extern unsafe AudioUnitStatus AudioUnitGetProperty(IntPtr inUnit, AUNGraphicParams inID,
				AudioUnitScopeType inScope, uint inElement,
				ref uint outData,
				ref uint ioDataSize);

			[DllImport(Constants.AudioUnitLibrary)]
			static extern AudioUnitStatus AudioUnitSetProperty(IntPtr inUnit, AUNGraphicParams inID, AudioUnitScopeType inScope,
				int inElement, ref uint inData, uint inDataSize);

			void VerifyProcessingFormat(AudioStreamBasicDescription processingFormat)
			{
				context.SupportedTapProcessingFormat = true;

				if (processingFormat.Format != AudioFormatType.LinearPCM)
				{
					Console.WriteLine("Unsupported audio format for AudioProcessingTap. LinearPCM only.");
					context.SupportedTapProcessingFormat = false;
				}

				if (processingFormat.FormatFlags.HasFlag(AudioFormatFlags.IsFloat)) return;

				Console.WriteLine("Unsupported audio format flag for AudioProcessingTap. Float only.");
				context.SupportedTapProcessingFormat = false;
			}

			unsafe void Unprepare(MTAudioProcessingTap tap)
			{
				/* Release bandpass filter Audio Unit */

				if (context.AudioUnit == null)
					return;

				context.AudioUnit.Dispose();
			}

			AudioUnitStatus Render(AudioUnitRenderActionFlags actionFlags, AudioTimeStamp timeStamp, uint busNumber,
				uint numberFrames, AudioBuffers data)
			{
				// Just return audio buffers from MTAudioProcessingTap.
				MTAudioProcessingTapFlags flags;
				CMTimeRange range;
				nint n;
				var error =
					(AudioUnitStatus) (int) audioProcessingTap.GetSourceAudio((nint) numberFrames, data, out flags, out range, out n);
				if (error != AudioUnitStatus.NoError)
					Console.WriteLine("{0} audioProcessingTap.GetSourceAudio failed", error);
				return error;
			}

			#endregion

			unsafe void UpdateVolumes(AudioBuffers bufferList, nint numberFrames)
			{
				// Calculate root mean square (RMS) for left and right audio channel.
				// http://en.wikipedia.org/wiki/Root_mean_square
				for (int i = 0; i < bufferList.Count; i++)
				{
					var pBuffer = bufferList[i];
					long cSamples = numberFrames*(context.IsNonInterleaved ? 1 : pBuffer.NumberChannels);

					var pData = (float*) (void*) pBuffer.Data;

					float rms = 0;
					for (var j = 0; j < cSamples; j++)
						rms += pData[j]*pData[j];

					if (cSamples > 0)
						rms = (float) Math.Sqrt(rms/cSamples);

					if (i == 0)
						context.LeftChannelVolume = rms;
					if (i == 1 || (i == 1 && bufferList.Count == 1))
						context.RightChannelVolume = rms;
				}

				// Pass calculated left and right channel volume to VU meters.
				UpdateVolumes(context.LeftChannelVolume, context.RightChannelVolume);
			}

			void UpdateVolumes(float leftVolume, float rightVolume)
			{
				PlaybackManager.Shared.NativePlayer.AudioLevels = new float[] {leftVolume, rightVolume};
				// Forward left and right channel volume to Controller
				//DispatchQueue.MainQueue.DispatchAsync (() => Controller.OnNewLeftRightChanelValue (this, leftVolume, rightVolume));
			}
		}

	}
}