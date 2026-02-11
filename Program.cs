using System;
using System.Diagnostics;
using System.Threading;

namespace SyncDemo
{
    class Program
    {
        const int ThreadsCount = 10;
        const int IncrementsPerThread = 1000;

        static int counter = 0;
        static readonly object lockObject = new object();
        static readonly Mutex mutex = new Mutex();

        static void Main()
        {
            Console.WriteLine("=== Synchronization demo ===");
            Console.WriteLine($"Threads: {ThreadsCount}, increments per thread: {IncrementsPerThread}\n");

            RunScenario("[NO SYNC]   ", IncrementNoSync);   // не оптимизированный вариант
            RunScenario("[lock]      ", IncrementWithLock); // оптимизированный вариант
            RunScenario("[Mutex]     ", IncrementWithMutex);// другой примитив синхронизации

            Console.WriteLine("\nDone. Press Enter.");
            Console.ReadLine();
        }

        static void RunScenario(string name, ThreadStart worker)
        {
            counter = 0;
            var sw = Stopwatch.StartNew();

            Thread[] threads = new Thread[ThreadsCount];
            for (int i = 0; i < ThreadsCount; i++)
            {
                threads[i] = new Thread(worker);
                threads[i].Start();
            }

            foreach (var t in threads)
                t.Join();

            sw.Stop();

            int expected = ThreadsCount * IncrementsPerThread;
            Console.WriteLine($"{name} Expected: {expected}, Actual: {counter}, Time: {sw.ElapsedMilliseconds} ms");
        }

        // -------- не синхронизированный вариант (гонка данных) --------
        static void IncrementNoSync()
        {
            for (int i = 0; i < IncrementsPerThread; i++)
            {
                counter++; // без защиты
            }
        }

        // -------- оптимизированный вариант: lock --------
        static void IncrementWithLock()
        {
            for (int i = 0; i < IncrementsPerThread; i++)
            {
                lock (lockObject)
                {
                    counter++;
                }
            }
        }

        // -------- другой примитив синхронизации: Mutex --------
        static void IncrementWithMutex()
        {
            for (int i = 0; i < IncrementsPerThread; i++)
            {
                mutex.WaitOne();
                counter++;
                mutex.ReleaseMutex();
            }
        }
    }
}
