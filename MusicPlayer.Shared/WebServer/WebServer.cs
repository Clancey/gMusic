using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MusicPlayer.Server
{
	public class WebServer
	{
		public Router Router = new Router ();
		public bool DebugMode { get; set; }
		public int Port { get; private set; }
		private readonly HttpListener _listener = new HttpListener();
		public WebServer(int webServerPort)
		{
			if (webServerPort == 0)
				webServerPort = GetRandomUnusedPort();
			Port = webServerPort;

			if (!HttpListener.IsSupported)
				throw new NotSupportedException("Http Listener is not supported");

			var prefixes = new [] {
				$"http://*:{webServerPort}/",
			};

			foreach (string s in prefixes) {
				Log ($"Listening on: {s}");
				_listener.Prefixes.Add (s);
			}
			init ();
		}
		void init ()
		{
			RegisterRoutes ();
		}
		public virtual void RegisterRoutes()
		{
			
		}

		/// <summary>
		/// Starts the server.
		/// </summary>
		public void Start(int concurrentConnections = 4)
		{
			_listener.Start();
			Task.Run (() => {
				var sem = new Semaphore (concurrentConnections, concurrentConnections);
				while (_listener.IsListening) {
					sem.WaitOne ();
					_listener.GetContextAsync ().ContinueWith (async (t) => {
						HttpListenerContext ctx = null;
						try {

							sem.Release ();
							ctx = await t;
							await ProcessReponse (ctx);

						} catch (Exception ex) {
							Console.WriteLine (ex);
							if (ctx == null)
								return;
							ctx.Response.StatusCode = 500;
						} finally {
							//if(!ctx.Request.KeepAlive)
								ctx?.Response.OutputStream.Close ();
						}
					});
				}
			});
		}

		public void Stop ()
		{
			if (_listener?.IsListening ?? false)
				return;
			_listener.Stop ();
		}

		async Task ProcessReponse(HttpListenerContext context)
		{
			var request = context.Request;
			try{
				var path = request?.Url?.LocalPath;

				path = path?.TrimStart ('/');
				Log ($"Request from: {request.RemoteEndPoint.Address} Path: {path}");
				var route = Router.GetRoute(path);
				if (route == null) {
					Console.WriteLine ($"Route not found: {path}");
					context.Response.StatusCode = 404;
					return;
				}
				if(!route.SupportsMethod(context.Request.HttpMethod)) {
					context.Response.StatusCode = 405;
					return;
				}
				if (!(await route.CheckAuthentication (context))) {
					context.Response.StatusCode = 403;
					return;
				}
				context.Response.ContentType = route.ContentType;
				await route.ProcessReponse (context);
			}
			catch(Exception ex) {
				Console.WriteLine (request.RawUrl);
				Console.WriteLine (ex);
				context.Response.StatusCode = 404;
				return;
			}
		}

		void Log (string message)
		{
			if (DebugMode)
				Console.WriteLine (message);
		}

		public static int GetRandomUnusedPort()
		{
			var listener = new TcpListener(IPAddress.Any, 0);
			listener.Start();
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}
	}
}
