using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.ViewModels;
using SimpleTables;

namespace MusicPlayer.iOS.ViewControllers
{
	class BaseModelViewController<TVM,T> : BaseTableViewController where TVM : TableViewModel<T> where T: class
	{
		TVM _model;

		public TVM Model
		{
			get { return _model; }
			set
			{
				_model = value;
			}
		}

		public override void LoadView()
		{
			base.LoadView();
			if(Model == null)
				throw new Exception("Model is required.");
			TableView.Source = Model;
		}

		public override void TeardownEvents()
		{
			base.TeardownEvents();
			Model.ClearEvents();
		}
	}
}
