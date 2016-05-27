
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SimpleTables;

namespace MusicPlayer.Droid
{
	public class BaseModelViewFragment <TVM, T> : BaseFragment where TVM : TableViewModel<T> where T : class
	{
		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			// Create your fragment here
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			if (ListAdapter == null)
			{
				if (Model == null)
					throw new Exception("Model is required.");

				Model.ListView = ListView;
				Model.Context = App.Context;
				ListAdapter = Model;
				
			}
			this.SetListShown(true);
		}

		TVM _model;

		public TVM Model
		{
			get { return _model; }
			set
			{
				_model = value;
			}
		}

		//public override void TeardownEvents()
		//{
		//	base.TeardownEvents();
		//	Model.ClearEvents();
		//}
	}
}

