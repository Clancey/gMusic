using System;
using Android.Views;
using Android.Content;
using Android.Widget;
using Android.Graphics;
using SimpleTables;
using MusicPlayer.Models;

namespace MusicPlayer.Droid
{
	public class SongCell : ICell
	{
		public Song Song {get;set;}
		public int LayoutId { get; private set; }
		public Color BackGroundColor = Color.Black;
		public Color TextColor = Color.White;
		static int cellCount;
		public virtual View GetCell (View convertView, ViewGroup parent, Context context, LayoutInflater inflater)
		{

			MyViewHolder holder = null;
			var view = convertView;

			if (view != null)
				holder = view.Tag as MyViewHolder;


			if (holder == null)
			{
				holder = new MyViewHolder();
				view = inflater.Inflate(Android.Resource.Layout.SimpleListItem2, null);
				holder.Name = view.FindViewById<TextView>(Android.Resource.Id.Text1);
				holder.Description = view.FindViewById<TextView>(Android.Resource.Id.Text2);
				//holder.Image = view.FindViewById<ImageView>(Resource.Id.imageView);
				view.Tag = holder;
			}


			holder.Name.Text = Song.Name;
			holder.Description.Text = Song.Artist;

			return view;

			//if(inflater == null)
			//	inflater = LayoutInflater.FromContext (context);
			//View view = convertView;
			//int type = Android.Resource.Layout.SimpleListItem2;
			//if (view == null || view.Id != type) // otherwise create a new one
			//{
			//	view = inflater.Inflate(type, null);

			//	Console.WriteLine("#######################.");
			//	Console.WriteLine("#######################");
			//	Console.WriteLine($"Created new Song Cell View {cellCount++}");
			//	Console.WriteLine("#######################");
			//	Console.WriteLine("#######################.");
			//}
			//view.Id = type;
			//var textView = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			//textView.Text = Song.Name;
			//textView.SetTextColor (TextColor);
			//textView.SetBackgroundColor(Color.Transparent);

			//var textView2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);
			//textView2.Text = Song.Artist;
			//textView2.SetTextColor (TextColor);
			//textView2.SetBackgroundColor(Color.Transparent);

			//view.SetBackgroundColor(BackGroundColor);
			//return view;
		}


		 class MyViewHolder : Java.Lang.Object
		{
			public TextView Name { get; set; }
			public TextView Description { get; set; }
			public ImageView Image { get; set; }
		}
	}
}