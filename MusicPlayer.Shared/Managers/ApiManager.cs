using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicPlayer.Data;
using MusicPlayer.Api;
using MusicPlayer.Api.GoogleMusic;
using Amazon.CloudDrive;
using OneDrive;
using YoutubeApi;
using SoundCloud;

namespace MusicPlayer.Managers
{
	public class ApiManager : ManagerBase<ApiManager>
	{
		Dictionary<ServiceType, Type> ApiServiceTypes = new Dictionary<ServiceType, Type>
		{
			//{ServiceType.Amazon,typeof(CloudDriveApi)},
			{ServiceType.Google,typeof(GoogleMusicApi)},
			#if DEBUG
			{ServiceType.OneDrive,typeof(OneDriveApi)},
			{ServiceType.SoundCloud,typeof(SoundCloudApi)},
			#endif
			{ServiceType.YouTube,typeof(YoutubeOauthApi)},
		};
		Dictionary<Type, Type> ApiProviderTypes = new Dictionary<Type, Type>
		{
			{typeof(GoogleMusicApi),typeof(GoogleMusicProvider) },
			{typeof(YoutubeOauthApi),typeof(YoutubeProvider) },

			#if DEBUG
			{typeof(SoundCloudApi),typeof(SoundCloudProvider) },
			//{typeof(CloudDriveApi),typeof(AmazonMusicProvider) },
			{typeof(OneDriveApi),typeof(OneDriveProvider) },
			#endif
		};

		public ApiManager()
		{
			
		}

		public async void Load()
		{
			var apis = Settings.CurrentApiModels.ToList();
			apis.ForEach(CreateApi);
			#if __IOS__
			var ipod = new MusicPlayer.Api.iPodApi.iPodProvider();
			Collection.Add(ipod.Id,ipod);
			#elif __OSX__

			var ipod = new ITunesProvider();
			ipod.Disabled = !Settings.IncludeIpod;
			Collection.Add(ipod.Id,ipod);

			var fileSytem = new FileSystemProvider{
				Disabled = Settings.ExcludeFileSystem
			};
			Collection.Add(fileSytem.Id,fileSytem);
			#endif
			await CreateYouTube ();
		}

