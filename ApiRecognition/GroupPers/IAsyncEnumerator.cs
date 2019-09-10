using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ApiRecognition.GroupPers
{
    public interface IMyAsyncEnumerable<out T>
    {
        IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default(CancellationToken));
    }

    public interface IAsyncEnumerator<out T> : IAsyncDisposable
    {
        T Current
        {
            get;
        }

        ValueTask<bool> MoveNextAsync();
    }

    public interface IAsyncDisposable
    {
        ValueTask DisposeAsync();
    }
}
