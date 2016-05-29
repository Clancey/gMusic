using System;
using System.Threading.Tasks;

using MusicPlayer.Models;

namespace TunezApi
{
	partial class TunezProvider
	{
		public override Task<bool> AddToLibrary(RadioStation station)
		{
			throw new NotImplementedException();
		}
		public override Task<RadioStation> CreateRadioStation(string name, Track track)
		{
			throw new NotImplementedException();
		}
		public override Task<RadioStation> CreateRadioStation(string name, AlbumIds track)
		{
			throw new NotImplementedException();
		}
		public override Task<RadioStation> CreateRadioStation(string name, ArtistIds track)
		{
			throw new NotImplementedException();
		}
		public override Task<RadioStation> CreateRadioStation(string name, RadioStationSeed seed)
		{
			throw new NotImplementedException();
		}
		public override Task<bool> DeleteRadioStation(RadioStation station)
		{
			throw new NotImplementedException();
		}
		public override Task<bool> LoadRadioStation(RadioStation station, bool isContinuation)
		{
			throw new NotImplementedException();
		}
	}
}
