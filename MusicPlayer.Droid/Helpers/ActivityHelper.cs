using System;
using Android.App;
using System.Reflection;
using System.Linq;
using Android.Views;

namespace MusicPlayer
{
	public static class ActivityHelper
	{
		public static void WireUpViews(this Activity activity)
		{
			//Get all the View fields from the activity
			var members = from m in activity.GetType ().GetFields (BindingFlags.NonPublic | BindingFlags.Instance)
					where m.FieldType.IsSubclassOf (typeof(View))
				select m;

			if (!members.Any ())
				return;

			members.ToList ().ForEach (m => {
				try
				{
					//Find the android identifier with the same name
					var id = activity.Resources.GetIdentifier(m.Name, "id", activity.PackageName);
					//Set the activity field's value to the view with that identifier
					m.SetValue (activity, activity.FindViewById (id));
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed to wire up the field "
						+ m.Name + " to a View in your layout with a corresponding identifier");
//					throw new MissingFieldException ("Failed to wire up the field "
//						+ m.Name + " to a View in your layout with a corresponding identifier", ex);
				}
			});
		}
	}
}

