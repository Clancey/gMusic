using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.App;
using Android.Views;
using AView = Android.Views.View;
using AColor = Android.Graphics.Drawables.ColorDrawable;
using Android.Support.Design.Widget;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Android.Content.Res;
using Android.Content;
using Android.Runtime;
using MusicPlayer.Forms.Droid;
using MusicPlayer.Forms;

[assembly:ExportRenderer(typeof(SlideUpPanel),typeof(SlideUpPanelRenderer))]
namespace MusicPlayer.Forms.Droid
{
	public class SlideUpPanelRenderer: CoordinatorLayout, IVisualElementRenderer
	{
		//from Android source code
		const uint DefaultScrimColor = 0x99000000;
		int _currentLockMode = -1;
		MasterDetailContainer _detailLayout;
		bool _isPresentingFromCore;
		MasterDetailContainer _masterLayout;
		MasterDetailPage _page;
		bool _presented;

		public SlideUpPanelRenderer() : base(MainActivity.Shared)
		{
		}

		IMasterDetailPageController MasterDetailPageController => _page as IMasterDetailPageController;

		public bool Presented
		{
			get { return _presented; }
			set
			{
				if (value == _presented)
					return;
				UpdateSplitViewLayout();
				_presented = value;
				if (_page.MasterBehavior == MasterBehavior.Default && MasterDetailPageController.ShouldShowSplitMode)
					return;

				//TODO:
				//if (_presented)
				//	OpenDrawer(_masterLayout);
				//else
				//	CloseDrawer(_masterLayout);
			}
		}

		IPageController MasterPageController => _page.Master as IPageController;
		IPageController DetailPageController => _page.Detail as IPageController;
		IPageController PageController => Element as IPageController;

		public void OnDrawerClosed(AView drawerView)
		{
		}

		public void OnDrawerOpened(AView drawerView)
		{
		}

		public void OnDrawerSlide(AView drawerView, float slideOffset)
		{
		}

		public void OnDrawerStateChanged(int newState)
		{
			//TODO:
			//_presented = IsDrawerVisible(_masterLayout);
			UpdateIsPresented();
		}

		public VisualElement Element
		{
			get { return _page; }
		}

		public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

		public SizeRequest GetDesiredSize(int widthConstraint, int heightConstraint)
		{
			Measure(widthConstraint, heightConstraint);
			return new SizeRequest(new Size(MeasuredWidth, MeasuredHeight));
		}
		AnchorBottomSheetBehavior behaviour;
		public void SetElement(VisualElement element)
		{
			MasterDetailPage oldElement = _page;
			_page = element as MasterDetailPage;

			_detailLayout = new MasterDetailContainer(_page, false, Context) { LayoutParameters = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) };

			var parameters = new CoordinatorLayout.LayoutParams(new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent) { Gravity = (int)GravityFlags.Start });
			parameters.Behavior = new AnchorBottomSheetBehavior();

			_masterLayout = new MasterDetailContainer(_page, true, Context)
			{
				LayoutParameters = parameters,
			};
			_masterLayout.RequestLayout();
			AddView(_detailLayout);

			AddView(_masterLayout);
			behaviour = AnchorBottomSheetBehavior.From(_masterLayout);

			behaviour.PeekHeight = 200;


			var activity = Context as Activity;
			//activity.ActionBar.SetDisplayShowHomeEnabled(true);
			//activity.ActionBar.SetHomeButtonEnabled(true);

			UpdateBackgroundColor(_page);
			UpdateBackgroundImage(_page);

			OnElementChanged(oldElement, element);

			if (oldElement != null)
				((IMasterDetailPageController)oldElement).BackButtonPressed -= OnBackButtonPressed;

			if (_page != null)
				MasterDetailPageController.BackButtonPressed += OnBackButtonPressed;

			if (Tracker == null)
				Tracker = new VisualElementTracker(this);

			_page.PropertyChanged += HandlePropertyChanged;
			_page.Appearing += MasterDetailPageAppearing;
			_page.Disappearing += MasterDetailPageDisappearing;

			UpdateMaster();
			UpdateDetail();

			//Device.Info.PropertyChanged += DeviceInfoPropertyChanged;
			SetGestureState();

			Presented = _page.IsPresented;

			//AddDrawerListener(this);

			//if (element != null)
			//	element.SendViewInitialized(this);

