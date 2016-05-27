using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Web;
using MusicPlayer.Api;
using SimpleAuth;
using SimpleAuth.Providers;
using MusicPlayer.Managers;

namespace Amazon.CloudDrive
{
	public class CloudDriveApi : AmazonApi
	{
		const string RootUrl = "https://drive.amazonaws.com/drive/v1";
		public string ContentUrl { get; set; }
		public string MetaUrl { get; set; }

		public CloudDriveApi(string identifier, HttpClientHandler handler = null)
			: this(
				identifier, MusicPlayer.ApiConstants.AmazonApiKey, MusicPlayer.ApiConstants.AmazonSecret, handler)
		{
			Scopes = new string[] {"profile","clouddrive:read", "clouddrive:write"};
		}

		public CloudDriveApi(string identifier, string clientId, string clientSecret, HttpMessageHandler handler = null)
			: base(identifier, clientId, clientSecret, handler)
		{
			SetEndpoint(RootUrl);
		}

		public async Task Identify()
		{

			try
			{
				var data = await Get<Dictionary<string, string>>("https://api.amazon.com/user/profile");
				foreach (var pair in data)
				{
					CurrentAccount.UserData[pair.Key] = pair.Value;
				}
				SaveAccount(CurrentAccount);
			}
			catch (Exception ex)
			{
				LogManager.Shared.Report(ex);
			}
		}

		protected override async Task OnAccountUpdated(Account account)
		{
			//			try{
			const string contentUrlKey = "CloudDriveEndpointContent";
			const string metaUrlKey = "CloudDriveEndpointMeta";
			const string expirationKey = "CloudDriveEndpointExpiration";
			//Amazon gives users specific endpoints to hit. Check the cache and see if its valid.
			string endpoint;
			if (account.UserData.TryGetValue(contentUrlKey, out endpoint))
			{
				var expires = DateTime.Parse(account.UserData[expirationKey]);
				if (expires > DateTime.Today)
				{
					ContentUrl = endpoint;
					MetaUrl = account.UserData[metaUrlKey];
					return;
				}
			}
			//
			var accountData = await GetEndpoint();
			if (accountData.HasError)
				throw new Exception(accountData.ErrorDescription);

			//It is reccomended you keep them for 3-5 days;
			ContentUrl = account.UserData[contentUrlKey] = accountData.ContentUrl;
			MetaUrl = account.UserData[metaUrlKey] = accountData.MetadataUrl;
			account.UserData[expirationKey] = DateTime.Today.AddDays(4).ToShortDateString();
			SaveAccount(account);
			//			}
			//			catch(Exception ex) {
			//				Console.WriteLine (ex);
			//			}
		}

		static string CreateUrl(string root, string path = "")
		{
			var url = Path.Combine(root.TrimEnd('/'), path.Trim('/'));
			return url;
		}

		void SetEndpoint(string endpoint)
		{
			var uri = new Uri(endpoint);
			Client.BaseAddress = uri;
		}

		string CreateMetaUrl(string path)
		{
			var url = CreateUrl(string.IsNullOrEmpty(MetaUrl) ? RootUrl : MetaUrl, path);
			return url;
		}

		string CreateContentUrl(string path)
		{
			var url = CreateUrl(string.IsNullOrEmpty(ContentUrl) ? RootUrl : ContentUrl, path);
			return url;
		}

		#region API Calls

		#region Accounts

		public async Task<CloudAccountInfoResponse> GetAccountInfo()
		{
			const string path = "account/info";
			var accountData = await this.Get<CloudAccountInfoResponse>(CreateMetaUrl(path));
			return accountData;
		}

		public async Task<CloudEndpointResponse> GetEndpoint()
		{
			const string path = "account/endpoint";
			var accountData = await this.Get<CloudEndpointResponse>(CreateMetaUrl(path));

			return accountData;
		}

		public async Task<CloudQuotaResponse> GetQuota()
		{
			const string path = "account/quota";
			return await this.Get<CloudQuotaResponse>(CreateMetaUrl(path));
		}

		public async Task<CloudUsageResponse> GetUsage()
		{
			const string path = "account/usage";
			return await this.Get<CloudUsageResponse>(CreateMetaUrl(path));
		}

		#endregion// Accounts

		#region Nodes

		public async Task<string> UploadFile(FileUploadData data, string file, bool suppressDuplicateError = false)
		{
			var type = MimeTypes.GetMimeType(file);
			using (var stream = File.OpenRead(file))
			{
				return await UploadFile(data, type, stream);
			}
		}

