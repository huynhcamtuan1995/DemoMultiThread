using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;

namespace ConsoleApp1
{
    public class Models
    {
        public string Name { get; set; }
        public DateTime Time { get; set; }
        public int Sleep { get; set; }

        public DateTime ExpireTime { get; set; } = DateTime.Now.AddSeconds(30);

        public AutoResetEvent Event = new AutoResetEvent(false);
    }
    public class ThreadTest
    {
        private static System.Timers.Timer newTimer;
        private static Dictionary<string, Models> ConcurrentDictionary = new Dictionary<string, Models>();
        private static Queue<string> ConcurrentQueue = new Queue<string>();
        private static Semaphore semaphore = new Semaphore(10, 10, "MySemaphore");

        static ThreadTest()
        {
            TimerRun();
        }

        private static void DisplayTimeEvent(object source, ElapsedEventArgs e)
        {
            var GetAll = ConcurrentDictionary.Where(c => c.Value.ExpireTime <= DateTime.Now);
            
            foreach(var data in GetAll)
            {
                data.Value.Event.Set();
                ConcurrentDictionary.Remove(data.Key);
            }
        }

        public static void TimerRun()
        {
            newTimer = new System.Timers.Timer();
            newTimer.Elapsed += new ElapsedEventHandler(DisplayTimeEvent);
            newTimer.Interval = 5000;
            newTimer.Enabled = true;
            newTimer.Start();
        }

        public static Thread CreateThread(Semaphore semaphore, ThreadStart threadStart)
        {
            semaphore.WaitOne();

            return new Thread(threadStart);
        }

        public static void NewThreads(int maxCount)
        {
            for (int i = 0; i < maxCount; i++)
            {
                string name = Guid.NewGuid().ToString("N");

                string key = $"{name}_{DateTime.Now.ToString("HHmmss")}";

                int randomSleep = new Random().Next(1, 9) * 1000;

                Models model = new Models
                {
                    Name = name,
                    Time = DateTime.Now,
                    Sleep = randomSleep
                };

                ConcurrentDictionary.Add(key, model);

                Thread thread = CreateThread(semaphore,
                    () => Progress(model));

                thread.Start();
                //model.Event.WaitOne();
            }
        }

        private static void Progress(Models model)
        {
            try
            {
                model.Event.WaitOne();


                Common.WriteLog(model);
            }

            finally
            {

                semaphore.Release();
            }

        }
    }
}
