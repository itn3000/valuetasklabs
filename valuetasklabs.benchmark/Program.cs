using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace valuetasklabs.benchmark
{
    [MemoryDiagnoser]
    [ShortRunJob]
    public class QueueBench
    {
        [Params(100000)]
        public int LoopNum;
        [Params(1, 10, 100)]
        public int TaskNum;
        [Benchmark]
        public void ValueTaskBench()
        {
            using (var queue = new QueueWithValueTask())
            {
                Task.WhenAll(Enumerable.Range(0, TaskNum).Select(async threadId =>
                {
                    for (int i = 0; i < LoopNum; i++)
                    {
                        await queue.Enqueue().ConfigureAwait(false);
                    }
                })).Wait();
            }
        }
        [Benchmark]
        public void TcsBench()
        {
            using (var queue = new QueueWithTcs())
            {
                Task.WhenAll(Enumerable.Range(0, TaskNum).Select(async threadId =>
                {
                    for (int i = 0; i < LoopNum; i++)
                    {
                        await queue.Enqueue().ConfigureAwait(false);
                    }
                })).Wait();
            }
        }

    }
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<QueueBench>();
        }
    }
}
