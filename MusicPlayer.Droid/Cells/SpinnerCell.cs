using System;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Widget;
using SimpleTables;

namespace MusicPlayer.Cells
{
	public class SpinnerCell: Java.Lang.Object, ICell
	{
		
		public int LayoutId { get; private set; }
		public virtual View GetCell(View convertView, ViewGroup parent, Context context, LayoutInflater inflater)
		{
			View view = null; // re-use an existing view, if one is available
			int type = Android.Resource.Layout.SimpleSpinnerItem;
			if (view == null || view.Id != type) // otherwise create a new one
				view = inflater.Inflate(type, null);
			
			return view;
		}
	}
}