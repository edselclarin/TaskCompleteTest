using System.Collections.Concurrent;

namespace TaskCompleteTest
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Synchronous tests.
            ReadRegisterTest();
            WriteRegisterTest();

            // Asynchronous tests.
            UploadFileTest();
            DownloadFileTest();

            Console.ReadKey();
        }

        private static async void DownloadFileTest()
        {
            var processor = new Processor();

            var cmd = new MyCommand()
            {
                Command = "DownloadFile",
                TaskCompletionSource = new TaskCompletionSource<MyResponse>()
            };

            using var cts = new CancellationTokenSource(60000);
            cts.Token.Register(() => cmd.TaskCompletionSource.TrySetCanceled());

            processor.CommandQueue.Enqueue(cmd);

            Console.WriteLine("[DownloadFileTest] Command enqueued.");

            try
            {
                var res = await cmd.TaskCompletionSource.Task;

                Console.WriteLine($"[DownloadFileTest] {res.Response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DownloadFileTest] Error processing {cmd.Command}. {ex.Message}");
            }
        }

        private static async void UploadFileTest()
        {
            var processor = new Processor();

            var cmd = new MyCommand()
            {
                Command = "UploadFile",
                TaskCompletionSource = new TaskCompletionSource<MyResponse>()
            };

            using var cts = new CancellationTokenSource(60000);
            cts.Token.Register(() => cmd.TaskCompletionSource.TrySetCanceled());

            processor.CommandQueue.Enqueue(cmd);

            Console.WriteLine("[UploadFileTest] Command enqueued.");

            try
            {
                var res = await cmd.TaskCompletionSource.Task;

                Console.WriteLine($"[UploadFileTest] {res.Response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UploadFileTest] Error processing {cmd.Command}. {ex.Message}");
            }
        }

        private static void ReadRegisterTest()
        {
            var processor = new Processor();

            var cmd = new MyCommand()
            {
                Command = "ReadRegister",
                TaskCompletionSource = new TaskCompletionSource<MyResponse>()
            };

            // Wait longer than process time.
            using var cts = new CancellationTokenSource(3000);
            cts.Token.Register(() => cmd.TaskCompletionSource.TrySetCanceled());

            processor.CommandQueue.Enqueue(cmd);

            Console.WriteLine("[ReadRegisterTest] Command enqueued.");

            try
            {
                cmd.TaskCompletionSource.Task.Wait();

                var res = cmd.TaskCompletionSource.Task.Result;

                Console.WriteLine($"[ReadRegisterTest] {res.Response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReadRegisterTest] Error processing {cmd.Command}. {ex.Message}");
            }
        }

        private static void WriteRegisterTest()
        {
            var processor = new Processor();

            var cmd = new MyCommand()
            {
                Command = "WriteRegister",
                TaskCompletionSource = new TaskCompletionSource<MyResponse>()
            };

            using var cts = new CancellationTokenSource(1000);
            cts.Token.Register(() => cmd.TaskCompletionSource.TrySetCanceled());

            processor.CommandQueue.Enqueue(cmd);

            Console.WriteLine("[WriteRegisterTest] Command enqueued.");

            try
            {
                cmd.TaskCompletionSource.Task.Wait();

                var res = cmd.TaskCompletionSource.Task.Result;

                Console.WriteLine($"[WriteRegisterTest] {res.Response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WriteRegisterTest] Error processing {cmd.Command}. {ex.Message}");
            }
        }
    }

    public class MyCommand
    {
        public string Command { get; set; } 
        public TaskCompletionSource<MyResponse> TaskCompletionSource { get; set; }
    }

    public class MyResponse
    {
        public string Response { get; set; }
    }

    public class Processor
    {
        public ConcurrentQueue<MyCommand> CommandQueue = new ConcurrentQueue<MyCommand>();

        public Processor()
        {
            var cts = new CancellationTokenSource();
            _ = Task.Run(async () =>
            {
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(10);

                    if (!CommandQueue.TryDequeue(out var cmd))
                    {
                        continue;
                    }

                    // Simulate process time.
                    int timeMs = cmd.Command switch
                    {
                        "UploadFile" => 10000,
                        "DownloadFile" => 5000,
                        _ => 1000
                    };
                    await Task.Delay(timeMs);

                    var res = new MyResponse()
                    {
                        Response = $"Executed {cmd.Command} at {DateTime.Now}."
                    };

                    cmd.TaskCompletionSource.TrySetResult(res);
                }
            });
        }
    }
}