using System;
using Android.Views;
using Android.Content;
using Android.Widget;
using Android.Graphics;
using SimpleTables;
using MusicPlayer.Models;

namespace MusicPlayer
{
	public class AlbumCell : Java.Lang.Object , ICell
	{
		public Album Album {get;set;}
		public int LayoutId { get; private set; }
		public Color BackGroundColor = Color.Black;
		public Color TextColor = Color.White;
		public virtual View GetCell (View convertView, ViewGroup parent, Context context,LayoutInflater inflater)
		{
			View view = null; // re-use an existing view, if one is available
			int type = Android.Resource.Layout.SimpleListItem2;
			if (view == null || view.Id != type) // otherwise create a new one
				view = inflater.Inflate (type , null);

			var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			textView.Text = Album.Name;
			textView.SetTextColor (TextColor);
			textView.SetBackgroundColor(Color.Transparent);


			var textView2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);
			textView2.Text = Album.Artist;
			textView2.SetTextColor (TextColor);
			textView2.SetBackgroundColor(Color.Transparent);

			view.SetBackgroundColor(BackGroundColor);
			return view;
		}
	}
}