		void CreateApi(ApiModel model)
		{
			try
			{
				if (model.Service == ServiceType.FileSystem)
					model.Service = ServiceType.OneDrive;
				var apiType = ApiServiceTypes[model.Service];
				var api = Activator.CreateInstance(apiType, model.Id.ToString(), null) as SimpleAuth.Api;
				api.DeviceId = model.DeviceId;
				api.ExtraDataString = model.ExtraData;

				var provider = CreateProvider(api);
				Collection.Add(model.Id.ToString(), provider);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}
		public MusicProvider CreateProvider(SimpleAuth.Api api)
		{
			try
			{
				var providerType = ApiProviderTypes[api.GetType()];
				var provider = Activator.CreateInstance(providerType, api) as MusicProvider;
				return provider;

			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}

			App.ShowNotImplmented(new Dictionary<string, string> { { "Service Type", api.GetType().ToString() } });
			return null;
		}

		public async Task CreateYouTube()
		{
			if (string.IsNullOrWhiteSpace (ApiConstants.YouTubeClientId))
				return;
			var youtubsApis = Collection.Values.Where(x => x.ServiceType == ServiceType.YouTube).ToList();
			if (youtubsApis.Any(x => x.Id != YoutubeProvider.DefaultId))
			{
				return;
			}
			var hasGoogle = Collection.Values.Any (x => x.ServiceType == ServiceType.Google);

			//If setting is on, and google is logged in. Then login, create youtube api
			if (hasGoogle && Settings.AutoAddYoutube) {
				if (youtubsApis.Count > 0)
				{
					Collection.Remove(youtubsApis.First().Id);
				}
				var api = ApiManager.Shared.CreateApi (MusicPlayer.Api.ServiceType.YouTube);
				var account = await api.Authenticate ();
				if (account == null) {
					var youTube = new YoutubeProvider ();
					Collection.Add (youTube.Id, youTube);
					return;
				}
				ApiManager.Shared.AddApi (api);
				
			}
//			// else just add a normal one too the collection
			else if(youtubsApis.Count == 0) {
				var youTube = new YoutubeProvider ();
				Collection.Add (youTube.Id, youTube);
			}
		}

		public async Task StartSync()
		{
			var tasks = Collection.Values.Select(x => x.SyncDatabase()).ToList();
			await Task.WhenAll(tasks);
		}

		public async Task ReSync()
		{
			using (new Spinner ("Syncing")) {
				var apis = Collection.Values.ToList();
				var tasks = apis.Select(x => x.Resync());
				await Task.WhenAll (tasks);
			}
		}

		readonly Dictionary<string, MusicProvider> Collection = new Dictionary<string, MusicProvider>();

		//If IPodOnly, iPod on counts
		public int Count => Collection.Count(x=> x.Value.RequiresAuthentication) +  (Settings.IPodOnly && Settings.IncludeIpod ? 1 : 0);

		public MusicProvider [] CurrentProviders => Collection.Where (x => x.Value.RequiresAuthentication).Select(x=> x.Value).ToArray ();

		public ServiceType[] SearchableServiceTypes => Collection.Values.Where(x=> x.Capabilities.Contains(MediaProviderCapabilities.Searchable)).Select(x => x.ServiceType).Distinct().ToArray();
		public ServiceType [] AvailableApiServiceTypes => ApiServiceTypes.Keys.ToArray ();

		public SimpleAuth.AuthenticatedApi CreateApi(ServiceType type)
		{
			var id = Settings.GetNextApiId().ToString();

			var apiType = ApiServiceTypes[type];
			var api = Activator.CreateInstance(apiType, id, null) as SimpleAuth.AuthenticatedApi;
			if (api != null)
				return api;
			App.ShowNotImplmented(new Dictionary<string, string> { { "Service Type", type.ToString() } });
			return null;
		}

		public static string ServiceTitle(ServiceType type)
		{
			return type.ToString();
		}
		public MusicProvider AddApi(SimpleAuth.Api api)
		{
			var provider = CreateProvider(api);
			if (provider == null)
			{
				return null;
			}
			SaveApi(api);

			Collection.Add(api.Identifier, provider);

			return provider;

		}

		public void SaveApi(SimpleAuth.Api api)
		{
			var id = int.Parse(api.Identifier);
			var record = new ApiModel
			{
				Id = id,
				Service = GetServiceType(api),
				DeviceId = api.DeviceId,
				ExtraData = api.ExtraDataString,
			};
			Settings.AddApiModel(record);
		}


		static ServiceType GetServiceType(SimpleAuth.Api api)
		{
			if (api is GoogleMusicApi)
				return ServiceType.Google;
			if (api is CloudDriveApi)
				return ServiceType.Amazon;
			if(api is YoutubeOauthApi)
				return ServiceType.YouTube;
			if (api is SoundCloudApi)
				return ServiceType.SoundCloud;
			if (api is OneDriveApi)
				return ServiceType.OneDrive;

			App.ShowNotImplmented(new Dictionary<string, string> { { "Api type", api.GetType().ToString() } });
			return ServiceType.FileSystem;
		}

		public T GetMusicProvider<T>(ServiceType type) where T : MusicProvider
		{
			var provider = GetMusicProvider(type);
			return provider as T;
		}

		public MusicProvider GetMusicProvider(ServiceType type)
		{
			return Collection.Values.FirstOrDefault(x => x.ServiceType == type);
		}

		public T GetMusicProvider<T>(string id) where T : MusicProvider
		{
			var provider = GetMusicProvider(id);
			return provider as T;
		}

		public MusicProvider GetMusicProvider(string id)
		{
			MusicProvider provider;
			Collection.TryGetValue(id, out provider);
			return provider;
		}

		public string GetAccount(ServiceType service)
		{
			return GetMusicProvider(service)?.Email;
		}
		public string DisplayName(ServiceType service)
		{
			switch (service)
			{
				case ServiceType.Amazon:
					return "Amazon Cloud Drive";
				case ServiceType.DropBox:
					return "Dropbox";
				case ServiceType.Google:
					return "Google Play Music";
				case ServiceType.SoundCloud:
					return "SoundCloud";
				case ServiceType.YouTube:
					return "YouTube";
				case ServiceType.OneDrive:
					return "OneDrive";
			}
			return service.ToString();
		}


		public string GetSvg(ServiceType serviceType)
		{
			switch (serviceType)
			{
				case ServiceType.Amazon:
				return "SVG/amazon.svg";
				case ServiceType.DropBox:
				return "SVG/dropbox-outline.svg";
				case ServiceType.Google:
				return "SVG/googleMusic.svg";
				case ServiceType.SoundCloud:
				return "SVG/soundCloudColor.svg";
				case ServiceType.YouTube:
				return "SVG/youtubeLogo.svg";
				case ServiceType.OneDrive:
				return "SVG/onedrive.svg";
			}
			return "";
		}

		public async Task LogOut (MusicProvider provider)
		{
			try {

				await provider.Logout ();
				int id;
				if(int.TryParse(provider.Id,out id))
					Settings.DeleteApiModel (id);
				Collection.Remove (provider.Id);

			} catch (Exception ex) {
				Console.WriteLine (ex);
			}

		}

		public async Task<bool> CreateAndLogin(ServiceType service)
		{
			var api = CreateApi(service);
			api.ResetData();
			var account = await api.Authenticate();
			if (account == null)
				return false;
			var manager = AddApi(api);
			using (new Spinner("Syncing Database"))
			{
				await manager.Resync();
			}
			return true;
		}


	}
}