using System;
using Foundation;
using System.Collections;
using System.Collections.Generic;

namespace MusicPlayer
{
	public class MenuSection : Element, IEnumerable
	{
		public bool IsExpandable { get; set; } = true;

		public List<Element> Children {get; private set;} = new List<Element>();

		public MenuSection ()
		{
			
		}
		public MenuSection (string text)
		{
			Text = text;
		}
		public void Add(Element element)
		{
			Children.Add (element);
		}

		#region IEnumerable implementation

		public IEnumerator GetEnumerator ()
		{
			return Children.GetEnumerator ();
		}

		#endregion
	}
}

