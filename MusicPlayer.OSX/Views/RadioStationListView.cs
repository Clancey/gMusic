using System;
using MusicPlayer.ViewModels;
using MusicPlayer.Models;
using AppKit;

namespace MusicPlayer
{
	public class RadioStationListView :  BaseCollectionView<RadioStationViewModel,RadioStation>
	{
		public RadioStationListView ()
		{
			Frame = new CoreGraphics.CGRect (0, 0, 500, 500);
			Model = new RadioStationViewModel{
				AutoPlaysOnSelect = false,
			};
		}
		public bool IsIncluded
		{
			get{ return Model.IsIncluded; }
			set{ Model.IsIncluded = value; }
		}
	}
}

