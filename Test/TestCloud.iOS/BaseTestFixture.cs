
using NUnit.Framework;
using System;
using Xamarin.UITest;
using System.Linq;

namespace TestCloud.iOS
{
	public partial class BaseTestFixture
	{
		protected IApp App;
		protected Platform platform;

		public BaseTestFixture (Platform platform)
		{
			this.platform = platform;
		}

		[SetUp]
		public void BeforeEachTest ()
		{
			App = AppInitializer.StartApp (platform);
			if (RequiresLogin)
				LoginTest.CheckLogin ();
		}

		public virtual bool RequiresLogin
		{
			get{return true;}

		}
	}
}

