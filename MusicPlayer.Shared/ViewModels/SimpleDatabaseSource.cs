using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using SimpleDatabase;
using MusicPlayer.Data;

namespace MusicPlayer.ViewModels
{
	public class SimpleDatabaseSource<T> : IList, INotifyCollectionChanged where T : new()
	{
		public bool IsGrouped { get; set; } = true;
		public SimpleDatabaseSource(SimpleDatabaseConnection connection)
		{
			Database = connection;
		}
		public object this[int index]
		{
			get
			{
				try
				{
					Debug.WriteLine($"Loading {index}");
					if (IsGrouped)
						return new GroupedList<T>(Database, GroupInfo, index)
						{
							Display = Database?.SectionHeader<T>(GroupInfo, index) ?? "",
						};
					return Database != null ? Database.ObjectForRow<T>(GroupInfo,0,index) : new T();
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return new T();
				}
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		GroupInfo groupInfo;

		public event NotifyCollectionChangedEventHandler CollectionChanged;

		public void ResfreshData()
		{
			CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
		}

		public GroupInfo GroupInfo
		{
			get { return groupInfo ?? (groupInfo =Database.GetGroupInfo<T>()); }
			set { 
				groupInfo = value;
				ResfreshData();
			}
		}
		public GroupInfo OfflineGroupInfo
		{
			get { return groupInfo ?? (groupInfo = Database.GetGroupInfo<T>()); }
			set
			{
				groupInfo = value;
				ResfreshData();
			}
		}

		public GroupInfo CurrentGroupInfo => Settings.ShowOfflineOnly ? OfflineGroupInfo : GroupInfo;

		public int Count
		{
			get
			{
				var c = (IsGrouped ? Database?.NumberOfSections<T>(GroupInfo) : Database?.RowsInSection<T>(GroupInfo, 0)) ?? 0;
				if (c == 0)
					Console.WriteLine("what?");
				return c;
			}
		}

		public SimpleDatabase.SimpleDatabaseConnection Database { get; set; }

		public bool IsFixedSize
		{
			get
			{
				return true;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public bool IsSynchronized
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public object SyncRoot
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public int Add(object value)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(object value)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public int IndexOf(object value)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		public void Remove(object value)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}


	}
	public class GroupedList<T> : IList where T : new()
	{
		public GroupedList(SimpleDatabase.SimpleDatabaseConnection database, GroupInfo groupInfo, int section)
		{
			GroupInfo = groupInfo;
			Database = database;
			Section = section;
		}
		public GroupInfo GroupInfo { get; set; }
		string display = "";
		public string Display { 
			get
			{
				return display;
			}
			set { display = value; }
		}

		public SimpleDatabase.SimpleDatabaseConnection Database { get; set; }
		public int Section { get; set; }

		public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public bool IsFixedSize
		{
			get
			{
				return true;
			}
		}

		public int Count
		{
			get
			{
				return Database?.RowsInSection<T>(GroupInfo,Section) ?? 0;
			}
		}

		public object SyncRoot
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public bool IsSynchronized
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		public object this[int index]
		{
			get
			{
				try
				{
					Debug.WriteLine($"Loading {Section}:{index}");
					var item =  Database.ObjectForRow<T>(GroupInfo, Section, index);
					return item;
				}
				catch (Exception ex)
				{
					Debug.WriteLine(ex);
					return new T();
				}
			}

			set
			{
				throw new NotImplementedException();
			}
		}

		public int Add(object value)
		{
			throw new NotImplementedException();
		}

		public bool Contains(object value)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public int IndexOf(object value)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, object value)
		{
			throw new NotImplementedException();
		}

		public void Remove(object value)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public IEnumerator GetEnumerator()
		{
			throw new NotImplementedException();
		}
	}
}
