using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TP.ConcurrentProgramming.Data
{
    class DiagnosticLogger
    {
        private static readonly BlockingCollection<string> Queue = new();
        private static Thread? writerThread;
        private static volatile bool stop = false;

        static DiagnosticLogger()
        {
            writerThread = new Thread(() =>
            {
                using var writer = new StreamWriter("diagnostics.txt", append: true, Encoding.ASCII);
                while (!stop || !Queue.IsCompleted)
                {
                    try
                    {
                        string? line = Queue.Take();
                        writer.WriteLine(line);
                        writer.Flush();
                    }
                    catch
                    {
                        Thread.Sleep(100);
                    }
                }
            });
            writerThread.Start();
        }

        public static void LogBallState(IBall ball)
        {
            if (Queue.IsAddingCompleted)
                return; // Nie próbuj dodawać, jeśli kolejka jest zamknięta

            string line = $"{DateTime.UtcNow:O};{ball.Position.x:F2};{ball.Position.y:F2};{ball.Velocity.x:F2};{ball.Velocity.y:F2};{ball.Mass:F2};{ball.Radius:F2}";
            try
            {
                Queue.Add(line);
            }
            catch (InvalidOperationException)
            {
                // Kolejka została zamknięta, ignorujemy
            }
        }

        public static void Stop()
        {
            stop = true;
            Queue.CompleteAdding();
            writerThread?.Join();
        }
    }
}
