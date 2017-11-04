using System;
using UIKit;
using CoreGraphics;
namespace MusicPlayer.iOS.UI
{
	public class DragDismissAnimator : UIViewControllerAnimatedTransitioning
	{

		public override void AnimateTransition(IUIViewControllerContextTransitioning transitionContext)
		{
			var fromVC = transitionContext?.GetViewControllerForKey(UITransitionContext.FromViewControllerKey);
			var toVC = transitionContext?.GetViewControllerForKey(UITransitionContext.ToViewControllerKey);
			var containerView = transitionContext?.ContainerView;
			containerView.InsertSubviewBelow(toVC.View, fromVC.View);

			containerView.InsertSubviewBelow(toVC.View, fromVC.View);

			var screenBounds = UIScreen.MainScreen.Bounds;
			screenBounds.Y = screenBounds.Height;
			UIView.Animate(TransitionDuration(transitionContext), () =>
			 {
				 fromVC.View.Frame = screenBounds;
			 }, () =>
			 {
				 transitionContext.CompleteTransition(!transitionContext.TransitionWasCancelled);
			 });
		}

		public override double TransitionDuration(IUIViewControllerContextTransitioning transitionContext) => .6;
	}

	public class DragDismissInteractor : UIPercentDrivenInteractiveTransition
	{
		public bool HasStarted { get; set; }
		public bool ShouldFinish { get; set; }
	}
}
