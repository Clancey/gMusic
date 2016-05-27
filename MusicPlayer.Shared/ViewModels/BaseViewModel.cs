using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using SimpleDatabase;
using SimpleTables;
using MusicPlayer;

namespace MusicPlayer.ViewModels
{
	internal partial class BaseViewModel : BaseModel
	{
		string title;

		public string Title
		{
			get { return title; }
			set { ProcPropertyChanged(ref title, value); }
		}
	}

	public abstract partial class BaseViewModel<T> : TableViewModel<T> where T : new()
	{
		public string Title { get; set; }

		GroupInfo groupInfo;

		public GroupInfo GroupInfo
		{
			get { return groupInfo ?? (groupInfo = Database.Main.GetGroupInfo<T>()); }
			set
			{
				groupInfo = value;
				ReloadData();
			}
		}

		public GroupInfo CurrentGroupInfo
		{
			get
			{
				return
					
					Settings.ShowOfflineOnly ? OfflineGroupInfo :
					GroupInfo;
			}
		}

		public abstract GroupInfo OfflineGroupInfo { get; set; }

#if __IOS__
		public BaseViewModel()
		{
		}

#elif Droid
		public BaseViewModel (Android.Content.Context context, Android.Widget.ListView list) : base (context, list)
		{
		}
#endif

		public virtual bool IsSearching { get; set; }

		public GroupInfo SearchResults = new GroupInfo();


		public void PrecachData()
		{
			//Database.Main.Precache<T> ();
		}

		#region implemented abstract members of TableViewModel

		public override int RowsInSection(int section)
		{
			if (Database.Main == null)
				return 0;
			int rows = Database.Main.RowsInSection<T>(CurrentGroupInfo, section);
			return rows;
		}

		public override int NumberOfSections()
		{
			if (Database.Main == null)
				return 0;
			int sections = Database.Main.NumberOfSections<T>(CurrentGroupInfo);
			return sections;
		}

		public override int GetItemViewType(int section, int row)
		{
			throw new NotImplementedException();
		}


		public override string HeaderForSection(int section)
		{
			if (Database.Main == null)
				return "";
			return Database.Main.SectionHeader<T>(CurrentGroupInfo, section);
		}

		public override string[] SectionIndexTitles()
		{
			if (Database.Main == null)
				return null;
			return Database.Main.QuickJump<T>(CurrentGroupInfo);
		}

		public virtual string[] SectionIndexTitlesArrows()
		{
			return new[]
			{
				"\u25B2",
				"\u25CF",
				"\u25CF",
				"\u25CF",
				"\u25BC",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
				" ",
			};
		}

		public override T ItemFor(int section, int row)
		{
			return Database.Main.ObjectForRow<T>(CurrentGroupInfo, section, row);
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
		}
	}
}