			if (element != null && !string.IsNullOrEmpty(element.AutomationId))
				ContentDescription = element.AutomationId;
		}

		public VisualElementTracker Tracker { get; private set; }

		public void UpdateLayout()
		{
			if (Tracker != null)
				Tracker.UpdateLayout();
		}

		public ViewGroup ViewGroup
		{
			get { return this; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Tracker != null)
				{
					Tracker.Dispose();
					Tracker = null;
				}

				if (_detailLayout != null)
				{
					_detailLayout.Dispose();
					_detailLayout = null;
				}

				if (_masterLayout != null)
				{
					_masterLayout.Dispose();
					_masterLayout = null;
				}

				//Device.Info.PropertyChanged -= DeviceInfoPropertyChanged;

				if (_page != null)
				{
					MasterDetailPageController.BackButtonPressed -= OnBackButtonPressed;
					_page.PropertyChanged -= HandlePropertyChanged;
					_page.Appearing -= MasterDetailPageAppearing;
					_page.Disappearing -= MasterDetailPageDisappearing;
					//_page.ClearValue(Platform.RendererProperty);
					_page = null;
				}
			}

			base.Dispose(disposing);
		}

		public override void OnAttachedToWindow()
		{
			base.OnAttachedToWindow();
			PageController.SendAppearing();
		}

		public override void OnDetachedFromWindow()
		{
			base.OnDetachedFromWindow();
			PageController.SendDisappearing();
		}

		protected virtual void OnElementChanged(VisualElement oldElement, VisualElement newElement)
		{
			EventHandler<VisualElementChangedEventArgs> changed = ElementChanged;
			if (changed != null)
				changed(this, new VisualElementChangedEventArgs(oldElement, newElement));
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			base.OnLayout(changed, l, t, r, b);
			//hack to make the split layout handle touches the full width
			if (MasterDetailPageController.ShouldShowSplitMode && _masterLayout != null)
				_masterLayout.Right = r;
		}

		async void DeviceInfoPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "CurrentOrientation")
			{
				if (!MasterDetailPageController.ShouldShowSplitMode && Presented)
				{
					MasterDetailPageController.CanChangeIsPresented = true;
					//hack : when the orientation changes and we try to close the Master on Android		
					//sometimes Android picks the width of the screen previous to the rotation 		
					//this leaves a little of the master visible, the hack is to delay for 50ms closing the drawer
					await Task.Delay(50);
					//CloseDrawer(_masterLayout);
				}
				UpdateSplitViewLayout();
			}
		}

		void HandleMasterPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//if (e.PropertyName == Page.TitleProperty.PropertyName || e.PropertyName == Page.IconProperty.PropertyName)
			//	((Platform)_page.Platform).UpdateMasterDetailToggle(true);
		}

		void HandlePropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "Master")
				UpdateMaster();
			else if (e.PropertyName == "Detail")
			{
				UpdateDetail();
				//((Platform)_page.Platform).UpdateActionBar();
			}
			else if (e.PropertyName == MasterDetailPage.IsPresentedProperty.PropertyName)
			{
				_isPresentingFromCore = true;
				Presented = _page.IsPresented;
				_isPresentingFromCore = false;
			}
			else if (e.PropertyName == "IsGestureEnabled")
				SetGestureState();
			else if (e.PropertyName == Page.BackgroundImageProperty.PropertyName)
				UpdateBackgroundImage(_page);
			if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
				UpdateBackgroundColor(_page);
		}

		void MasterDetailPageAppearing(object sender, EventArgs e)
		{
			MasterPageController?.SendAppearing();
			DetailPageController?.SendAppearing();
		}

		void MasterDetailPageDisappearing(object sender, EventArgs e)
		{
			MasterPageController?.SendDisappearing();
			DetailPageController?.SendDisappearing();
		}

		void OnBackButtonPressed(object sender, BackButtonPressedEventArgs backButtonPressedEventArgs)
		{
			if (behaviour.State != AnchorBottomSheetState.Collapsed)
			{
				behaviour.State = AnchorBottomSheetState.Collapsed;
				backButtonPressedEventArgs.Handled = true;
			}

		}

		void SetGestureState()
		{
			//SetDrawerLockMode(_page.IsGestureEnabled ? LockModeUnlocked : LockModeLockedClosed);
		}

		//void IVisualElementRenderer.SetLabelFor(int? id)
		//{
		//}

		void SetLockMode(int lockMode)
		{
			if (_currentLockMode != lockMode)
			{
				//SetDrawerLockMode(lockMode);
				_currentLockMode = lockMode;
			}
		}

		void UpdateBackgroundColor(Page view)
		{
			if (view.BackgroundColor != Color.Default)
				SetBackgroundColor(view.BackgroundColor.ToAndroid());
		}

		void UpdateBackgroundImage(Page view)
		{
			if (!string.IsNullOrEmpty(view.BackgroundImage))
				this.SetBackground(Context.Resources.GetDrawable(view.BackgroundImage));
		}

		void UpdateDetail()
		{
			Context.HideKeyboard(this);
			_detailLayout.ChildView = _page.Detail;
		}

		void UpdateIsPresented()
		{
			if (_isPresentingFromCore)
				return;
			if (Presented != _page.IsPresented)
				((IElementController)_page).SetValueFromRenderer(MasterDetailPage.IsPresentedProperty, Presented);
		}

		void UpdateMaster()
		{
			if (_masterLayout != null && _masterLayout.ChildView != null)
				_masterLayout.ChildView.PropertyChanged -= HandleMasterPropertyChanged;
			_masterLayout.ChildView = _page.Master;
			if (_page.Master != null)
				_page.Master.PropertyChanged += HandleMasterPropertyChanged;
		}

		void UpdateSplitViewLayout()
		{
			if (Device.Idiom == TargetIdiom.Tablet)
			{
				bool isShowingSplit = MasterDetailPageController.ShouldShowSplitMode
					|| (MasterDetailPageController.ShouldShowSplitMode && _page.MasterBehavior != MasterBehavior.Default && _page.IsPresented);
				//SetLockMode(isShowingSplit ? LockModeLockedOpen : LockModeUnlocked);
				unchecked
				{
					//SetScrimColor(isShowingSplit ? Color.Transparent.ToAndroid() : (int)DefaultScrimColor);
				}
				//((Platform)_page.Platform).UpdateMasterDetailToggle();
			}
		}
	}

	class MasterDetailContainer : ViewGroup
	{
		const int DefaultMasterSize = 320;
		const int DefaultSmallMasterSize = 240;
		readonly bool _isMaster;
		readonly MasterDetailPage _parent;
		VisualElement _childView;

		public MasterDetailContainer(MasterDetailPage parent, bool isMaster, Context context) : base(context)
		{
			_parent = parent;
			_isMaster = isMaster;
		}

		public MasterDetailContainer(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer) { }

		IMasterDetailPageController MasterDetailPageController => _parent as IMasterDetailPageController;

		public VisualElement ChildView
		{
			get { return _childView; }
			set
			{
				if (_childView == value)
					return;

				RemoveAllViews();
				if (_childView != null)
					DisposeChildRenderers();

				_childView = value;

				if (_childView == null)
					return;

				AddChildView(_childView);
			}
		}

		protected virtual void AddChildView(VisualElement childView)
		{
			IVisualElementRenderer renderer = Platform.GetRenderer(childView);
			if (renderer == null)
				Platform.SetRenderer(childView, renderer = Platform.CreateRenderer(childView));

			if (renderer.ViewGroup.Parent != this)
			{
				if (renderer.ViewGroup.Parent != null)
					renderer.ViewGroup.RemoveFromParent();
				SetDefaultBackgroundColor(renderer);
				AddView(renderer.ViewGroup);
				renderer.UpdateLayout();
			}
		}

		public int TopPadding { get; set; }

		double DefaultWidthMaster
		{
			get
			{
				double w = Context.FromPixels(Resources.DisplayMetrics.WidthPixels);
				return w < DefaultSmallMasterSize ? w : (w < DefaultMasterSize ? DefaultSmallMasterSize : DefaultMasterSize);
			}
		}

		public override bool OnInterceptTouchEvent(MotionEvent ev)
		{
			bool isShowingPopover = _parent.IsPresented && !MasterDetailPageController.ShouldShowSplitMode;
			if (!_isMaster && isShowingPopover)
				return true;
			return base.OnInterceptTouchEvent(ev);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				RemoveAllViews();
				DisposeChildRenderers();
			}
			base.Dispose(disposing);
		}

		protected override void OnLayout(bool changed, int l, int t, int r, int b)
		{
			if (_childView == null)
				return;

			Rectangle bounds = GetBounds(_isMaster, l, t, r, b);
			if (_isMaster)
				MasterDetailPageController.MasterBounds = bounds;
			else
				MasterDetailPageController.DetailBounds = bounds;

			IVisualElementRenderer renderer = Platform.GetRenderer(_childView);
			renderer?.UpdateLayout();
		}

		void DisposeChildRenderers()
		{
			IVisualElementRenderer childRenderer = Platform.GetRenderer(_childView);
			if (childRenderer != null)
				childRenderer.Dispose();
			//_childView.ClearValue(Platform.RendererProperty);
		}

		Rectangle GetBounds(bool isMasterPage, int left, int top, int right, int bottom)
		{
			double width = Context.FromPixels(right - left);
			double height = Context.FromPixels(bottom - top);
			double xPos = 0;

			double padding = 0;
			return new Rectangle(xPos, padding, width, height - padding);
		}

		protected void SetDefaultBackgroundColor(IVisualElementRenderer renderer)
		{
			if (ChildView.BackgroundColor == Color.Default)
			{
				TypedArray colors = Context.Theme.ObtainStyledAttributes(new[] { global::Android.Resource.Attribute.ColorBackground });
				renderer.ViewGroup.SetBackgroundColor(new global::Android.Graphics.Color(colors.GetColor(0, 0)));
			}
		}
	}

}