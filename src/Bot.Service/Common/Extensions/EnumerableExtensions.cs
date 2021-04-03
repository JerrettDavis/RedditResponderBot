// TedCruzResponderBot - Simple real-time chat application.
// Copyright (C) 2021  Jerrett D. Davis
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

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