using System;
using System.Reflection;
using System.Linq;
namespace MusicPlayer.Forms
{
	public static class InternalRegistrar
	{
		static MethodInfo registerMethod;
		static MethodInfo GetHandlerMethod;
		static object reggistrar;
		static InternalRegistrar ()
		{
			try
			{
				var assembly = Assembly.LoadFrom("Xamarin.Forms.Core.dll");
				Type baseType = assembly.GetType("Xamarin.Forms.Registrar");
				var prop = baseType.GetProperty("Registered",BindingFlags.Instance |
							BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
                var props = baseType.GetProperties();
				reggistrar = prop.GetValue(null, null);
				var type = reggistrar.GetType();
				registerMethod = type.GetMethod("Register",BindingFlags.Instance | BindingFlags.NonPublic |
											   BindingFlags.Public | BindingFlags.Static);
				GetHandlerMethod = type.GetMethod("GetHandlerType", BindingFlags.Instance | BindingFlags.NonPublic |
											   BindingFlags.Public | BindingFlags.Static);

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			Console.WriteLine("foo");
		}

		public static void Register<T1, T2>()
		{
			Register(typeof(T1), typeof(T2));
		}
		public static void Register(Type tview, Type trender)
		{
			//registerMethod.Invoke(reggistrar, new object[] { tview, trender });
		}
	}
}
