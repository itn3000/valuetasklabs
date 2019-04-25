using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using Microsoft.Extensions.ObjectPool;

namespace valuetasklabs
{
    // from https://github.com/dotnet/corefx/blob/master/src/Common/tests/System/Threading/Tasks/Sources/ManualResetValueTaskSource.cs
    sealed class ManualResetValueTaskSource<T> : IValueTaskSource<T>, IValueTaskSource
    {
        private ManualResetValueTaskSourceCore<T> _core; // mutable struct; do not make this readonly

        public bool RunContinuationsAsynchronously { get => _core.RunContinuationsAsynchronously; set => _core.RunContinuationsAsynchronously = value; }
        public short Version => _core.Version;
        public void Reset() => _core.Reset();
        public void SetResult(T result) => _core.SetResult(result);
        public void SetException(Exception error) => _core.SetException(error);
        public void SetPool(ObjectPool<ManualResetValueTaskSource<T>> pool)
        {
            _pool = pool;
        }
        ObjectPool<ManualResetValueTaskSource<T>> _pool;

        public T GetResult(short token)
        {
            try
            {
                var ret = _core.GetResult(token);
                return ret;
            }
            finally
            {
                _pool.Return(this);
                _pool = null;
            }
        }
        void IValueTaskSource.GetResult(short token) => _core.GetResult(token);
        public ValueTaskSourceStatus GetStatus(short token) => _core.GetStatus(token);
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags) => _core.OnCompleted(continuation, state, token, flags);
    }
    public class QueueWithValueTask : IDisposable
    {
        Task _Worker;
        Channel<ManualResetValueTaskSource<int>> _Channel;
        ObjectPool<ManualResetValueTaskSource<int>> _Pool;
        CancellationTokenSource _Cts = new CancellationTokenSource();
        public QueueWithValueTask()
        {
            _Pool = new DefaultObjectPool<ManualResetValueTaskSource<int>>(
                new DefaultPooledObjectPolicy<ManualResetValueTaskSource<int>>()
            );
            _Channel = Channel.CreateUnbounded<ManualResetValueTaskSource<int>>();
            _Worker = Task.Run(async () => await Worker().ConfigureAwait(false));
        }
        async Task Worker()
        {
            try
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        public ValueTask<int> Enqueue()
        {
            if (_Worker.IsCompleted || _Worker.IsCanceled)
            {
                throw new Exception("thread already stopped");
            }
            var vts = _Pool.Get();
            vts.SetPool(_Pool);
            if (!_Channel.Writer.TryWrite(vts))
            {
                throw new Exception("failed to write to channel");
            }
            return new ValueTask<int>(vts, vts.Version);
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
                item.SetException(new OperationCanceledException());
            }
        }
    }
}