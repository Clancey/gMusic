using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicPlayer
{
	public static class IEnumberableExtension
	{
		public static async Task BatchForeach<T>(this IEnumerable<T> items, int batchSize,Func<IEnumerable<T>,Task> action)
		{
			int currentBatch = 0;
			bool hasMore = items.Any();
			while (hasMore) {
				var itemsBatch = items.Skip(currentBatch * batchSize).Take(batchSize);
				hasMore = itemsBatch.Count() == batchSize;
				currentBatch++;
				var t = action(itemsBatch);
				await t;
			}
		}
	}
}

