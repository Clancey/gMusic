using System;
using System.Runtime.InteropServices;
using AudioUnit;
using ObjCRuntime;
using CoreFoundation;
using Foundation;
using System.Text;
namespace MusicPlayer
{
	public class AudioOutputHelper
	{
		const uint AudioDevicePropertyDataSourceNameForIDCFString = 1819501422;
		static uint kAudioHardwarePropertyDefaultOutputDevice = ConvertToUInt("dOut");
		const uint AudioDevicePropertyDataSource = 1936945763;
		const uint AudioDevicePropertyTransportType = 1953653102;
		public static  Action OutputChanged { get; set; }
		public static void Init()
		{
			try
			{
				var src = ConvertToUInt("dOut");
				uint theRunLoop = 0;
				var property  = new AudioObjectPropertyAddress(
					kAudioHardwarePropertyDefaultOutputDevice,
					(uint)AudioObjectPropertyScope.Global,
					(uint)AudioObjectPropertyElement.Master);

				AudioObjectAddPropertyListener(1, ref property, OutputDidChange, IntPtr.Zero);

				//var defaultDevice = GetCurrentOutputDevice();
				////	var name = GetDeviceName (defaultDevice);
				//var sourceId = GetDeviceSourceId(defaultDevice);
				////	var trasnport = ConvertToString (GetDeviceProperty (defaultDevice, AudioDevicePropertyTransportType));
				//var theAddress = new AudioObjectPropertyAddress(
				//	AudioDevicePropertyDataSource,
				//	(uint)AudioObjectPropertyScope.Output,
				//	(uint)AudioObjectPropertyElement.Master);
				//AudioObjectAddPropertyListener(1, ref theAddress, OutputDidChange, IntPtr.Zero);


			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		struct AudioObjectPropertyAddress
		{
			public uint /* UInt32 */ Selector;
			public uint /* UInt32 */ Scope;
			public uint /* UInt32 */ Element;

			public AudioObjectPropertyAddress(uint selector, uint scope, uint element)
			{
				Selector = selector;
				Scope = scope;
				Element = element;
			}

			public AudioObjectPropertyAddress(AudioObjectPropertySelector selector, AudioObjectPropertyScope scope, AudioObjectPropertyElement element)
			{
				Selector = (uint)selector;
				Scope = (uint)scope;
				Element = (uint)element;
			}
		}

		public static uint GetCurrentOutputDevice()
		{
			uint outputDevice;
			uint size = (uint)Marshal.SizeOf(typeof(uint));

			var theAddress = new AudioObjectPropertyAddress(
				AudioObjectPropertySelector.DefaultSystemOutputDevice,
				AudioObjectPropertyScope.Global,
				AudioObjectPropertyElement.Master);

			uint inQualifierDataSize = 0;
			IntPtr inQualifierData = IntPtr.Zero;

			var err = AudioObjectGetPropertyData(1, ref theAddress, ref inQualifierDataSize, ref inQualifierData, ref size, out outputDevice);

			if (err != 0)
			{
				var status = (AudioUnitStatus)err;
				Console.WriteLine(status);
			}
			return outputDevice;
		}
		public static string GetDeviceName(uint deviceId)
		{
			var property = GetDeviceProperty(deviceId, AudioDevicePropertyDataSourceNameForIDCFString);
			return ConvertToString(property);
		}
		public static string GetDeviceSourceId(uint deviceId)
		{
			var property = GetDeviceProperty(deviceId, AudioDevicePropertyDataSource);
			return ConvertToString(property);
		}
		public static uint GetDeviceProperty(uint deviceId, uint property)
		{
			uint dataSourceId;
			uint size = (uint)Marshal.SizeOf(typeof(UInt64));
			var theAddress = new AudioObjectPropertyAddress(
				property,
				(uint)AudioObjectPropertyScope.Output,
				(uint)AudioObjectPropertyElement.Master);
			uint inQualifierDataSize = 0;
			IntPtr inQualifierData = IntPtr.Zero;

			var err = AudioObjectGetPropertyData(deviceId, ref theAddress, ref inQualifierDataSize, ref inQualifierData, ref size, out dataSourceId);

			if (err != 0)
			{
				var error = new NSError(NSError.OsStatusErrorDomain, err);
				Console.WriteLine(error);
			}

			return dataSourceId;
		}

		static bool SetDeviceProperty(AudioObjectPropertyAddress theAddress, uint deviceId, uint data)
		{
			uint dataSourceId = data;
			uint size = (uint)Marshal.SizeOf(typeof(UInt64));
			uint inQualifierDataSize = 0;
			IntPtr inQualifierData = IntPtr.Zero;

			var err = AudioObjectSetPropertyData(deviceId, ref theAddress, ref inQualifierDataSize, ref inQualifierData, ref size, ref dataSourceId);

			if (err != 0)
			{
				var error = new NSError(NSError.OsStatusErrorDomain, err);
				Console.WriteLine(error);
				return false;
			}

			return true;;
		}


		static string ConvertToString(uint input)
		{
			if (input == 0)
				return "";
			var hexString = input.ToString("x8");
			var sb = new StringBuilder();
			for (int i = 0; i < hexString.Length; i += 2)
			{
				string hs = hexString.Substring(i, 2);
				sb.Append(Convert.ToChar(Convert.ToUInt32(hs, 16)));
			}
			return sb.ToString();
		}


		static uint ConvertToUInt(string input)
		{
			if (string.IsNullOrWhiteSpace(input))
				return 0;
			var hex = HexIt(input);
			return Convert.ToUInt32(hex, 16);
		}
		public static string HexIt(string yourString)
		{
			string hex = "";
			foreach (char c in yourString)
			{
				int tmp = c;
				hex += String.Format("{0:x2}", (uint)System.Convert.ToUInt32(tmp.ToString()));
			}
			return hex;
		}

		[DllImport(Constants.SecurityLibrary)]
		static extern IntPtr SecCopyErrorMessageString(uint status, IntPtr reserved);

		[MonoPInvokeCallback(typeof(OutputChange))]
		static void OutputDidChange(uint inObjectID, uint inNumberAddresses, AudioObjectPropertyAddress inAddresses, IntPtr clientData)
		{
			Console.WriteLine("Output Changed");
			OutputChanged?.Invoke();
		}

		public static uint GetDeviceId(uint deviceId)
		{
			uint dataSourceId;
			uint size = (uint)Marshal.SizeOf(typeof(uint));
			var theAddress = new AudioObjectPropertyAddress(
				AudioDevicePropertyDataSource,
				(uint)AudioObjectPropertyScope.Output,
				(uint)AudioObjectPropertyElement.Master);
			uint inQualifierDataSize = 0;
			IntPtr inQualifierData = IntPtr.Zero;

			var err = AudioObjectGetPropertyData(deviceId, ref theAddress, ref inQualifierDataSize, ref inQualifierData, ref size, out dataSourceId);

			//if (err != 0)
			//throw new AudioUnitException((int)err);
			return dataSourceId;
		}

		[DllImport(Constants.AudioUnitLibrary)]
		static extern int AudioObjectGetPropertyData(
			uint inObjectID,
			ref AudioObjectPropertyAddress inAddress,
			ref uint inQualifierDataSize,
			ref IntPtr inQualifierData,
			ref uint ioDataSize,
			out uint outData
		);

		[DllImport(Constants.AudioUnitLibrary)]
		static extern int AudioObjectSetPropertyData(
			uint inObjectID,
			ref AudioObjectPropertyAddress inAddress,
			ref uint inQualifierDataSize,
			ref IntPtr inQualifierData,
			ref uint ioDataSize,
			ref uint inData
		);

		delegate void OutputChange(uint inObjectID, uint inNumberAddresses, AudioObjectPropertyAddress inAddresses, IntPtr clientData);

		[DllImport(Constants.AudioUnitLibrary)]
		static extern int AudioObjectAddPropertyListener(uint inObjectID,
															  ref AudioObjectPropertyAddress inAddress,
															  OutputChange callback, IntPtr clientData);

		[DllImport(Constants.libcLibrary)]
		// dispatch_queue_t dispatch_get_global_queue (long priority, unsigned long flags);
		extern static IntPtr dispatch_get_global_queue(nint priority, nuint flags);
	}
}