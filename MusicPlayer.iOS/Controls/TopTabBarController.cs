using System;
using UIKit;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;

namespace MusicPlayer.iOS
{
	public class TopTabBarController : UIViewController
	{
		public TopTabBarController()
		{
			init();
			this.AutomaticallyAdjustsScrollViewInsets = false;
		}
		UIViewController[] viewControllers = new UIViewController[0];

		public UIViewController[] ViewControllers
		{
			get { return viewControllers; }
			set
			{
				view?.Reset();
				viewControllers = value;
				view?.SetContent();
			}
		}

		void init()
		{
			
		}

		nfloat topOffset = 64;

		public nfloat TopOffset
		{
			get { return topOffset; }
			set
			{
				if (topOffset == value)
					return;
				topOffset = value;
				view?.SetNeedsLayout();
			}
		}

		nfloat headerHeight = 44;

		public nfloat HeaderHeight
		{
			get { return headerHeight; }
			set
			{
				if (headerHeight == value)
					return;
				headerHeight = value;
				view?.SetNeedsLayout();
			}
		}

		UIFont titleFont = Style.DefaultStyle.HeaderTextThinFont;

		public UIFont TitleFont
		{
			get { return titleFont; }
			set
			{
				titleFont = value;
				view?.UpdateButtons();
			}
		}

		UIFont selectedTitleFont = Style.DefaultStyle.HeaderTextFont;

		public UIFont SelectedTitleFont
		{
			get { return selectedTitleFont; }
			set
			{
				selectedTitleFont = value;
				view?.UpdateButtons();
			}
		}

		UIColor titleColor = Style.DefaultStyle.HeaderTextColor.ColorWithAlpha(.25f);

		public UIColor TitleColor
		{
			get { return titleColor; }
			set
			{
				titleColor = value;
				view?.UpdateButtons();
			}
		}

		UIColor selectedTitleColor = Style.DefaultStyle.HeaderTextColor;

		public UIColor SelectedTitleColor
		{
			get { return selectedTitleColor; }
			set
			{
				selectedTitleColor = value;
				view?.UpdateButtons();
			}
		}


		UIColor headerBackgroundColor;

		public UIColor HeaderBackgroundColor
		{
			get { return headerBackgroundColor; }
			set
			{
				headerBackgroundColor = value;
				view?.UpdateButtons();
			}
		}

		PanaramaView view;

		public override void LoadView()
		{
			View = view = new PanaramaView(this);
			if (viewControllers.Length > 0)
			{
				view.SetContent();
			}
			TopOffset = View.GetSafeArea().Top;
		}
		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			ApplyStyle();
		}

		public void ScrollTo(int index, bool animated = true)
		{
			var v = View;
			view.ScrollTo(index, animated);
		}

		void ApplyStyle()
		{
			this.StyleViewController();
			var style = View.GetStyle();
			view.BackgroundColor = style.BackgroundColor;
			SelectedTitleColor = style.HeaderTextColor;
			this.HeaderBackgroundColor = style.BackgroundColor;
			this.TitleColor = style.HeaderTextColor.ColorWithAlpha(.25f);
		}
		public class PanaramaView : UIView
		{
			readonly CustomScroller scroller;
			List<SimpleButton> Buttons = new List<SimpleButton>();
			List<UIView> Views = new List<UIView>();
			UIView HeaderBackground;
			UIActivityIndicatorView spinner;
			WeakReference _parent;

			TopTabBarController Parent
			{
				get { return _parent?.Target as TopTabBarController; }
				set { _parent = new WeakReference(value); }
			}

			public PanaramaView(TopTabBarController viewController)
			{
				BackgroundColor = UIColor.White;
				Parent = viewController;
				scroller = new CustomScroller()
				{
					ScrollsToTop = false,
					PagingEnabled = true,
					TranslatesAutoresizingMaskIntoConstraints = false,
				};
				scroller.Scrolled += (sender, args) =>
				{
					var scroll = sender as CustomScroller;
					var p = scroll.Superview as PanaramaView;
					p.SetTopScroll();
				};
				scroller.DecelerationEnded += (sender, args) =>
				{
					var scroll = sender as CustomScroller;
					var p = scroll.Superview as PanaramaView;
					p.SetTopScroll();
				};
				Add(scroller);

				HeaderBackground = new UIView
				{
					BackgroundColor = Parent.HeaderBackgroundColor,
				};

				Add(spinner = new UIActivityIndicatorView
				{
					ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge,
				});
				spinner.StartAnimating();
			}

			void SetScrollToTop()
			{
				foreach (var vc in Parent.ViewControllers.OfType<UITableViewController>())
				{
					SetScrollToTop(vc);
				}
			}

			void SetScrollToTop(UITableViewController tvc)
			{
				if (tvc == null)
					return;
				var offset = scroller.ContentOffset;
				offset.Y += Parent.TopOffset;
				var enabled = tvc.View.Frame.Contains(offset);
				tvc.TableView.ScrollsToTop = enabled;
			}

			public override void LayoutSubviews()
			{
				base.LayoutSubviews();
				if (spinner != null)
				{
					spinner.Center = Center;
					this.BringSubviewToFront(spinner);
				}
				var frame = Bounds;
				frame.Height -= Parent.TopOffset;
				frame.Y = Parent.TopOffset;
				scroller.Frame = frame;
				SetScrollContent();
			}

			nfloat scrollWidth;

