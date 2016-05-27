//using System;
//using SimpleTables;
//using Android.Graphics;
//using Android.Views;
//using Android.Widget;
//using Android.Content;

//namespace MusicPlayer.Droid
//{
//	public class MediaItemCell : Java.Lang.Object , ICell
//	{
//		public MediaItem MediaItem {get;set;}
//		public int LayoutId { get; private set; }
//		public Color BackGroundColor = Color.Black;
//		public Color TextColor = Color.White;
//		public virtual View GetCell (View convertView, ViewGroup parent, Context context)
//		{
//			var inflater = LayoutInflater.FromContext (context);
//			View view = null; // re-use an existing view, if one is available
//			int type = Resource.Layout.ImageCell;
//			if (view == null || view.Id != type) // otherwise create a new one
//				view = inflater.Inflate (type , null);
//			var imageView = view.FindViewById<ImageView> (Resource.Id.Image);
//			Koush.UrlImageViewHelper.SetUrlDrawable (imageView, MediaItem.ImageUrl);
//			var textView = view.FindViewById<TextView> (Resource.Id.Text1);
//			textView.Text = MediaItem.Title;
//			textView.SetTextColor (TextColor);
//			textView.SetBackgroundColor(Color.Transparent);

//			var textView2 = view.FindViewById<TextView> (Resource.Id.Text2);
//			textView2.Text = MediaItem.OrigionalUrl;
//			textView2.SetTextColor (TextColor);
//			textView2.SetBackgroundColor(Color.Transparent);

//			view.SetBackgroundColor(BackGroundColor);
//			return view;
//		}
//	}
//}