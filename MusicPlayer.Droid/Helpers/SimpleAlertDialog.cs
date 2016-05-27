using System;
using System.Collections.Generic;
using Android.Content;
using Android.App;
using System.Linq;


namespace MusicPlayer
{
	public class SimpleAlertDialog
	{
		public SimpleAlertDialog ()
		{

		}
		public string Title {get;set;}
		readonly List<Tuple<string,Action>> items = new List<Tuple<string,Action>>();
		public void Add(string title, Action action)
		{
			items.Add (new Tuple<string,Action>(title, action));
		}
		public void Show(Context context)
		{
			var builder = new AlertDialog.Builder (context).SetTitle (Title).
				SetItems (items.Select (x => x.Item1).ToArray (),(s,e)=>{
					var item = items[e.Which];
					if(item.Item2 != null)
					{
						item.Item2();
					}
				});

			builder.SetCancelable (true);

			builder.Show ();
		}
	}
}

