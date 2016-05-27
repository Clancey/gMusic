using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace MusicPlayer.Models
{
    public class RadioStationSeed
    {
		[PrimaryKey]
		public string Id {get; set; }

		[Indexed]
		public string StationId { get; set; }
		
		public string ItemId { get; set; }

		/// <summary>
		/// 3 = Artist
		/// 9 = Currated Station
		/// </summary>
		public int Kind { get; set; }

		public string Description {get; set; }
    }
}