		public async Task<string> UploadFile(FileUploadData data, string mimetype, Stream filestream,
			bool suppressDuplicateError = false)
		{
			const string path = "nodes?suppress={0}";

			try
			{
				using (var content = new MultipartFormDataContent())
				{
					var json = SerializeObject(data);
					var metaContent = new StringContent(json);
					metaContent.Headers.Add("Content-Disposition", "form-data; name=\"metadata\"");
					content.Add(metaContent);

					var streamContent = new StreamContent(filestream);
					streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimetype);
					streamContent.Headers.Add("Content-Disposition",
						string.Format("form-data; name=\"content\";filename=\"{0}\"", data.Name));

					content.Add(streamContent);
					var url = CreateContentUrl(string.Format(path, suppressDuplicateError));
					var result = await PostMessage(url, content);
					result.EnsureSuccessStatusCode();
					var resp = await result.Content.ReadAsStringAsync();
					return resp;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			return "";
		}


		public async Task<CloudNodeResponse> GetNodeList(CloudNodeRequest request)
		{
			const string path = "nodes";
			var query = request.ToString();
			var url = Path.Combine(CreateMetaUrl(path), query);
			//			var data = await GetString (url);
			return await this.Get<CloudNodeResponse>(url);
		}

		public async Task<HttpResponseMessage> GetDownload(CloudNode node)
		{
			var url = GetDownloadUrl(node);
			return await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
		}

		public string GetDownloadUrl(CloudNode node)
		{
			return GetDownloadUrl(node.Id);
		}

		public string GetDownloadUrl(string id)
		{
			const string path = "nodes/{0}/content?download=false";
			return CreateContentUrl(string.Format(path, id));
		}

		public async Task<CloudNode> GetNode(string id)
		{
			const string path = "nodes/{0}?tempLink=true";
			return await Get<CloudNode>(CreateMetaUrl(string.Format(path, id)));
		}

		public async Task<string> GetTemporaryUrl(CloudNode node)
		{
			return await GetTemporaryUrl(node.Id);
		}

		public async Task<string> GetTemporaryUrl(string id)
		{
			var node = await GetNode(id);
			return node.TempLink;
		}

		public async Task<CloudChangeResult> GetChanges(CloudChangesRequest parameters = null)
		{
			const string path = "changes";
			try
			{
				var url = CreateMetaUrl(path);
				var postData = parameters == null || parameters.IsEmpty() ? "" : this.SerializeObject(parameters);
				var message = await PostMessage(url, new StringContent(postData, Encoding.UTF8, "text/json"));
				var stream = await message.Content.ReadAsStreamAsync();
				var gZip = new GZipStream(stream, CompressionMode.Decompress);
				var result = new CloudChangeResult
				{
					HasMore = true,
				};
				Task.Run(() => ReadChanges(gZip, result));
				return result;
			}
			catch (Exception ex)
			{
				return new CloudChangeResult
				{
					Error = ex.ToString(),
					ErrorDescription = ex.Message.ToString(),
				};
			}
		}

		public async Task<bool> UpdateMetaDataProperty(string id,string owner, string key, string value)
		{
			try
			{
				var path = $"nodes/{id}/properties/{HttpUtility.UrlEncode(owner)}/{HttpUtility.UrlEncode(key)}";
				var url = CreateMetaUrl(path);
				//EnsureApiStatusCode = false;
				var result = await Put(new { value = value },url);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		public async Task<bool> UpdateMetaData(CloudNode node)
		{
			try
			{
				var path = $"nodes/{node.Id}";
				var url = CreateMetaUrl(path);
				var result = await Patch(node,url);
				return true;
			}
			catch (Exception ex)
			{
				return false;
			}
		}

		protected void ReadChanges(Stream stream, CloudChangeResult result)
		{
			try
			{
				using (var reader = new StreamReader(stream, Encoding.UTF8))
				{
					while (!reader.EndOfStream)
					{
						var line = reader.ReadLine();
						if (line.StartsWith("{\"end\""))
						{
							result.AddResults(new List<CloudNode>(), false);
							return;
						}
						var data = Deserialize<CloudChangesResultData>(line);
						if (data.HasError)
						{
							result.SetError(data.StatusCode, data.Error, data.ErrorDescription);
							return;
						}
						result.AddResults(data.Nodes, true);
					}
				}
			}
			catch (Exception ex)
			{
				result.SetError(ex);
			}
		}

		#endregion //Nodes

		#region Trash

		public async Task<bool> AddToTrash(CloudNode node)
		{
			return await AddToTrash(node.Id);
		}

		public async Task<bool> AddToTrash(string id)
		{
			const string path = "trash/{0}";
			var url = CreateMetaUrl(string.Format(path, id));
			var response = await PutMessage(url, null);
			var json = await response.Content.ReadAsStringAsync();
			var node = Deserialize<CloudNode>(json);
			return node.Status == CloudNodeStatus.TRASH;
		}

		public async Task<CloudNode[]> ListTrash()
		{
			const string path = "trash";
			return await Get<CloudNode[]>(CreateMetaUrl(path));
		}

		public async Task<CloudNode> Restore(CloudNode node)
		{
			return await Restore(node.Id);
		}

		public async Task<CloudNode> Restore(string id)
		{
			const string path = "trash/{0}/restore";
			return await Post<CloudNode>(CreateMetaUrl(string.Format(path, id)), "");
		}

		#endregion //Trash

		#endregion// API Calls
	}
}