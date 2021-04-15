using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Bot.Service.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static async Task ForEachAsync<T>(this IEnumerable<T> source,
            Func<T, Task> action, int dop = -1, CancellationToken cancellationToken = default)
        {
            // Arguments validation omitted
            var block = new ActionBlock<T>(action, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = dop, 
                BoundedCapacity = dop, 
                CancellationToken = cancellationToken
            });
            try
            {
                foreach (var item in source)
                    if (!await block.SendAsync(item).ConfigureAwait(false)) break;
                block.Complete();
            }
            catch (Exception ex) { ((IDataflowBlock)block).Fault(ex); }
            try { await block.Completion.ConfigureAwait(false); }
            catch { block.Completion.Wait(cancellationToken); } // Propagate AggregateException
        }
    }
}