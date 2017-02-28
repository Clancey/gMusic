using MusicPlayer.Models;
using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Data;
using SimpleDatabase;
using MusicPlayer;

namespace MusicPlayer.ViewModels
{
	public  partial class BaseViewModel : BaseModel
	{
		public virtual void OnViewAppearing()
		{

		}
		public virtual void OnViewDissapearing()
		{

		}
	}
	public abstract partial class BaseViewModel<T> : BaseViewModel where T : new()
	{

		SimpleDatabaseSource<T> items = new SimpleDatabaseSource<T>(Database.Main);
		public SimpleDatabaseSource<T> Items
		{
			get { return items; }
			set { ProcPropertyChanged(ref items, value); }
		}

		bool isLoading;
		public bool IsLoading { 
			get { return isLoading;}
			set { ProcPropertyChanged(ref isLoading, value);}
		}

		public bool IsGrouped { 
			get { return items.IsGrouped; } 
			set {
				items.IsGrouped = value;
				ProcPropertyChanged(nameof(IsGrouped));
			}
		}

		GroupInfo groupInfo;

		public GroupInfo GroupInfo
		{
			get { return items.GroupInfo; }
			set { items.GroupInfo = value; }
		}

		public GroupInfo CurrentGroupInfo => items.CurrentGroupInfo;

		public abstract GroupInfo OfflineGroupInfo { get; set; }

		public virtual bool IsSearching { get; set; }

		public GroupInfo SearchResults = new GroupInfo();



		public void OnRowSelected(object obj)
		{
			RowSelected((T)obj);
		}
		public virtual void RowSelected(T item)
		{

		}
	}
}