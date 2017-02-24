// <author>Jean-Marie Alfonsi</author>
// <date>09/30/2016</date>
// <summary>
// Modification of the {@link android.support.design.widget.BottomSheetBehavior} with an Anchor state
//
// Ported to Xamarin.Android from google BottomSheetBehavior
// https://github.com/android/platform_frameworks_support/blob/master/design/src/android/support/design/widget/BottomSheetBehavior.java
// Version in synced with commit from 2016, May 12th: https://github.com/android/platform_frameworks_support/commit/362585b01e5ca19d1c58e4b152ad0a863b5f6d91
// Java style has been kept in private implementation to ease the sync with future commit from google.
//
// Anchor code adapted from: https://medium.com/@marxallski/from-bottomsheetbehavior-to-anchorsheetbehavior-262ad7997286
// </summary>


/*
 * Copyright (C) 2015 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Util;
using Android.Views;
using Java.Interop;

using Debug = System.Diagnostics.Debug;

namespace MusicPlayer.Forms.Droid
{
	/**
     *
     * An interaction behavior plugin for a child view of {@link CoordinatorLayout} to make it work as
     * a bottom sheet.
     *
     * Modification of the {@link android.support.design.widget.BottomSheetBehavior} with an Anchor state.
     */
	[Register("SeLoger.Droid.Controls.AnchorBottomSheetBehavior")]
	public partial class AnchorBottomSheetBehavior : CoordinatorLayout.Behavior
	{
		/**
         * Callback for monitoring events about bottom sheets.
         */
		public abstract class AnchorSheetCallback
		{

			/**
             * Called when the bottom sheet changes its state.
             *
             * @param bottomSheet The bottom sheet view.
             * @param newState    The new state. This will be one of {@link #STATE_DRAGGING},
             *                    {@link #STATE_SETTLING}, {@link #STATE_EXPANDED},
             *                    {@link #STATE_COLLAPSED}, or {@link #STATE_HIDDEN}.
             */
			public abstract void OnStateChanged(View bottomSheet, int newState);

			/**
             * Called when the bottom sheet is being dragged.
             *
             * @param bottomSheet The bottom sheet view.
             * @param slideOffset The new offset of this bottom sheet within its range, from 0 to 1
             *                    when it is moving upward, and from 0 to -1 when it moving downward.
             */
			public abstract void OnSlide(View bottomSheet, float slideOffset);
		}

		/**
         * The bottom sheet is dragging.
         */
		public const int STATE_DRAGGING = 1;

		/**
         * The bottom sheet is settling.
         */
		public const int STATE_SETTLING = 2;

		/**
         * The bottom sheet is expanded.
         */
		public const int STATE_EXPANDED = 3;

		/**
         * The bottom sheet is collapsed.
         */
		public const int STATE_COLLAPSED = 4;

		/**
         * The bottom sheet is hidden.
         */
		public const int STATE_HIDDEN = 5;

		/**
         * The bottom sheet is anchor.
         */
		public const int STATE_ANCHOR = 6;

		private const float HIDE_THRESHOLD = 0.5f;

		private const float HIDE_FRICTION = 0.1f;

		private const float ANCHOR_THRESHOLD = 0.25f;

		private float mAnchorThreshold = ANCHOR_THRESHOLD;

		private float mMaximumVelocity;

		private int mPeekHeight;

		private int mMinOffset;

		private int mMaxOffset;

		private int mAnchorOffset;

		private bool mHideable;

		private bool mSkipCollapsed;

		private int mState = STATE_COLLAPSED;

		private ViewDragHelper mViewDragHelper;

		private bool mIgnoreEvents;

		private int mLastNestedScrollDy;

		private bool mNestedScrolled;

		private int mParentHeight;

		private WeakReference<View> mViewRef;

		private WeakReference<View> mNestedScrollingChildRef;

		private AnchorSheetCallback mCallback;

		private VelocityTracker mVelocityTracker;

		private int mActivePointerId;

		private int mInitialY;

		private bool mTouchingScrollingChild;

		private readonly ViewDragHelper.Callback mDragCallback;


		/**
         * Default constructor for instantiating AnchorSheetBehavior.
         */
		public AnchorBottomSheetBehavior()
		{
			mDragCallback = new AnchorSheetDragCallback(this);
		}

		/**
         * Default constructor for inflating AnchorSheetBehavior from layout.
         *
         * @param context The {@link Context}.
         * @param attrs   The {@link AttributeSet}.
         */
		public AnchorBottomSheetBehavior(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.AnchorBottomSheetBehavior_Layout);

			setPeekHeight(
				a.GetDimensionPixelSize(Resource.Styleable.BottomSheetBehavior_Layout_behavior_peekHeight, 0));

			setHideable(
				a.GetBoolean(Resource.Styleable.BottomSheetBehavior_Layout_behavior_hideable, false));

			setSkipCollapsed(
				a.GetBoolean(
					Resource.Styleable.BottomSheetBehavior_Layout_behavior_skipCollapsed,
					false));

			a.Recycle();
			ViewConfiguration configuration = ViewConfiguration.Get(context);
			mMaximumVelocity = configuration.ScaledMaximumFlingVelocity;

			mDragCallback = new AnchorSheetDragCallback(this);
		}

		public override IParcelable OnSaveInstanceState(CoordinatorLayout parent, Java.Lang.Object child)
		{
			return new SavedState(base.OnSaveInstanceState(parent, child), mState);
		}

		public override void OnRestoreInstanceState(CoordinatorLayout parent, Java.Lang.Object child, IParcelable state)
		{
			var ss = (SavedState)state;
			base.OnRestoreInstanceState(parent, child, ss.SuperState);

			// Intermediate states are restored as collapsed state
			if (ss.State == STATE_DRAGGING || ss.State == STATE_SETTLING)
			{
				mState = STATE_COLLAPSED;
			}
			else
			{
				mState = ss.State;
			}
		}

		public override bool OnLayoutChild(
			CoordinatorLayout parent, Java.Lang.Object childObject, int layoutDirection)
		{
			Debug.WriteLineIf(DebugTrace, $"OnLayoutChild");
			View child = Android.Runtime.Extensions.JavaCast<View>(childObject);

			if (ViewCompat.GetFitsSystemWindows(parent) && !ViewCompat.GetFitsSystemWindows(child))
			{
				ViewCompat.SetFitsSystemWindows(child, true);
			}
			int savedTop = child.Top;
			// First let the parent lay it out
			parent.OnLayoutChild(child, layoutDirection);
			// Offset the bottom sheet
			mParentHeight = parent.Height;
			mMinOffset = Math.Max(0, mParentHeight - child.Height);
			mMaxOffset = Math.Max(mParentHeight - mPeekHeight, mMinOffset);
			mAnchorOffset = (int)Math.Max(mParentHeight * mAnchorThreshold, mMinOffset);

			Debug.WriteLineIf(DebugTrace, $"offset computed => savedTop:{savedTop} mMinOffset:{mMinOffset} mMaxOffset:{mMaxOffset} mAnchorOffset:{mAnchorOffset} ");
			if (mState == STATE_EXPANDED)
			{
				ViewCompat.OffsetTopAndBottom(child, mMinOffset);
			}
			else if (mState == STATE_ANCHOR)
			{
				ViewCompat.OffsetTopAndBottom(child, mAnchorOffset);
			}
			else if (mHideable && mState == STATE_HIDDEN)
			{
				ViewCompat.OffsetTopAndBottom(child, mParentHeight);
			}
			else if (mState == STATE_COLLAPSED)
			{
				ViewCompat.OffsetTopAndBottom(child, mMaxOffset);
			}
			else if (mState == STATE_DRAGGING || mState == STATE_SETTLING)
			{
				ViewCompat.OffsetTopAndBottom(child, savedTop - child.Top);
			}
			if (mViewDragHelper == null || mViewDragHelper.Handle == IntPtr.Zero)
			{
				mViewDragHelper = ViewDragHelper.Create(parent, mDragCallback);
			}
			mViewRef = new WeakReference<View>(child);
			mNestedScrollingChildRef = new WeakReference<View>(findScrollingChild(child));
			return true;
		}

		public override bool OnInterceptTouchEvent(
			CoordinatorLayout parent, Java.Lang.Object childObject, MotionEvent @event)
		{
			View child = Android.Runtime.Extensions.JavaCast<View>(childObject);

			if (!child.IsShown)
			{
				Debug.WriteLineIf(DebugTrace, $"OnInterceptTouchEvent: return false");
				return false;
			}

			int action = MotionEventCompat.GetActionMasked(@event);

			// Record the velocity
			if (action == (int)MotionEventActions.Down)
			{
				reset();
			}

			if (mVelocityTracker == null || mVelocityTracker.Handle == IntPtr.Zero)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}

			mVelocityTracker.AddMovement(@event);
			switch (action)
			{
				case (int)MotionEventActions.Up:
				case (int)MotionEventActions.Cancel:
					mTouchingScrollingChild = false;
					mActivePointerId = MotionEvent.InvalidPointerId;
					// Reset the ignore flag
					if (mIgnoreEvents)
					{
						mIgnoreEvents = false;
						return false;
					}
					break;

				case (int)MotionEventActions.Down:
					int initialX = (int)@event.GetX();
					mInitialY = (int)@event.GetY();
					View nestedScroll;

					if (mNestedScrollingChildRef.TryGetTarget(out nestedScroll) && parent.IsPointInChildBounds(nestedScroll, initialX, mInitialY))
					{
						mActivePointerId = @event.GetPointerId(@event.ActionIndex);
						//mTouchingScrollingChild = true;
					}
					mIgnoreEvents =
							mActivePointerId == MotionEvent.InvalidPointerId
														   && !parent.IsPointInChildBounds(child, initialX, mInitialY);
					break;
			}
			if (!mIgnoreEvents && mViewDragHelper.ShouldInterceptTouchEvent(@event))
			{
				Debug.WriteLineIf(DebugTrace, $"OnInterceptTouchEvent: return true");
				return true;
			}
			// We have to handle cases that the ViewDragHelper does not capture the bottom sheet because
			// it is not the top most view of its parent. This is not necessary when the touch event is
			// happening over the scrolling content as nested scrolling logic handles that case.
			View scroll;
			var result = action == (int)MotionEventActions.Move &&
					mNestedScrollingChildRef.TryGetTarget(out scroll) &&
					!mIgnoreEvents && mState != STATE_DRAGGING &&
					!parent.IsPointInChildBounds(scroll, (int)@event.GetX(), (int)@event.GetY()) &&
					Math.Abs(mInitialY - @event.GetY()) > mViewDragHelper.TouchSlop;

			Debug.WriteLineIf(DebugTrace, $"OnInterceptTouchEvent: return {result}");
			return result;
		}


		public override bool OnTouchEvent(CoordinatorLayout parent, Java.Lang.Object childObject, MotionEvent @event)
		{
			View child = Android.Runtime.Extensions.JavaCast<View>(childObject);

			if (!child.IsShown)
			{
				return false;
			}

			int action = MotionEventCompat.GetActionMasked(@event);
			if (mState == STATE_DRAGGING && action == (int)MotionEventActions.Down)
			{
				return true;
			}

			mViewDragHelper.ProcessTouchEvent(@event);
			// Record the velocity
			if (action == (int)MotionEventActions.Down)
			{
				reset();
			}

			if (mVelocityTracker == null || mVelocityTracker.Handle == IntPtr.Zero)
			{
				mVelocityTracker = VelocityTracker.Obtain();
			}

			mVelocityTracker.AddMovement(@event);

			// The ViewDragHelper tries to capture only the top-most View. We have to explicitly tell it
			// to capture the bottom sheet in case it is not captured and the touch slop is passed.
			if (action == (int)MotionEventActions.Move && !mIgnoreEvents)
			{
				if (Math.Abs(mInitialY - @event.GetY()) > mViewDragHelper.TouchSlop)
				{
					mViewDragHelper.CaptureChildView(child, @event.GetPointerId(@event.ActionIndex));
				}
			}

			return !mIgnoreEvents;
		}

		public override bool OnStartNestedScroll(
			CoordinatorLayout coordinatorLayout,
			Java.Lang.Object childObject,
			View directTargetChild, View target,
			int nestedScrollAxes)
		{
			mLastNestedScrollDy = 0;
			mNestedScrolled = false;
			var result = (nestedScrollAxes & ViewCompat.ScrollAxisVertical) != 0;
			Debug.WriteLineIf(DebugTrace, $"OnStartNestedScroll: return {result}");
			return result;
		}

		public override void OnNestedPreScroll(
				CoordinatorLayout coordinatorLayout,
				Java.Lang.Object childObject,
				View target,
				int dx,
				int dy,
				int[] consumed)
		{
			Debug.WriteLineIf(DebugTrace, $"OnNestedPreScroll");
			View child = Android.Runtime.Extensions.JavaCast<View>(childObject);

			View scrollingChild;
			mNestedScrollingChildRef.TryGetTarget(out scrollingChild);
			if (target != scrollingChild)
			{
				return;
			}

			int currentTop = child.Top;
			int newTop = currentTop - dy;
			Debug.WriteLineIf(DebugTrace, $"currentTop:{currentTop} newTop:{newTop}");
			if (dy > 0)
			{ // Upward
				Debug.WriteLineIf(DebugTrace, $"dy > 0: Upward");
				if (newTop < mMinOffset)
				{
					Debug.WriteLineIf(DebugTrace, $"newTop < mMinOffset: STATE_EXPANDED");
					consumed[1] = currentTop - mMinOffset;
					ViewCompat.OffsetTopAndBottom(child, -consumed[1]);
					setStateInternal(STATE_EXPANDED);
				}
				else
				{
					Debug.WriteLineIf(DebugTrace, $"else: STATE_DRAGGING");
					consumed[1] = dy;
					ViewCompat.OffsetTopAndBottom(child, -dy);
					setStateInternal(STATE_DRAGGING);
				}
			}
			else if (dy < 0)
			{
				// Downward
				Debug.WriteLineIf(DebugTrace, $"dy < 0: Downward");
				if (!ViewCompat.CanScrollVertically(target, -1))
				{
					if (newTop <= mMaxOffset || mHideable)
					{
						Debug.WriteLineIf(DebugTrace, $"newTop <= mMaxOffset || mHideable: STATE_DRAGGING");
						consumed[1] = dy;
						ViewCompat.OffsetTopAndBottom(child, -dy);
						setStateInternal(STATE_DRAGGING);
					}
					else
					{
						Debug.WriteLineIf(DebugTrace, $"else: STATE_COLLAPSED");
						consumed[1] = currentTop - mMaxOffset;
						ViewCompat.OffsetTopAndBottom(child, -consumed[1]);
						setStateInternal(STATE_COLLAPSED);
					}
				}
			}

			dispatchOnSlide(child.Top);
			mLastNestedScrollDy = dy;
			mNestedScrolled = true;
		}

		public void OnStopNestedScrollOriginal(
			CoordinatorLayout coordinatorLayout, Java.Lang.Object childObject, View target)
		{
			View child = Android.Runtime.Extensions.JavaCast<View>(childObject);

			if (child.Top == mMinOffset)
			{
				setStateInternal(STATE_EXPANDED);
				return;
			}
			View nestedScrollingChild;
			mNestedScrollingChildRef.TryGetTarget(out nestedScrollingChild);
			if (target != nestedScrollingChild || !mNestedScrolled)
			{
				return;
			}
			int top;
			int targetState;
			if (mLastNestedScrollDy > 0)
			{
				top = mMinOffset;
				targetState = STATE_EXPANDED;
			}
			else if (mHideable && shouldHide(child, getYVelocity()))
			{
				top = mParentHeight;
				targetState = STATE_HIDDEN;
			}
			else if (mLastNestedScrollDy == 0)
			{
				int currentTop = child.Top;
				if (Math.Abs(currentTop - mMinOffset) < Math.Abs(currentTop - mMaxOffset))
				{
					top = mMinOffset;
					targetState = STATE_EXPANDED;
				}
				else
				{
					top = mMaxOffset;
					targetState = STATE_COLLAPSED;
				}
			}
			else
			{
				top = mMaxOffset;
				targetState = STATE_COLLAPSED;
			}
			if (mViewDragHelper.SmoothSlideViewTo(child, child.Left, top))
			{
				setStateInternal(STATE_SETTLING);
				ViewCompat.PostOnAnimation(child, this.CreateSettleRunnable(child, targetState));
			}
			else
			{
				setStateInternal(targetState);
			}
			mNestedScrolled = false;
		}

		public override void OnStopNestedScroll(
			CoordinatorLayout coordinatorLayout, Java.Lang.Object childObject, View target)
		{
			Debug.WriteLineIf(DebugTrace, "OnStopNestedScroll");

			View child = Android.Runtime.Extensions.JavaCast<View>(childObject);

			if (child.Top == mMinOffset)
			{
				Debug.WriteLineIf(DebugTrace, "top == minOffset => EXPANDED");
				setStateInternal(STATE_EXPANDED);
				return;
			}

			View nestedScrollingChild;
			mNestedScrollingChildRef.TryGetTarget(out nestedScrollingChild);
			if (target != nestedScrollingChild || !mNestedScrolled)
			{
				return;
			}

			int top;
			int targetState;
			if (mHideable && shouldHide(child, getYVelocity()))
			{
				Debug.WriteLineIf(DebugTrace, "hideable && shouldHide => HIDDEN");
				top = mParentHeight;
				targetState = STATE_HIDDEN;
			}
			else if (mLastNestedScrollDy >= 0)
			{
				// It went Up
				int currentTop = child.Top;
				Debug.WriteLineIf(DebugTrace,
					$"mLastNestedScrollDy >= 0: currentTop:{currentTop} mAnchorOffset:{mAnchorOffset} mMinOffset:{mMinOffset} mMaxOffset:{mMaxOffset}");

				if (Math.Abs(currentTop - mAnchorOffset) < Math.Abs(currentTop - mMinOffset))
				{
					Debug.WriteLineIf(DebugTrace, "top close to anchor => ANCHOR");
					top = mAnchorOffset;
					targetState = STATE_ANCHOR;
				}
				else if (Math.Abs(currentTop - mMinOffset) < Math.Abs(currentTop - mMaxOffset))
				{
					Debug.WriteLineIf(DebugTrace, "top close child height => EXPANDED");
					top = mMinOffset;
					targetState = STATE_EXPANDED;
				}
				else
				{
					Debug.WriteLineIf(DebugTrace, "else => COLLAPSED");
					top = mMaxOffset;
					targetState = STATE_COLLAPSED;
				}
			}
			else
			{
				// It went down
				Debug.WriteLineIf(DebugTrace, $"global else");
				int currentTop = child.Top;
				Debug.WriteLineIf(DebugTrace, $"mLastNestedScrollDy >= 0: currentTop:{currentTop} mAnchorOffset:{mAnchorOffset} mMinOffset:{mMinOffset} mMaxOffset:{mMaxOffset}");

				if (Math.Abs(currentTop - mAnchorOffset) < Math.Abs(currentTop - mMaxOffset))
				{
					Debug.WriteLineIf(DebugTrace, "top close to anchor => ANCHOR");
					top = mAnchorOffset;
					targetState = STATE_ANCHOR;
				}
				else
				{
					Debug.WriteLineIf(DebugTrace, $"else => COLLAPSED");
					top = mMaxOffset;
					targetState = STATE_COLLAPSED;
				}
			}

			if (mViewDragHelper.SmoothSlideViewTo(child, child.Left, top))
			{
				setStateInternal(STATE_SETTLING);
				ViewCompat.PostOnAnimation(child, this.CreateSettleRunnable(child, targetState));
			}
			else
			{
				setStateInternal(targetState);
			}

			mNestedScrolled = false;
		}

		public override bool OnNestedPreFling(
				CoordinatorLayout coordinatorLayout,
				Java.Lang.Object childObject,
				View target,
				float velocityX,
				float velocityY)
		{
			View nestedScrollingChild;
			mNestedScrollingChildRef.TryGetTarget(out nestedScrollingChild);
			var result = target == nestedScrollingChild && (mState != STATE_EXPANDED || base.OnNestedPreFling(
					coordinatorLayout, childObject, target, velocityX, velocityY));

			Debug.WriteLineIf(DebugTrace, $"OnNestedPreFling: return {result}");
			return result;
		}

		public override bool OnNestedFling(
			CoordinatorLayout coordinatorLayout,
			Java.Lang.Object child,
			View target,
			float velocityX,
			float velocityY,
			bool consumed)
		{
			return base.OnNestedFling(coordinatorLayout, child, target, velocityX, velocityY, consumed);
		}

		/**
         * Sets the height of the bottom sheet when it is collapsed.
         *
         * @param peekHeight The height of the collapsed bottom sheet in pixels.
         * @attr ref android.support.design.R.styleable#AnchorBehavior_Params_behavior_peekHeight
         */
		public void setPeekHeight(int peekHeight)
		{
			mPeekHeight = Math.Max(0, peekHeight);
			mMaxOffset = mParentHeight - peekHeight;
		}

		/**
         * Gets the height of the bottom sheet when it is collapsed.
         *
         * @return The height of the collapsed bottom sheet.
         * @attr ref android.support.design.R.styleable#AnchorBehavior_Params_behavior_peekHeight
         */
		public int getPeekHeight()
		{
			return mPeekHeight;
		}

		/**
         * Get the size in pixels from the anchor state to the top of the parent (Expanded state)
         *
         * @return pixel size of the anchor state
         */
		public int getAnchorOffset()
		{
			return mAnchorOffset;
		}

		/**
         * The multiplier between 0..1 to calculate the Anchor offset
         *
         * @return float between 0..1
         */
		public float getAnchorThreshold()
		{
			return mAnchorThreshold;
		}

		/**
         * Set the offset for the anchor state. Number between 0..1
         * i.e: Anchor the panel at 1/3 of the screen: setAnchorOffset(0.25)
         *
         * @param threshold {@link Float} from 0..1
         */
		public void setAnchorOffset(float threshold)
		{
			this.mAnchorThreshold = threshold;
			this.mAnchorOffset = (int)Math.Max(mParentHeight * mAnchorThreshold, mMinOffset);
		}

		/**
         * Sets whether this bottom sheet can hide when it is swiped down.
         *
         * @param hideable {@code true} to make this bottom sheet hideable.
         * @attr ref android.support.design.R.styleable#AnchorBehavior_Params_behavior_hideable
         */
		public void setHideable(bool hideable)
		{
			mHideable = hideable;
		}

		/**
         * Gets whether this bottom sheet can hide when it is swiped down.
         *
         * @return {@code true} if this bottom sheet can hide.
         * @attr ref android.support.design.R.styleable#AnchorBehavior_Params_behavior_hideable
         */
		public bool isHideable()
		{
			return mHideable;
		}

		/**
         * Sets whether this bottom sheet should skip the collapsed state when it is being hidden
         * after it is expanded once. Setting this to true has no effect unless the sheet is hideable.
         *
         * @param skipCollapsed True if the bottom sheet should skip the collapsed state.
         * @attr ref android.support.design.R.styleable#BottomSheetBehavior_Layout_behavior_skipCollapsed
         */
		public void setSkipCollapsed(bool skipCollapsed)
		{
			mSkipCollapsed = skipCollapsed;
		}

		/**
         * Sets whether this bottom sheet should skip the collapsed state when it is being hidden
         * after it is expanded once.
         *
         * @return Whether the bottom sheet should skip the collapsed state.
         * @attr ref android.support.design.R.styleable#BottomSheetBehavior_Layout_behavior_skipCollapsed
         */
		public bool getSkipCollapsed()
		{
			return mSkipCollapsed;
		}

		/**
         * Sets a callback to be notified of bottom sheet events.
         *
         * @param callback The callback to notify when bottom sheet events occur.
         */
		public void SetAnchorSheetCallback(AnchorSheetCallback callback)
		{
			mCallback = callback;
		}

		/**
         * Sets the state of the bottom sheet. The bottom sheet will transition to that state with
         * animation.
         *
         * @param state One of {@link #STATE_COLLAPSED}, {@link #STATE_EXPANDED}, or
         *              {@link #STATE_HIDDEN}.
         */
		public void setState(int state)
		{
			Debug.WriteLineIf(DebugTrace, $"setState {(AnchorBottomSheetState)state}");
			if (state == mState)
			{
				return;
			}

			if (mViewRef == null)
			{
				// The view is not laid out yet; modify mState and let onLayoutChild handle it later
				if (state == STATE_COLLAPSED || state == STATE_EXPANDED || state == STATE_ANCHOR ||
						(mHideable && state == STATE_HIDDEN))
				{
					mState = state;
				}
				return;
			}

			View child;
			if (!mViewRef.TryGetTarget(out child))
			{
				return;
			}

			int top;
			if (state == STATE_COLLAPSED)
			{
				top = mMaxOffset;
				View scroll;
				if (mNestedScrollingChildRef.TryGetTarget(out scroll) && ViewCompat.CanScrollVertically(scroll, -1))
				{
					scroll.ScrollTo(0, 0);
				}
			}
			else if (state == STATE_EXPANDED)
			{
				top = mMinOffset;
			}
			else if (state == STATE_ANCHOR)
			{
				top = mAnchorOffset;
			}
			else if (mHideable && state == STATE_HIDDEN)
			{
				top = mParentHeight;
			}
			else
			{
				throw new ArgumentException("Illegal state argument: " + state, nameof(state));
			}
			setStateInternal(STATE_SETTLING);
			if (mViewDragHelper.SmoothSlideViewTo(child, child.Left, top))
			{
				ViewCompat.PostOnAnimation(child, this.CreateSettleRunnable(child, state));
			}
		}

		/**
         * Gets the current state of the bottom sheet.
         *
         * @return One of {@link #STATE_EXPANDED}, {@link #STATE_COLLAPSED}, {@link #STATE_DRAGGING},
         * and {@link #STATE_SETTLING}.
         */
		// @State
		public int getState()
		{
			return mState;
		}

		private void setStateInternal(int state)
		{
			Debug.WriteLineIf(DebugTrace, $"setStateInternal {(AnchorBottomSheetState)state}");
			if (mState == state)
			{
				return;
			}

			mState = state;
			View bottomSheet;
			if (mViewRef.TryGetTarget(out bottomSheet) && mCallback != null)
			{
				mCallback.OnStateChanged(bottomSheet, state);
			}
		}

		private void reset()
		{
			mActivePointerId = ViewDragHelper.InvalidPointer;
			if (mVelocityTracker != null)
			{
				mVelocityTracker.Recycle();
				mVelocityTracker = null;
			}
		}

		private bool shouldHide(View child, float yvel)
		{
			if (mSkipCollapsed)
			{
				return true;
			}

			if (child.Top < mMaxOffset)
			{
				// It should not hide, but collapse.
				return false;
			}
			float newTop = child.Top + yvel * HIDE_FRICTION;
			return Math.Abs(newTop - mMaxOffset) / (float)mPeekHeight > HIDE_THRESHOLD;
		}

		private View findScrollingChild(View view)
		{
			if (view is INestedScrollingChild)
			{
				return view;
			}
			if (view is ViewGroup)
			{
				ViewGroup group = (ViewGroup)view;
				for (int i = 0, count = group.ChildCount; i < count; i++)
				{
					View scrollingChild = findScrollingChild(group.GetChildAt(i));
					if (scrollingChild != null)
					{
						return scrollingChild;
					}
				}
			}
			return null;
		}

		private float getYVelocity()
		{
			mVelocityTracker.ComputeCurrentVelocity(1000, mMaximumVelocity);
			return VelocityTrackerCompat.GetYVelocity(mVelocityTracker, mActivePointerId);
		}

		private void dispatchOnSlide(int top)
		{
			View bottomSheet;
			if (mViewRef.TryGetTarget(out bottomSheet) && mCallback != null)
			{
				if (top > mMaxOffset)
				{
					mCallback.OnSlide(bottomSheet, (float)(mMaxOffset - top) / mPeekHeight);
				}
				else
				{
					mCallback.OnSlide(bottomSheet, (float)(mMaxOffset - top) / ((mMaxOffset - mMinOffset)));
				}
			}
		}

		private Java.Lang.IRunnable CreateSettleRunnable(View view, int targetState)
		{
			Java.Lang.IRunnable settleRunnable = null;
			settleRunnable = new Java.Lang.Runnable(() =>
			{
				if (mViewDragHelper != null && mViewDragHelper.ContinueSettling(true))
				{
					ViewCompat.PostOnAnimation(view, settleRunnable);
				}
				else
				{
					setStateInternal(targetState);
				}
			});

			return settleRunnable;
		}


		public class SavedState : View.BaseSavedState
		{
			public SavedState(IntPtr javaReference, JniHandleOwnership transfer)
				: base(javaReference, transfer)
			{ }

			public SavedState(Parcel source)
				: base(source)
			{
				//noinspection ResourceType
				State = source.ReadInt();
			}

			public SavedState(IParcelable superState, int state)
				: base(superState)
			{
				this.State = state;
			}

			public int State { get; }

			public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
			{
				base.WriteToParcel(dest, flags);
				dest.WriteInt(State);
			}

			[ExportField("CREATOR")]
			public static StateSavedStateCreator InitializeCreator()
			{
				return new StateSavedStateCreator();
			}

			public class StateSavedStateCreator : Java.Lang.Object, IParcelableCreator
			{
				public Java.Lang.Object CreateFromParcel(Parcel source)
				{
					return new SavedState(source);
				}

				public Java.Lang.Object[] NewArray(int size)
				{
					return new SavedState[size];
				}
			}
		}

		/**
         * A utility function to get the {@link AnchorSheetBehavior} associated with the {@code view}.
         *
         * @param view The {@link View} with {@link AnchorSheetBehavior}.
         * @return The {@link AnchorSheetBehavior} associated with the {@code view}.
         */
		public static AnchorBottomSheetBehavior From<TView>(TView view) where TView : View
		{
			var @params = view.LayoutParameters;
			if (!(@params is CoordinatorLayout.LayoutParams))
			{
				throw new ArgumentException("The view is not a child of CoordinatorLayout", nameof(view));
			}
			CoordinatorLayout.Behavior behavior = ((CoordinatorLayout.LayoutParams)@params).Behavior;
			if (!(behavior is AnchorBottomSheetBehavior))
			{
				throw new ArgumentException("The view is not associated with AnchorSheetBehavior", nameof(view));
			}
			return (AnchorBottomSheetBehavior)behavior;
		}

		static class MathUtils
		{

			public static int constrain(int amount, int low, int high)
			{
				return amount < low ? low : (amount > high ? high : amount);
			}

			public static float constrain(float amount, float low, float high)
			{
				return amount < low ? low : (amount > high ? high : amount);
			}
		}
	}
}