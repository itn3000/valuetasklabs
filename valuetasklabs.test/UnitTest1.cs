using System;
using Xunit;
using System.Threading.Tasks;
using System.Linq;

namespace valuetasklabs.test
{
    public class UnitTest1
    {
        [Fact]
        public async Task MultiValueTask()
        {
            const int TaskNum = 10;
            const int LoopNum = 100000;
            using (var queue = new QueueWithValueTask())
            {
                await Task.WhenAll(Enumerable.Range(0, TaskNum).Select(async threadId =>
                {
                    for (int i = 0; i < LoopNum; i++)
                    {
                        await queue.Enqueue().ConfigureAwait(false);
                    }
                })).ConfigureAwait(false);
            }
        }
    }
}
