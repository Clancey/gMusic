using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MusicPlayer.Managers;
using MusicPlayer.Models;
using MusicPlayer.Models.Scrobbling;
using MusicPlayer.Playback;
using SimpleDatabase;

namespace MusicPlayer.Data
{
	internal class ScrobbleDatabase : SimpleDatabaseConnection
	{
		public static ScrobbleDatabase Main { get; set; } = new ScrobbleDatabase();
		static string dbPath => Path.Combine(Locations.LibDir, "scrobble.db");

		public ScrobbleDatabase() : base(dbPath)
		{
			CreateTables(
				typeof (PlaybackEndedEvent),
				typeof (PlaybackContext)
				);
		}
	}
}