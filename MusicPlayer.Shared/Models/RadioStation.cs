using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MusicPlayer.Api;
using MusicPlayer.Data;
using SQLite;
using SimpleDatabase;

namespace MusicPlayer.Models
{
	public class OnlineRadioStation : RadioStation
	{
		
	}
	public class RadioStation : MediaItemBase
	{
		public RadioStation()
		{
		}

		public RadioStation(string name) : base(name)
		{
		}

		[Indexed]
		public bool IsIncluded { get; set; }

		public string Description { get; set; }

		public long DateCreated { get; set; }

		[Indexed]
		public long RecentDateTime { get; set; }

		[Indexed]
		public ServiceType ServiceType { get; set; }

		[Indexed]
		public string ServiceId { get; set; }

		[Indexed]
		public bool Deleted { get; set; }

		public override bool ShouldBeLocal()
		{
			return false;
		}

		public override string DetailText => Description;

		RadioStationArtwork[] allArtwork;

		[Ignore]
		public RadioStationArtwork[] AllArtwork
		{
			set { allArtwork = value; }
		}

		public async Task<RadioStationArtwork[]> GetAllArtwork()
		{

			if (allArtwork != null)
				return allArtwork;

			var art = await Database.Main.TablesAsync<RadioStationArtwork>().Where(x => x.StationId == Id).ToListAsync() ??
				new List<RadioStationArtwork>();
			return allArtwork = art.ToArray();
		}


		RadioStationSeed[] stationSeeds;

		[Ignore]
		public RadioStationSeed[] StationSeeds
		{
			get
			{
				if (stationSeeds != null)
					return stationSeeds;

				var art = Database.Main.TablesAsync<RadioStationSeed>().Where(x => x.StationId == Id).ToListAsync().Result ??
						new List<RadioStationSeed>();
				return stationSeeds = art.ToArray();
			}
			set { stationSeeds = value; }
		}
	}
}