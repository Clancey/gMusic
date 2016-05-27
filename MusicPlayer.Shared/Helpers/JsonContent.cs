using System;
using System.Collections.Generic;
using System.Text;

namespace System.Net.Http
{
	internal class JsonContent : StringContent
	{
		public JsonContent(string json) : base(json, Encoding.UTF8, "application/json")
		{
		}
	}
}