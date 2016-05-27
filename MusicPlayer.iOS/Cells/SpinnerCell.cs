using System;
using System.Collections.Generic;
using System.Text;
using UIKit;
using SimpleTables;

namespace MusicPlayer.Cells
{
	public class SpinnerCell : ICell
	{
		#region ICell implementation
		public UIKit.UITableViewCell GetCell(UIKit.UITableView tv)
		{
			var cell = tv.DequeueReusableCell(SpinnerTableViewCell.Key) as SpinnerTableViewCell ?? new SpinnerTableViewCell();
			cell.BackgroundColor = tv.BackgroundColor;
			cell.StartSpinner();
			return cell;
		}
		#endregion

		public class SpinnerTableViewCell : UITableViewCell
		{
			public const string Key = "SpinnerTableViewCell";

			UIActivityIndicatorView spinner;
			public SpinnerTableViewCell() : base(UITableViewCellStyle.Default, Key)
			{
				ContentView.Add(spinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.Gray));
				spinner.StartAnimating();
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				spinner.Frame = ContentView.Bounds;
			}

			public void StartSpinner()
			{
				
				spinner.StartAnimating();
			}
		}
	}
}
