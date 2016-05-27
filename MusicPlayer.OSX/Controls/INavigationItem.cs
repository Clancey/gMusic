using System;

namespace MusicPlayer
{
	public interface INavigationItem
	{
		string Title {get;}
		NSNavigationController NavigationController {get;set;}
	}
}

