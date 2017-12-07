using System;
namespace MusicPlayer
{
	public static class MathExtensions
	{

		public static double SafeDivideByZero(this double inDouble, double divisor) => divisor.IsZero() ? 0 : inDouble / divisor;
		public static bool IsZero(this double inDouble) => Math.Abs(inDouble) < Double.Epsilon;
		public static bool IsZero(this float inFloat) => Math.Abs(inFloat) < float.Epsilon;
		public static bool IsNotZero(this float inFloat) => Math.Abs(inFloat) > float.Epsilon;
		public static bool IsNotZero(this double inDouble) => Math.Abs(inDouble) > Double.Epsilon;
	}
}
