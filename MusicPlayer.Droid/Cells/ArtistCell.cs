using System;
using Android.Views;
using Android.Content;
using Android.Widget;
using Android.Graphics;
using SimpleTables;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class ArtistCell  : Java.Lang.Object , ICell
	{
		public Artist Artist {get;set;}
		public int LayoutId { get; private set; }
		public Color BackGroundColor = Color.Black;
		public Color TextColor = Color.White;
		public virtual View GetCell (View convertView, ViewGroup parent, Context context, LayoutInflater inflater)
		{
			View view = null; // re-use an existing view, if one is available
			int type = Android.Resource.Layout.SimpleListItem1;
			if (view == null || view.Id != type) // otherwise create a new one
				view = inflater.Inflate (type , null);

			var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			textView.Text = Artist.Name;
			textView.SetTextColor (TextColor);
			textView.SetBackgroundColor(Color.Transparent);

			view.SetBackgroundColor(BackGroundColor);
			return view;
		}
	}
}
