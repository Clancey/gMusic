using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace MusicPlayer
{
	public static class TableViewExtensions
	{
		public static T DequeueReusableCell<T>(this UITableView tv, string id) where T : UITableViewCell, new () 
		{
			var cell = tv.DequeueReusableCell(id) as T ?? new T();
			return cell;
		}
	}
}
