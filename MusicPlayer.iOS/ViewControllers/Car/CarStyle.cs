using System;
using UIKit;
using MusicPlayer.Managers;
namespace MusicPlayer.iOS
{
	public class CarStyle : DarkStyle
	{
		public static double NavIconImagePercent = .06;
		static nfloat rowHeight;

		public static nfloat RowHeight
		{
			get { return rowHeight; }

			set
			{
				if (rowHeight == value)
					return;
				rowHeight = value;
				RowHeightChanged?.Invoke();
				//NotificationManager.Shared.ProcSongDatabaseUpdated();
				//NotificationManager.Shared.ProcRadioDatabaseUpdated();
				//NotificationManager.Shared.ProcPlaylistDatabaseUpdated();
			}
		}
		static Action RowHeightChanged;

		public CarStyle()
		{
			Id = "Car";
			this.BackgroundColor = UIColor.FromRGB(39, 40, 34);
			this.SubTextColor = UIColor.LightGray;
			this.MainTextColor = UIColor.White;
			RowHeightChanged = ComputeStyles;
		}

		void ComputeStyles()
		{
			var mainTextSize = RowHeight * .34f;
			MainTextFont = Fonts.NormalFont(mainTextSize);
			SubTextFont = Fonts.NormalFont(RowHeight * (12f / 44f));
		}

	}
}

