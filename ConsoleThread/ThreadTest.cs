using ConsoleThread;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ConsoleThread
{

    public class ThreadTest
    {
        private static ConcurrentDictionary<string, ThreadModel> concurrentDictionary = new ConcurrentDictionary<string, ThreadModel>();
        private static ConcurrentQueue<string> concurrentQueue = new ConcurrentQueue<string>();
        private static Semaphore semaphore = new Semaphore(10, 10, "MySemaphore");

        public static AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        internal static Timer TimerNotification;

        static ThreadTest()
        {
            //TimerRun();
            //StartThreads();
        }

        public static void TimerRun()
        {
            TimerNotification = new Timer(1000 * 5);
            TimerNotification.Elapsed += ProcessOnExecute;
            TimerNotification.AutoReset = true;
            TimerNotification.Start();
        }

        private static void ProcessOnExecute(object source, ElapsedEventArgs e)
        {

        }

        public static void StartThreads()
        {
            while (true)
            {
                if (concurrentQueue.IsEmpty)
                {
                    break;
                }

                concurrentQueue.TryDequeue(out string modelName);
                if (concurrentDictionary.TryGetValue(modelName, out ThreadModel model))
                {
                    Thread t = CreateThread(semaphore,
                   () => ProgressAsync(model));

                    t.Start();
                }

                //model.Event.WaitOne();
            }

        }

        public static bool AddThreadRequest(ThreadModel model)
        {
            concurrentQueue.Enqueue(model.Name);
            concurrentDictionary.TryAdd(model.Name, model);
            return true;
        }

        public static Thread CreateThread(Semaphore semaphore, ThreadStart threadStart)
        {
            semaphore.WaitOne();
            return new Thread(threadStart);
        }

        private static void ProgressAsync(ThreadModel model)
        {
            try
            {
                //StringContent content = new StringContent(CreateJsonBody(data, function, requestID), Encoding.UTF8, "application/json");
                //HttpResponseMessage responseTask;
                //using (HttpClient client = new HttpClient())
                //{
                //    client.BaseAddress = new Uri(gatewayOptions.Url);
                //    client.DefaultRequestHeaders.Accept.Add(
                //        new MediaTypeWithQualityHeaderValue("application/json"));
                //    client.DefaultRequestHeaders.Add("Signature", signature);
                //    client.DefaultRequestHeaders.Add("Authorization", token);
                //    responseTask = await client.PostAsync("", content);
                //}

                int randomSleep = new Random().Next(0, 10);

                Console.WriteLine($"{model.Number} sleep {randomSleep * 1000}");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://localhost:5069");

                    var response = client.GetAsync($"WeatherForecast/ReceiveRequest/{randomSleep}").GetAwaiter().GetResult();
                }

                //model.Event.Set();
                concurrentDictionary.TryRemove(model.Name, out _);
                Console.WriteLine($"         {model.Number} Sent");
                //Common.WriteLog(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {model.Number}--------------------XXXXXX");
                //Log ex
            }
            finally
            {

                int availableThreads = semaphore.Release();

                Console.WriteLine($"                            availableThreads:{availableThreads}");
                Console.WriteLine($"                 Queue:{concurrentQueue.Count()}");
                Console.WriteLine($"                 Dictiondary:{concurrentDictionary.Count()}");

                if (concurrentDictionary.IsEmpty)
                {
                    autoResetEvent.Set();
                }
            }

        }
    }
}
