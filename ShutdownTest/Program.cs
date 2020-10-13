using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;

namespace ShutdownTest
{
    class Program
    {
        static void Main()
        {
            using IHost host = new HostBuilder().Build();
            Console.WriteLine("started");
            File.AppendAllLines("log.txt", new[] { "started" });
            host.Start();
            host.WaitForShutdown();
            Console.WriteLine("DISPOSING");
            File.AppendAllLines("log.txt", new[] { "DISPOSING" });
        }
        // Test vectors:
        // - must log for Windows shutdown.
        //static void Main()
        //{
        //    var done = new ManualResetEventSlim(false);
        //    using var shutdownCts = new CancellationTokenSource();
        //    try
        //    {
        //        AttachCtrlcSigtermShutdown(shutdownCts, done);
        //        while (true)
        //        {
        //            Thread.Sleep(1000);
        //            Console.WriteLine("running");
        //            File.AppendAllLines("log.txt", new[] { "running" });
        //            if (shutdownCts.IsCancellationRequested)
        //            {
        //                break;
        //            }
        //        }

        //        Console.WriteLine("DISPOSING");
        //        File.AppendAllLines("log.txt", new[] { "DISPOSING" });
        //    }
        //    finally
        //    {
        //        done.Set();
        //    }
        //}

        private static void AttachCtrlcSigtermShutdown(CancellationTokenSource cts, ManualResetEventSlim resetEvent)
        {
            void Shutdown()
            {
                if (!cts.IsCancellationRequested)
                {
                    try
                    {
                        cts.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }

                // Wait on the given reset event
                resetEvent.Wait();
            };

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => Shutdown();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                Shutdown();
                // Don't terminate the process immediately, wait for the Main thread to exit gracefully.
                eventArgs.Cancel = true;
            };
        }
    }
}
