using System;

namespace Amazon.CloudDrive
{
	public abstract class NodeFilter
	{
		protected abstract string Name { get; }

		protected virtual string Seperator
		{
			get { return ":"; }
		}

		protected abstract string Value { get; }

		public static string EscapeValue(string value)
		{
			return value.Replace(" ", "\\ ");
		}

		public override string ToString()
		{
			return string.Format("{0}{1}{2}", Name, Seperator, Value);
		}
	}
}