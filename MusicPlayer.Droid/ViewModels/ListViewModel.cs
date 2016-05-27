using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MusicPlayer.ViewModels;
using SimpleDatabase;

namespace MusicPlayer
{
	public partial class ListViewModel<T> : BaseViewModel<T> where T : new()
	{
		List<T> items = new List<T>(); 
		public List<T> Items
		{
			get { return items; }
			set { 
				items = value;
				this.ReloadData();
			}
		}

		public ListViewModel() 
		{
			
		}
		public ListViewModel (List<T> items ) 
		{
			this.items = items;
		}

		public override GroupInfo OfflineGroupInfo { get; set; }

		public override T ItemFor(int section, int row)
		{
			if (row >= items.Count)
			{
				this.ReloadData();
				return default(T);
			}
			return items[row];
		}
		public override string[] SectionIndexTitles()
		{
			return new string[0];
		}
		public override int RowsInSection(int section)
		{
			return items.Count;
		}
		public override int NumberOfSections()
		{
			return 1;
		}
		public override string HeaderForSection(int section)
		{
			return "";
		}
		public override SimpleTables.ICell GetHeaderICell(int section)
		{
			return null;
		}

		public Action LoadMore {get;set;}
	}
}
