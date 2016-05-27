using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UIKit;
using SimpleTables;

namespace MusicPlayer.iOS
{
	public class TabBar : UIView
	{

		public TabBar()
		{
			
		}
		List<TabButton> Buttons = new List<TabButton>();
		public UIColor SelectedColor { get; set; } = Style.DefaultStyle.AccentColor;
		 
		public UIColor UnselectedColor { get; set; } = UIColor.LightGray;

		public void Add(TabButton button)
		{
			Buttons.Add(button);
			AddSubview(button);
			button.TouchUpInside += Button_TouchUpInside;
		}

		public void Remove(TabButton button)
		{
			button.TouchUpInside -= Button_TouchUpInside;
		}

		public void Remove(int index)
		{
			Remove(Buttons[index]);
		}

		public void Insert(TabButton button,int index)
		{
			Buttons.Insert(index,button);
			InsertSubview(button, index);
		}

		bool hasSetup;
		public override void LayoutSubviews()
		{
			if (!hasSetup)
				SetButtonStates();
			hasSetup = true;

			base.LayoutSubviews();

			var width = Bounds.Width / Buttons.Count;
			var frame = Bounds;
			frame.Width = width;
			Buttons.ForEach(x =>
			{
				x.Frame = frame;
				frame.X += width;
			});
		}

		void Button_TouchUpInside(object sender, EventArgs e)
		{
			var index = Buttons.IndexOf(sender as TabButton);
			SelectedIndex = index;
		}

		public Action<TabButton> SelectedItemChanged;

		int selectedIndex;
		public int SelectedIndex
		{
			get{ return selectedIndex; }

			set
			{
				var old = selectedIndex;
				selectedIndex = Math.Max(Math.Min(value,Buttons.Count),0);
				SetButtonStates();
				if (old != selectedIndex)
					SelectedItemChanged?.Invoke(SelectedItem);
			}
		}


		void SetButtonStates()
		{
			var selected = selectedIndex > ((Buttons?.Count() ?? 0) - 1) ? null : Buttons[SelectedIndex];
			Buttons.ForEach(x => x.TintColor = x == selected ? SelectedColor : UnselectedColor);
		}

		public TabButton SelectedItem
		{
			get
			{
				var selected = selectedIndex > (Buttons.Count - 1) ? null : Buttons[SelectedIndex];
				return selected;
			}
			set
			{
				SelectedIndex = Buttons.IndexOf(value);
			}
		}
	}
}

