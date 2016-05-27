using System;
using System.Threading.Tasks;
using Localizations;
using MonoTouch.Dialog;
using MusicPlayer.Models;

namespace MusicPlayer.iOS
{
	public class SongTagEditor : DialogViewController
	{
		OnlineSong song;
		public OnlineSong Song {
			get {
				return song;
			}
			set {
				song = value;
				PopulateValues ();
			}
		}
		EntryElement album;
		EntryElement name;
		EntryElement artist;
		EntryElement albumArtist;
		EntryElement genre;
		EntryElement trackNumber;
		EntryElement trackTotal;
		EntryElement disc;
		EntryElement discTotal;
		EntryElement year;
		EntryElement comments;
		EntryElement bpm;

		public SongTagEditor () : base (UIKit.UITableViewStyle.Grouped,null)
		{
			//this.TableView.BackgroundColor = gMusic.Style.Current.ScreensDefaults.Background.Value;
			Root = new RootElement ("") {
				new Section () {
					(name = new EntryElement (Strings.Name, "", null)),
					(artist = new EntryElement (Strings.Artist, "", null)),
					(album = new EntryElement (Strings.Album, "", null)),
					(albumArtist = new EntryElement (Strings.AlbumArtist, "", null)),
					(genre = new EntryElement(Strings.Genre,"",null)),
					(trackNumber = new EntryElement(Strings.Track, Strings.TrackNumber,null)),
					(disc = new EntryElement(Strings.Disc,"",null)),
					(year = new EntryElement(Strings.Year,"",null)),
				}
			};
			this.NavigationItem.RightBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Save, (s, e) => {
				Save();
				tcs.TrySetResult(true);
				dismiss();
			});
			this.NavigationItem.LeftBarButtonItem = new UIKit.UIBarButtonItem (UIKit.UIBarButtonSystemItem.Cancel, (s, e) => {
				tcs.TrySetResult(false);
				dismiss();
			});
		}
		void PopulateValues()
		{
			this.Title = song.Name ?? "";
			name.Value = song.Name;
			artist.Value = song.TrackData.Artist;
			albumArtist.Value = song.TrackData.AlbumArtist;
			album.Value = song.TrackData.Album;
			genre.Value = song.TrackData.Genre;
			trackNumber.Value = song.TrackData.Track.ToString();
			disc.Value = song.TrackData.Disc.ToString();
			year.Value = song.TrackData.Year.ToString();
		}

		void Save()
		{
			name.FetchValue ();
			song.Name =  name.Value;

			artist.FetchValue ();
			song.TrackData.Artist = artist.Value;

			album.FetchValue ();

			song.TrackData.Album = album.Value;

			albumArtist.FetchValue ();
			song.TrackData.AlbumArtist =  albumArtist.Value;

			genre.FetchValue ();
			song.TrackData.Genre = genre.Value;

			trackNumber.FetchValue ();
			int i;
			if (int.TryParse (trackNumber.Value, out i))
				song.TrackData.Track = i;

			disc.FetchValue ();
			if (int.TryParse(disc.Value, out i))
				song.TrackData.Disc = i;

			year.FetchValue ();
			if (int.TryParse (year.Value, out i))
				song.TrackData.Year = i;

		}
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		public Task<bool> GetValues()
		{
			return tcs.Task;
		}
		void dismiss()
		{

			if(NavigationController != null && NavigationController.ViewControllers.Length > 1)
				this.NavigationController.PopViewController(true);
			else
				this.DismissViewController(true,null);
		}
	}
}

