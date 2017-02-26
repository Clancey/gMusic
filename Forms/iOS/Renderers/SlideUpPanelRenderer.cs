using System;
using System.ComponentModel;
using System.Linq;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using MediaPlayer;
using MusicPlayer.Forms;
using gMusic.Forms.iOS;

[assembly: ExportRenderer(typeof(SlideUpPanel), typeof(SlideUpPanelRenderer))]
namespace gMusic.Forms.iOS
{
	public class SlideUpPanelRenderer : UIViewController, IVisualElementRenderer, IEffectControlProvider
	{

		public SlideUpPanel Model => (SlideUpPanel)Element;
		UIViewController _detailController;

		bool _disposed;
		EventTracker _events;

		UIViewController _masterController;

		UIPanGestureRecognizer _panGesture;

		bool _presented;

		VisualElementTracker _tracker;

		IPageController PageController => Element as IPageController;

		public SlideUpPanelRenderer()
		{
		}

		IMasterDetailPageController MasterDetailPageController => Element as IMasterDetailPageController;

		bool Presented
		{
			get { return _presented; }
			set
			{
				if (_presented == value)
					return;
				_presented = value;
				LayoutChildren(true);

				((IElementController)Element).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, value);
			}
		}

		public VisualElement Element { get; private set; }

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
		}

		public UIView NativeView
		{
			get { return View; }
		}

		public void SetElement(Xamarin.Forms.VisualElement element)
		{
			var oldElement = Element;
			Element = element;
			Element.SizeChanged += PageOnSizeChanged;

			_masterController = new ChildViewController();
			_detailController = new ChildViewController();

			Presented = ((MasterDetailPage)Element).IsPresented;

			OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

			//EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);

			//if (element != null)
			//	element.SendViewInitialized(NativeView);
		}

		public void SetElementSize(Size size)
		{
			Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
		}

		public UIViewController ViewController
		{
			get { return this; }
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			PageController.SendAppearing();
		}

		public override void ViewDidDisappear(bool animated)
		{
			base.ViewDidDisappear(animated);
			PageController?.SendDisappearing();
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			LayoutChildren(false);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			_tracker = new VisualElementTracker(this);
			_events = new EventTracker(this);
			_events.LoadEvents(View);

			((MasterDetailPage)Element).PropertyChanged += HandlePropertyChanged;


			PackContainers();
			UpdateMasterDetailContainers();

			UpdateBackground();

			UpdatePanGesture();
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			if (!MasterDetailPageController.ShouldShowSplitMode && _presented)
				Presented = false;

			base.WillRotate(toInterfaceOrientation, duration);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				Element.SizeChanged -= PageOnSizeChanged;
				Element.PropertyChanged -= HandlePropertyChanged;

				if (_tracker != null)
				{
					_tracker.Dispose();
					_tracker = null;
				}

				if (_events != null)
				{
					_events.Dispose();
					_events = null;
				}


				if (_panGesture != null)
				{
					if (View != null && View.GestureRecognizers.Contains(_panGesture))
						View.RemoveGestureRecognizer(_panGesture);
					_panGesture.Dispose();
				}

				EmptyContainers();

				PageController.SendDisappearing();

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
		{
			var changed = ElementChanged;
			if (changed != null)
				changed(this, e);
		}

		void EmptyContainers()
		{
			foreach (var child in _detailController.View.Subviews.Concat(_masterController.View.Subviews))
				child.RemoveFromSuperview();

			foreach (var vc in _detailController.ChildViewControllers.Concat(_masterController.ChildViewControllers))
				vc.RemoveFromParentViewController();
		}

		void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Page.IconProperty.PropertyName || e.PropertyName == Page.TitleProperty.PropertyName)
				UpdateLeftBarButton();
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Master" || e.PropertyName == "Detail")
				UpdateMasterDetailContainers();
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
				Presented = ((MasterDetailPage)Element).IsPresented;
			else if (e.PropertyName == MasterDetailPage.IsGestureEnabledProperty.PropertyName)
				UpdatePanGesture();
			else if (e.PropertyName == Xamarin.Forms.VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackground();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackground();
		}

		void LayoutChildren(bool animated)
		{
			var frame = Element.Bounds.ToRectangleF();
			var masterFrame = _masterController.View.Frame;
			var target = frame;
			target.Y = 0;
			if (!Presented)
				target.Y = target.Height - this.Model.OverHang;

			if (animated)
			{
				UIView.BeginAnimations("Flyout");
				var view = _masterController.View;
				view.Frame = target;
				UIView.SetAnimationCurve(UIViewAnimationCurve.EaseOut);
				UIView.SetAnimationDuration(250);
				UIView.CommitAnimations();
			}
			else
				_masterController.View.Frame = target;

			MasterDetailPageController.MasterBounds = new Rectangle(0, 0, frame.Width, frame.Height);
			MasterDetailPageController.DetailBounds = new Rectangle(0, 0, frame.Width, frame.Height);

		}

		void PackContainers()
		{
			_detailController.View.BackgroundColor = new UIColor(1, 1, 1, 1);
			View.AddSubview(_detailController.View);
			View.AddSubview(_masterController.View);

			AddChildViewController(_masterController);
			AddChildViewController(_detailController);
		}

		void PageOnSizeChanged(object sender, EventArgs eventArgs)
		{
			LayoutChildren(false);
		}


		void UpdateBackground()
		{
			if (!string.IsNullOrEmpty(((Page)Element).BackgroundImage))
				View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromBundle(((Page)Element).BackgroundImage));
			else if (Element.BackgroundColor == Color.Default)
				View.BackgroundColor = UIColor.White;
			else
				View.BackgroundColor = Element.BackgroundColor.ToUIColor();
		}

		void UpdateMasterDetailContainers()
		{
			((MasterDetailPage)Element).Master.PropertyChanged -= HandleMasterPropertyChanged;

			EmptyContainers();

			if (Platform.GetRenderer(((MasterDetailPage)Element).Master) == null)
				Platform.SetRenderer(((MasterDetailPage)Element).Master, Platform.CreateRenderer(((MasterDetailPage)Element).Master));
			if (Platform.GetRenderer(((MasterDetailPage)Element).Detail) == null)
				Platform.SetRenderer(((MasterDetailPage)Element).Detail, Platform.CreateRenderer(((MasterDetailPage)Element).Detail));

			var masterRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Master);
			var detailRenderer = Platform.GetRenderer(((MasterDetailPage)Element).Detail);

			((MasterDetailPage)Element).Master.PropertyChanged += HandleMasterPropertyChanged;

			_masterController.View.AddSubview(masterRenderer.NativeView);
			_masterController.AddChildViewController(masterRenderer.ViewController);

			_detailController.View.AddSubview(detailRenderer.NativeView);
			_detailController.AddChildViewController(detailRenderer.ViewController);

			SetNeedsStatusBarAppearanceUpdate();
		}

		void UpdateLeftBarButton()
		{
			var masterDetailPage = Element as MasterDetailPage;
			if (!(masterDetailPage?.Detail is NavigationPage))
				return;

			var detailRenderer = Platform.GetRenderer(masterDetailPage.Detail) as UINavigationController;

			UIViewController firstPage = detailRenderer?.ViewControllers.FirstOrDefault();
			//if (firstPage != null)
			//	NavigationRenderer.SetMasterLeftBarButton(firstPage, masterDetailPage);
		}

		public override UIViewController ChildViewControllerForStatusBarHidden()
		{
			if (((MasterDetailPage)Element).Detail != null)
				return (UIViewController)Platform.GetRenderer(((MasterDetailPage)Element).Detail);
			else
				return base.ChildViewControllerForStatusBarHidden();
		}

		const float FlickVelocity = 1000f;
		void UpdatePanGesture()
		{
			var model = (MasterDetailPage)Element;
			if (!model.IsGestureEnabled)
			{
				if (_panGesture != null)
					View.RemoveGestureRecognizer(_panGesture);
				return;
			}

			if (_panGesture != null)
			{
				View.AddGestureRecognizer(_panGesture);
				return;
			}

			UITouchEventArgs shouldRecieve = (g, touch) =>
			{
				bool isMovingCell =
							touch.View.ToString().IndexOf("UITableViewCellReorderControl", StringComparison.InvariantCultureIgnoreCase) >
							-1;
				if (isMovingCell || touch.View is UISlider || touch.View is MPVolumeView )
					return false;
				return true;
			};
			var center = new PointF();
			nfloat startY = 0;
			bool isPanning = false;
			_panGesture = new UIPanGestureRecognizer(g =>
			{
				var frame = _masterController.View.Frame;
				nfloat translation = g.TranslationInView(this.View).Y;
				switch (g.State)
				{
					case UIGestureRecognizerState.Began:
						isPanning = true;
						startY = frame.Y;
						center = g.LocationInView(g.View);
						break;
					case UIGestureRecognizerState.Changed:
						frame.Y = translation + startY;
						frame.Y = NMath.Min(frame.Height, NMath.Max(frame.Y, Model.OverHang * -1));
						_masterController.View.Frame = frame;
						break;
					case UIGestureRecognizerState.Ended:
						isPanning = false;
						var velocity = g.VelocityInView(this.View).Y;
						//					Console.WriteLine (velocity);
						var show = (Math.Abs(velocity) > FlickVelocity)
							? (velocity < 0)
							: (translation * -1 > 100);
						float playbackBarHideTollerance = (float)Model.OverHang * 2 / 3;
						if (show)
							Presented = true;
						else
							Presented = false;
						LayoutChildren(true);
						break;
				}
			});
			_panGesture.ShouldReceiveTouch = shouldRecieve;
			_panGesture.MaximumNumberOfTouches = 2;
			View.AddGestureRecognizer(_panGesture);
		}

		class ChildViewController : UIViewController
		{
			public override void ViewDidLayoutSubviews()
			{
				foreach (var vc in ChildViewControllers)
					vc.View.Frame = View.Bounds;
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			//VisualElementRenderer<VisualElement>RegisterEffect(effect, View);
		}
	}
}