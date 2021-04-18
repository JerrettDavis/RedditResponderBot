using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Bot.Service.Common.Extensions
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Allows you to call ForEach on an enumerable asynchronously..
        /// </summary>
        /// <param name="source">The source enumerable</param>
        /// <param name="action">The action to perform</param>
        /// <param name="dop">The max degree of parallelism</param>
        /// <param name="cancellationToken">Tells the ForEach loop to terminate prematurely</param>
        /// <typeparam name="T">The type of enumerable being iterated over</typeparam>
        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> action, 
            int dop = -1, 
            CancellationToken cancellationToken = default)
        {
            // Arguments validation omitted
            var block = new ActionBlock<T>(action, 
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = dop, 
                    BoundedCapacity = dop, 
                    CancellationToken = cancellationToken
                });
            try
            {
                foreach (var item in source)
                    if (!await block.SendAsync(item, cancellationToken)
                        .ConfigureAwait(false))
                        break;
                block.Complete();
            }
            catch (Exception ex)
            {
                ((IDataflowBlock)block).Fault(ex);
            }

            try
            {
                await block.Completion.ConfigureAwait(false);
            }
            catch // Propagate AggregateException
            {
                block.Completion.Wait(cancellationToken);
            } 
        }
    }
}