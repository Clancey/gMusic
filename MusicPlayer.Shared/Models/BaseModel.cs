using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MusicPlayer.Models
{
	public class BaseModel : INotifyPropertyChanged, IDisposable
	{
		readonly Dictionary<string, List<Action>> actions = new Dictionary<string, List<Action>>();
		public event PropertyChangedEventHandler PropertyChanged;

		public BaseModel()
		{
			PropertyChanged += OnPropertyChanged;
		}


		/// <summary>
		///     Dispose method.
		/// </summary>
		public void Dispose()
		{
			ClearEvents();
		}

		internal bool ProcPropertyChanged<T>(ref T currentValue, T newValue, [CallerMemberName] string propertyName = "")
		{
			return PropertyChanged.SetProperty(this, ref currentValue, newValue, propertyName);
		}

		internal void ProcPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
		{
			List<Action> actionList;
			if (!actions.TryGetValue(propertyChangedEventArgs.PropertyName, out actionList)) return;
			foreach (Action action in actionList)
			{
				action();
			}
		}

		public void SubscribeToProperty(string property, Action action)
		{
			List<Action> actionList;
			if (!actions.TryGetValue(property, out actionList))
				actionList = new List<Action>();
			actionList.Add(action);
			actions[property] = actionList;
		}

		public void UnSubscribeToProperty(string property, Action action)
		{
			List<Action> actionList;
			if (!actions.TryGetValue(property, out actionList))
				return;
			if (actionList.Contains(action))
				actionList.Remove(action);
			actions[property] = actionList;
		}

		public void UnSubscribeToProperty(string property)
		{
			List<Action> actionList;
			if (!actions.TryGetValue(property, out actionList))
				return;
			actionList.Clear();
			actions[property] = actionList;
		}

		public void ClearEvents()
		{
			actions.Clear();
			if (PropertyChanged == null)
				return;
			var invocation = PropertyChanged.GetInvocationList();
			foreach (var p in invocation)
				PropertyChanged -= (PropertyChangedEventHandler) p;
		}
	}
}