			void SetScrollContent()
			{
				var bounds = scroller.Bounds;
				bounds.Height -= Parent.HeaderHeight;
				bounds.Y = Parent.HeaderHeight;
				bounds.X = 0;
				scrollWidth = bounds.Width;
				foreach (var view in Views)
				{
					view.Frame = bounds;
					bounds.X += scrollWidth;
				}

				scroller.ContentSize = new CGSize(bounds.X, bounds.Height);
				//Set the header view to be bigger then the scrollview;
				var width = scroller.ContentSize.Width;
				HeaderBackground.Frame = new CGRect(-width, 0, width*3, Parent.HeaderHeight);
				SetTopScroll();

				ScrollTo(CurrentIndex, false);
			}

			void SetTopScroll()
			{
				if (Buttons.Count == 0)
					return;
				var scrollOffset = scroller.ContentOffset.X;
				var index = scrollWidth == 0 ? 0 : (scrollOffset/scrollWidth);
//				var intindex = (int)Math.Round (index, MidpointRounding.ToEven);
				var intindex = (int) Math.Max(0, Math.Min(index, Buttons.Count - 1));
				var currentButton = Buttons[intindex];

				//Figure out the exact middle of the screen inside the scrollview
				var half = scrollWidth/2;
				var x = (scrollWidth*index) + half - (currentButton.Frame.Width/2);


				//We need to calculate the distance we scroll based on the distance of the middles of the current label and the next label
				var nextIndex = intindex + 1;

				nfloat currentWidth = currentButton.Frame.Width;
				if (nextIndex < Buttons.Count)
					currentWidth = currentWidth/2 + headerPadding + Buttons[nextIndex].Frame.Width/2;

				//This is the start of the Current Label
				var leftX = x - (index - (nfloat) intindex)*currentWidth;

				var halfTop = Parent.HeaderHeight/2f;
				var previousIndexes = Enumerable.Range(0, intindex).Reverse();
				var nextIndexes = Enumerable.Range(intindex + 1, Buttons.Count - intindex - 1);

				//Set the current page frame
				currentButton.Font = Parent.SelectedTitleFont;
				currentButton.TitleColor = Parent.SelectedTitleColor;
				var frame = currentButton.Frame;
				frame.X = leftX;
				frame.Y = halfTop - frame.Height/2;
				currentButton.Frame = frame;


				var rightX = currentButton.Frame.Right + headerPadding;
				//Set the frame of the buttons in front of the current label ones
				foreach (var i in previousIndexes)
				{
					var button = Buttons[i];
					frame = button.Frame;
					leftX -= frame.Width + headerPadding;
					frame.X = leftX;
					frame.Y = halfTop - frame.Height/2;
					button.Frame = frame;

					button.Font = Parent.TitleFont;
					button.TitleColor = Parent.TitleColor;
				}

				//Set the frame of the next labels
				foreach (var i in nextIndexes)
				{
					if (i < 0)
						continue;
					if (i > Buttons.Count - 1)
						break;
					var button = Buttons[i];
					frame = button.Frame;
					frame.X = rightX;
					frame.Y = halfTop - frame.Height/2;
					button.Frame = frame;
					button.Font = Parent.TitleFont;
					button.TitleColor = Parent.TitleColor;
					rightX = frame.Right + headerPadding;
				}
			}

			public int CurrentIndex { get; set; }

			public void ScrollTo(int index, bool animated = true)
			{
				if (index < 0 || index >= Views.Count)
					return;
				CurrentIndex = index;
				var view = Views[index];
				scroller.ScrollRectToVisible(view.Frame, animated);
			}

			public void Reset()
			{
				Buttons.Clear();
				Views.Clear();
				scroller.Subviews.ForEach(x => x.RemoveFromSuperview());
				if (Parent != null)
					Parent.ViewControllers.ForEach(x => x.RemoveFromParentViewController());
			}

			public void SetContent()
			{
				if (Parent != null)
				{
					nint tag = 0;
					Parent.ViewControllers.ForEach(x =>
					{
						var button = new SimpleButton
						{
							Text = x.Title,
							Tag = tag++,
							Tapped = (b) =>
							{
								var s = b.Superview as CustomScroller;
								var p = s.Superview as PanaramaView;
								p.ScrollTo((int) b.Tag);
							}
						};
						if (Parent.SelectedTitleFont != null)
							button.Font = Parent.SelectedTitleFont;
						if (Parent.TitleColor != null)
							button.TitleColor = Parent.TitleColor;
						button.SizeToFit();
						Buttons.Add(button);
						Views.Add(x.View);
						scroller.Add(button);
						scroller.Add(x.View);
						Parent.AddChildViewController(x);
						SetScrollToTop(x as UITableViewController);
					});
					scroller.Add(HeaderBackground);
					SetScrollContent();
					if (Parent.ViewControllers.Length > 0)
					{
						spinner.RemoveFromSuperview();
						spinner = null;
					}
					UpdateButtons();
				}
			}

			static nfloat headerPadding = 20f;

			public void UpdateButtons()
			{
				scroller.BringSubviewToFront(HeaderBackground);
				Buttons.ForEach(x =>
				{
					if (Parent.TitleColor != null)
						x.SetTitleColor(Parent.TitleColor, UIControlState.Normal);
					if (Parent.TitleFont != null)
						x.Font = Parent.TitleFont;
					scroller.BringSubviewToFront(x);
					//					totalButtonWidth += x.Frame.Width;
				});
				HeaderBackground.BackgroundColor = Parent.HeaderBackgroundColor;
			}
		}

		public class CustomScroller : UIScrollView
		{
			public bool DisableScrolling { get; set; }

			public CustomScroller()
			{
			}

			public override CoreGraphics.CGPoint ContentOffset
			{
				get { return base.ContentOffset; }
				set
				{
					if (DisableScrolling)
						return;
					base.ContentOffset = value;
				}
			}
		}
	}
}