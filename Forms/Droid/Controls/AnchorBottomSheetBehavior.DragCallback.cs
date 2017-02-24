using System;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;

using Debug = System.Diagnostics.Debug;

namespace MusicPlayer.Forms.Droid
{
	public enum AnchorBottomSheetState
	{
		Unknown = 0,
		Dragging = 1,
		Settling = 2,
		Expanded = 3,
		Collapsed = 4,
		Hidden = 5,
		Anchored = 6
	}

	public partial class AnchorBottomSheetBehavior
	{
		private const bool DebugTrace = false;

		public int PeekHeight
		{
			get { return this.getPeekHeight(); }
			set { this.setPeekHeight(value); }
		}

		public float AnchorThreshold
		{
			get { return mAnchorThreshold; }
			set { this.SetAnchorThreshold(value); }
		}

		public float AnchorOffset
		{
			get { return mAnchorOffset; }
		}

		public float PeekOffset
		{
			get { return mMaxOffset; }
		}

		public AnchorBottomSheetState State
		{
			get { return (AnchorBottomSheetState)this.getState(); }
			set { this.setState((int)value); }
		}

		public bool Hideable
		{
			get { return this.isHideable(); }
			set { this.setHideable(value); }
		}

		public bool SkipCollapsed
		{
			get { return this.getSkipCollapsed(); }
			set { this.setSkipCollapsed(value); }
		}

		private void SetAnchorThreshold(float value)
		{
			if (value > 1.0 || value < 0)
				throw new ArgumentException("threshold value should be between 0 and 1");

			mAnchorThreshold = value;
		}

		private class AnchorSheetDragCallback : ViewDragHelper.Callback
		{
			private readonly AnchorBottomSheetBehavior mBehavior;

			public AnchorSheetDragCallback(AnchorBottomSheetBehavior behavior)
			{
				mBehavior = behavior;
			}

			public override bool TryCaptureView(View child, int pointerId)
			{
				if (mBehavior.mState == STATE_DRAGGING)
				{
					return false;
				}
				if (mBehavior.mTouchingScrollingChild)
				{
					return false;
				}
				if (mBehavior.mState == STATE_EXPANDED && mBehavior.mActivePointerId == pointerId)
				{
					View scroll;
					if (mBehavior.mNestedScrollingChildRef.TryGetTarget(out scroll)
						&& ViewCompat.CanScrollVertically(scroll, -1))
					{
						// Let the content scroll up
						return false;
					}
				}

				View currentChild;
				return mBehavior.mViewRef != null
					&& mBehavior.mViewRef.TryGetTarget(out currentChild)
					&& currentChild == child;
			}

			public override void OnViewPositionChanged(View changedView, int left, int top, int dx, int dy)
			{
				mBehavior.dispatchOnSlide(top);
			}

			public override void OnViewDragStateChanged(int state)
			{
				if (state == ViewDragHelper.StateDragging)
				{
					mBehavior.setStateInternal(STATE_DRAGGING);
				}
			}

			public void OnViewReleasedOriginal(View releasedChild, float xvel, float yvel)
			{
				Debug.WriteLineIf(DebugTrace, $"OnViewReleased => xvel:{xvel} yvel:{yvel}");
				int top;
				int targetState;
				if (yvel < 0)
				{ // Moving up
					Debug.WriteLineIf(DebugTrace, "Moving up: EXPANDED");
					top = mBehavior.mMinOffset;
					targetState = STATE_EXPANDED;
				}
				else if (mBehavior.mHideable && mBehavior.shouldHide(releasedChild, yvel))
				{
					Debug.WriteLineIf(DebugTrace, "Hideable and should hide: HIDDEN");
					top = mBehavior.mParentHeight;
					targetState = STATE_HIDDEN;
				}
				else if (yvel == 0f)
				{
					int currentTop = releasedChild.Top;
					if (Math.Abs(currentTop - mBehavior.mMinOffset) < Math.Abs(currentTop - mBehavior.mMaxOffset))
					{
						Debug.WriteLineIf(DebugTrace, "Near top: EXPANDED");
						top = mBehavior.mMinOffset;
						targetState = STATE_EXPANDED;
					}
					else
					{
						Debug.WriteLineIf(DebugTrace, "Else near top: COLLAPSED");
						top = mBehavior.mMaxOffset;
						targetState = STATE_COLLAPSED;
					}
				}
				else
				{
					Debug.WriteLineIf(DebugTrace, "Else: COLLAPSED");
					top = mBehavior.mMaxOffset;
					targetState = STATE_COLLAPSED;
				}
				if (mBehavior.mViewDragHelper.SettleCapturedViewAt(releasedChild.Left, top))
				{
					mBehavior.setStateInternal(STATE_SETTLING);
					ViewCompat.PostOnAnimation(releasedChild,
											   mBehavior.CreateSettleRunnable(releasedChild, targetState));
				}
				else
				{
					mBehavior.setStateInternal(targetState);
				}
			}

