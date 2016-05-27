using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace MusicPlayer
{
	public partial class AlbumDetailsViewControllerController : AppKit.NSViewController
	{
		#region Constructors

		// Called when created from unmanaged code
		public AlbumDetailsViewControllerController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public AlbumDetailsViewControllerController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Call to load from the XIB/NIB file
		public AlbumDetailsViewControllerController () : base ("AlbumDetailsViewController", NSBundle.MainBundle)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed view accessor
		public new AlbumDetailsViewController View {
			get {
				return (AlbumDetailsViewController)base.View;
			}
		}
	}
}
