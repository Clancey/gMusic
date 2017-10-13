using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicPlayer.Server
{
	public class Router
	{

		Dictionary<string, (Type RouteType, string Path)> routes = new Dictionary<string, (Type RouteType, string Path)>();
		Dictionary<string, (Type RouteType, string Path)> matchedRoutes = new Dictionary<string, (Type RouteType, string Path)>();
		public void AddRoute(string path,Type route)
		{
			var orgPath = path;
			routes [path.ToLower ()] = (route,orgPath);
			var parts = path.Split ('/');
			for (var i = 0; i < parts.Length; i++) {
				var part = parts [i];
				if (!part.StartsWith ("{") || !part.EndsWith ("}"))
					continue;
				parts [i] = "*";
			}
			if (!parts.Contains ("*"))
				return;
			path = string.Join ("/", parts);
			matchedRoutes [path] = (route, orgPath);
		}

		public Route GetRoute (string path)
		{
			(Type RouteType, string Path) routeInformation;
			if (!routes.TryGetValue (path.Trim('/').ToLower (), out routeInformation)) {
				var matches = matchedRoutes.Where (x => path.ToLower().IsMatch (x.Key.ToLower())).ToList ();
				if (matches.Count == 0)
					return null;
				if (matches.Count == 1) {
					routeInformation = matches.First ().Value;
				}
				else {
					//Sometimes there are two matching paterns check what one matches perfectly;
					var pathSplit = path.Split (new char [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
					var closestMatch = matches.Where (x => {
						var matchParts = x.Key.Split (new char [] { '/' }, StringSplitOptions.RemoveEmptyEntries);
						if (matchParts.Length != pathSplit.Length)
							return false;
						//Check the matching patern
						var pathPartCopy = pathSplit.ToArray ();
						for (var i = 0; i < matchParts.Length; i++) {
							if (matchParts [i] == "*")
								pathPartCopy [i] = "*";
						}
						return string.Join("/",matchParts) == string.Join ("/", pathPartCopy);
					}).ToList();
					routeInformation = closestMatch.FirstOrDefault ().Value;
				}
			}
			var route = (Route)Activator.CreateInstance(routeInformation.RouteType);
			route.Path = routeInformation.Path;
			return route;
		}

		public void AddRoute<T>() where T : Route
		{
			var type = typeof(T);
			var path = type.GetCustomAttributes(true).OfType<PathAttribute>().FirstOrDefault();
			if (path == null)
				throw new Exception("Cannot automatically regiseter Route without Path attribute");
			AddRoute(path.Path.Trim('/'), type); 
		}

	}
}