			public override void OnViewReleased(View releasedChild, float xvel, float yvel)
			{
				Debug.WriteLineIf(DebugTrace, $"OnViewReleased => xvel:{xvel} yvel:{yvel}");
				int top;
				int targetState;
				if (mBehavior.mHideable && mBehavior.shouldHide(releasedChild, yvel))
				{
					Debug.WriteLineIf(DebugTrace, "Hideable and should hide: HIDDEN");
					top = mBehavior.mParentHeight;
					targetState = STATE_HIDDEN;
				}
				else if (yvel <= 0f)
				{
					int currentTop = releasedChild.Top;
					Debug.WriteLineIf(DebugTrace, $"yvel <= 0f: currentTop:{currentTop} mAnchorOffset:{mBehavior.mAnchorOffset} mMinOffset:{mBehavior.mMinOffset} mMaxOffset:{mBehavior.mMaxOffset}");
					if (Math.Abs(currentTop - mBehavior.mAnchorOffset) < Math.Abs(currentTop - mBehavior.mMinOffset))
					{
						Debug.WriteLineIf(DebugTrace, "top close to anchor => ANCHOR");
						top = mBehavior.mAnchorOffset;
						targetState = STATE_ANCHOR;
					}
					else if (Math.Abs(currentTop - mBehavior.mMinOffset) < Math.Abs(currentTop - mBehavior.mMaxOffset))
					{
						Debug.WriteLineIf(DebugTrace, "top close child height => EXPANDED");
						top = mBehavior.mMinOffset;
						targetState = STATE_EXPANDED;
					}
					else
					{
						Debug.WriteLineIf(DebugTrace, "else => COLLAPSED");
						top = mBehavior.mMaxOffset;
						targetState = STATE_COLLAPSED;
					}
				}
				else
				{
					Debug.WriteLineIf(DebugTrace, $"global else");
					int currentTop = releasedChild.Top;
					Debug.WriteLineIf(DebugTrace, $"yvel <= 0f: currentTop:{currentTop} mAnchorOffset:{mBehavior.mAnchorOffset} mMinOffset:{mBehavior.mMinOffset} mMaxOffset:{mBehavior.mMaxOffset}");
					if (Math.Abs(currentTop - mBehavior.mAnchorOffset) < Math.Abs(currentTop - mBehavior.mMaxOffset))
					{
						Debug.WriteLineIf(DebugTrace, "top close to anchor => ANCHOR");
						top = mBehavior.mAnchorOffset;
						targetState = STATE_ANCHOR;
					}
					else
					{
						Debug.WriteLineIf(DebugTrace, $"else => COLLAPSED");
						top = mBehavior.mMaxOffset;
						targetState = STATE_COLLAPSED;
					}
				}
				if (mBehavior.mViewDragHelper.SettleCapturedViewAt(releasedChild.Left, top))
				{
					mBehavior.setStateInternal(STATE_SETTLING);
					ViewCompat.PostOnAnimation(
						releasedChild, mBehavior.CreateSettleRunnable(releasedChild, targetState));
				}
				else
				{
					mBehavior.setStateInternal(targetState);
				}
			}


			public override int ClampViewPositionVertical(View child, int top, int dy)
			{
				return MathUtils.constrain(
					top,
					mBehavior.mMinOffset,
					mBehavior.mHideable ? mBehavior.mParentHeight : mBehavior.mMaxOffset);
			}

			public override int ClampViewPositionHorizontal(View child, int left, int dx)
			{
				return child.Left;
			}

			public override int GetViewVerticalDragRange(View child)
			{
				if (mBehavior.mHideable)
				{
					return mBehavior.mParentHeight - mBehavior.mMinOffset;
				}
				else
				{
					return mBehavior.mMaxOffset - mBehavior.mMinOffset;
				}
			}
		}
	}
}