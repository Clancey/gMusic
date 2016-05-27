using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using AppKit;

namespace MusicPlayer
{
	public partial class AlbumDetailViewController : AppKit.NSViewController
	{
		#region Constructors

		// Called when created from unmanaged code
		public AlbumDetailViewController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}

		// Called when created directly from a XIB file
		[Export ("initWithCoder:")]
		public AlbumDetailViewController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}

		// Call to load from the XIB/NIB file
		public AlbumDetailViewController () : base ("AlbumDetailView", NSBundle.MainBundle)
		{
			Initialize ();
		}

		// Shared initialization code
		void Initialize ()
		{
		}

		#endregion

		//strongly typed view accessor
		public new AlbumDetailView View {
			get {
				return (AlbumDetailView)base.View;
			}
		}
	}
}
