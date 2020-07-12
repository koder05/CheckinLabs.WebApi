using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CheckinLabs.AppBase
{
    public abstract class StartModuleBase
    {
        public TaskCompletionSource<bool> CompletionSource { get; } = new TaskCompletionSource<bool>();

        protected abstract Task RunInnerAsync(CancellationToken cancellationToken);
        internal async Task RunAsync(CancellationToken cancellationToken, IEnumerable<Task> depModulesTasks)
        {
            await Task.WhenAll(depModulesTasks);
            try
            {
                await RunInnerAsync(cancellationToken);
            }
            finally
            {
                CompletionSource.SetResult(true);
            }

        }
    }
}
