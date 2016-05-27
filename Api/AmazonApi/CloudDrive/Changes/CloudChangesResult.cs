using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusicPlayer.Api;
using SimpleAuth;

namespace Amazon.CloudDrive
{
	public class CloudChangeResult : ApiResponse
	{
		public bool Reset { get; set; }

		public int StatusCode { get; set; }

		public bool HasMore { get; internal set; }

		public string Checkpoint { get; set; }

		TaskCompletionSource<List<CloudNode>> tcs;

		public async Task<List<CloudNode>> LoadMoreNodes()
		{
			if (!HasMore)
			{
				lock (Nodes)
				{
					var nodes = Nodes.ToList();
					Nodes.Clear();
					return nodes;
				}
			}

			if (tcs?.Task?.IsCompleted == false)
				return await tcs.Task;
			tcs = new TaskCompletionSource<List<CloudNode>>();
			return await tcs.Task;
		}

		List<CloudNode> Nodes = new List<CloudNode>();

		internal void AddResults(List<CloudNode> data, bool hasMore)
		{
			HasMore = hasMore;

			if (tcs != null)
			{
				lock (Nodes)
				{
					data.AddRange(Nodes);
					Nodes.Clear();
				}
				tcs.TrySetResult(data);
			}
			else
			{
				lock (Nodes)
				{
					Nodes.AddRange(data);
				}
			}
		}

		internal void SetError(int code, string error, string description)
		{
			this.StatusCode = code;
			this.Error = error;
			this.ErrorDescription = description;
			tcs?.TrySetException(new Exception(error));
		}

		internal void SetError(Exception ex)
		{
			this.Error = ex.Message;
			this.ErrorDescription = ex.Message;
			tcs?.TrySetException(ex);
		}
	}

	internal class CloudChangesResultData : ApiResponse
	{
		public bool Reset { get; set; }

		public int StatusCode { get; set; }

		public string Checkpoint { get; set; }

		public List<CloudNode> Nodes { get; set; }
	}
}