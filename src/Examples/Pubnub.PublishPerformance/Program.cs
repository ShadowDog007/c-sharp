using System;
using System.Diagnostics;
using System.Threading;

namespace PubnubApi.PublishPerformance
{
    class Program
    {
        private const int TestSize = 100;
        private static int Remaining;
        private static SemaphoreSlim Signal = new SemaphoreSlim(0);

        private static Pubnub Pubnub;

        public static int GetWorkerThreads()
        {
            ThreadPool.GetAvailableThreads(out var threads, out var _);
            return threads;
        }

        static void Main(string[] args)
        {
            var subKey = args[0];
            var pubKey = args[1];

            Console.WriteLine($"sub: {subKey}");
            Console.WriteLine($"pub: {pubKey}");

            ThreadPool.SetMinThreads(200, 200);

            Pubnub = new Pubnub(new PNConfiguration
            {
                SubscribeKey = subKey,
                PublishKey = pubKey,
                EnableTelemetry = false,
            });

            Test("Warmup");

            Test("Run - 1");
            Test("Run - 2");
        }

        private static void Test(string mode)
        {
            Console.WriteLine($"Starting Test {mode}");
            var threads = TimeTest(TestThreads);
            Console.WriteLine($"{mode}-Threads     took {threads}");
            var taskFactory = TimeTest(TestTaskFactory);
            Console.WriteLine($"{mode}-TaskFactory took {taskFactory}");

            Console.WriteLine($"Improvement: {100.0 - 100.0*taskFactory.Ticks/threads.Ticks:F2}");
        }

        private static TimeSpan TimeTest(Action test)
        {
            Remaining = TestSize;

            var sw = Stopwatch.StartNew();
            test();
            // Wait for test to complete
            Console.WriteLine("Waiting");
            Signal.Wait();
            sw.Stop();

            return sw.Elapsed;
        }

        private static void TestThreads()
        {
            for (var i = 0; i < TestSize; ++i)
            {
                Pubnub.Publish()
                    .Channel("test")
                    .Message("")
                    .AsyncThread(new PNPublishResultExt(OnCallback));
            }
        }

        private static void TestTaskFactory()
        {
            for (var i = 0; i < TestSize; ++i)
            {
                Pubnub.Publish()
                    .Channel("test")
                    .Message("")
                    .AsyncTaskFactory(new PNPublishResultExt(OnCallback));
            }
        }

        private static void OnCallback(PNPublishResult result, PNStatus status)
        {
            if (Interlocked.Decrement(ref Remaining) == 0)
                Signal.Release();

            if (status.Error)
            {
                Console.WriteLine(status.ErrorData.Information);
                Console.WriteLine(status.ErrorData.Throwable?.ToString());
            }
        }
    }
}
