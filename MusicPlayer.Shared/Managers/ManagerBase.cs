using System;
using System.Collections.Generic;
using System.Text;
using MusicPlayer.Models;

namespace MusicPlayer.Managers
{
	public class ManagerBase<T> : BaseModel where T : new()
	{
		public static T Shared { get; set; } = new T();
	}
}