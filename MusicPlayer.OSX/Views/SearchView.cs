using System;
using AppKit;
using MusicPlayer.ViewModels;
using CoreGraphics;
using System.Collections.Generic;
using System.Linq;

namespace MusicPlayer
{
	public class SearchView : NSView, ILifeCycleView, INavigationItem
	{
		NSSearchField SearchBar;
		NSScrollView SearchScrollView;
		List<SearchListResultView> tableViews = new List<SearchListResultView>();
		List<NSTextField> labels = new List<NSTextField>();

		SearchViewModel model = new SearchViewModel();
		public SearchViewModel Model {
			get {
				return model;
			}
			set {
				model = value;
			}
		}

		#region INavigationItem implementation

		public string Title { get; set; } = "Search";

		public NSNavigationController NavigationController { get; set; }
		#endregion

		public SearchView ()
		{
			AddSubview(SearchBar = new NSSearchField(new CGRect(0,0,400,50)));
			SearchBar.SearchingStarted += (object sender, EventArgs e) => {
				Model.Search(SearchBar.StringValue);
			};
			SearchBar.SendsSearchStringImmediately = false;
			AddSubview (SearchScrollView = new NSScrollView ());

		}

		#region ILifeCycleView implementation

		public void ViewWillAppear ()
		{
			CheckModels ();
		}

		public void ViewWillDissapear ()
		{
			
		}

		void CheckModels()
		{
			var newTables = Model.GetNewModels().Select(x => new SearchListResultView {Parent = this ,Model = x, AutoresizingMask = NSViewResizingMask.HeightSizable}).ToArray();
			newTables.ForEach ( (x)=>{
				tableViews.Add(x);
				SearchScrollView.AddSubview(x);
			});
			var newLabels = newTables.Select (x => new NSTextField{ StringValue = x.Title }.StyleAsMainTextCentered()).ToArray ();
			newLabels.ForEach ( (x)=>{
				labels.Add(x);
				SearchScrollView.AddSubview(x);
			});

			ResizeSubviewsWithOldSize (Bounds.Size);
		}
			

		#endregion

		public override bool IsFlipped {
			get {
				return true;
			}
		}
		public override void ResizeSubviewsWithOldSize (CGSize oldSize)
		{
			base.ResizeSubviewsWithOldSize (oldSize);
			var bounds = Bounds;
			SearchBar.SizeToFit ();
			var frame = SearchBar.Frame;
			frame.Width = bounds.Width;
			frame.Width -= 10f;
			SearchBar.Frame = frame;

			var y = frame.Bottom;

			frame = bounds;
			frame.Y = y;
			frame.Height = bounds.Height - y;
			SearchScrollView.Frame = frame;

			var width = 320;
			const float topHeight = 75f;
			frame = new CGRect (0, topHeight, width, frame.Height - topHeight);
			if (tableViews?.Any () == true) {
				for (var i = 0; i< tableViews.Count; i++) {
					var table = tableViews[i];
					var label = labels [i];
					label.SizeToFit ();
					table.Frame = frame;
					var labelFrame = label.Frame;;
					labelFrame.Width = width;
					labelFrame.X = frame.X;
					label.Frame = labelFrame;
					frame.X += width;
				}
			}
//			SearchScrollView = new CGSize (frame.X, frame.Height);
		}
	}
}

