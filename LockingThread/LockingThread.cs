using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace LockingThread
{
    public class LockingThread
    {
        public static readonly Thread CoreThread;

        public static SemaphoreSlim Semaphore = new SemaphoreSlim(50, 50);

        private static ConcurrentDictionary<int, ThreadModel> ConcurrentDictionary = new ConcurrentDictionary<int, ThreadModel>();

        static LockingThread()
        {
            //isolate thread to run thread
            CoreThread = new Thread(() => StartThreads());
        }

        /// <summary>
        /// isolate thread to running thread
        /// </summary>
        public static void StartThreads()
        {
            while (true)
            {
                List<int> list = ConcurrentDictionary
                    .OrderBy(x => x.Value.CreateAt)
                    .Select(x => x.Key)
                    .ToList();

                //dequeue from queue and progress 
                foreach (var key in list)
                {
                    if (ConcurrentDictionary.TryRemove(key, out ThreadModel model))
                    {
                      
                        Thread t = CreateThread(
                            Semaphore,

                            () =>

                        {

                            ProgressAsync(model);

                        });

                        t.Start();
                    }
                }

            }

        }

        /// <summary>
        /// adding new request to queue
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool AddThreadRequest(ThreadModel model)
        {
            if (!ConcurrentDictionary.TryAdd(model.ID, model))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///  managed max thread to progress by semaphore
        /// </summary>
        /// <param name="semaphore"></param>
        /// <param name="threadStart"></param>
        /// <returns></returns>
        public static Thread CreateThread(SemaphoreSlim semaphore, ThreadStart threadStart)
        {
            semaphore.Wait();
            return new Thread(threadStart);
        }

        /// <summary>
        /// function process request
        /// </summary>
        /// <param name="model"></param>
        private static void ProgressAsync(ThreadModel model)
        {
           
            try
            {
                lock (model)
                {
                    Console.WriteLine($"{model.ID} going to sleep | left: {ConcurrentDictionary.Count()}");
                    Thread.Sleep(5000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {model.ID}-------------------- {ex.Message} {ex.InnerException}");

                //Log ex
            }
            finally
            {
                int availableThreads = Semaphore.Release();
                Console.WriteLine($"                        ---> AvailableThreads:{availableThreads} || Dictiondary:{ConcurrentDictionary.Count()}");
            }
        }
    }
}
