using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;
using Microsoft.Extensions.ObjectPool;
namespace valuetasklabs
{
    public class QueueWithTcs : IDisposable
    {
        Task _Worker;
        Channel<TaskCompletionSource<int>> _Channel;
        CancellationTokenSource _Cts = new CancellationTokenSource();
        public QueueWithTcs()
        {
            _Channel = Channel.CreateUnbounded<TaskCompletionSource<int>>();
            _Worker = Task.Run(async () => await Worker().ConfigureAwait(false));
        }
        async Task Worker()
        {
            while (!_Cts.IsCancellationRequested)
            {
                if (!await _Channel.Reader.WaitToReadAsync().ConfigureAwait(false))
                {
                    break;
                }
                while (!_Cts.IsCancellationRequested && _Channel.Reader.TryRead(out var item))
                {
                    item.SetResult(1);
                }
            }
        }
        public Task<int> Enqueue()
        {
            if (_Worker.IsCompleted || _Worker.IsCanceled)
            {
                throw new Exception("thread already stopped");
            }
            var tcs = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);
            if (!_Channel.Writer.TryWrite(tcs))
            {
                throw new Exception("failed to write to channel");
            }
            return tcs.Task;
        }
        public void Dispose()
        {
            if (_Cts != null && !_Cts.IsCancellationRequested)
            {
                _Cts.Cancel();
            }
            if (!_Channel.Reader.Completion.IsCompleted)
            {
                _Channel.Writer.TryComplete();
            }
            if (!_Worker.IsCompleted)
            {
                _Worker.Wait();
            }
            while (_Channel.Reader.TryRead(out var item))
            {
                item.TrySetCanceled();
            }
            _Cts.Dispose();
            _Cts = null;
        }
    }

}