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
        private static AutoResetEvent queueEvent = new AutoResetEvent(false);

        internal static Timer TimerNotification;

        static ThreadTest()
        {
            TimerRun();

            new Thread(() => StartThreads()).Start();
        }

        public static void TimerRun()
        {
            TimerNotification = new Timer(1000 * 5);
            TimerNotification.Elapsed += ProcessOnRemoveExpired;
            TimerNotification.AutoReset = true;
            TimerNotification.Start();
        }

        /// <summary>
        /// Remove expired request
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ProcessOnRemoveExpired(object source, ElapsedEventArgs e)
        {
            //get all expired request in Dictionary store
            List<string> expiredModels = concurrentDictionary.Values
                .Where(x => x.IsExpire())
                .OrderBy(x => x.CreateAt)
                .Select(x => x.Name)
                .ToList();

            foreach (string modelKey in expiredModels)
            {
                if (concurrentDictionary.TryRemove(modelKey, out ThreadModel model))
                {
                    //set value for expire item
                    if (model.Response == null)
                    {
                        //reponse stats reponse to timeout or somethign...
                        //then set request event to continutes reponse 
                        ThreadResponse response = new ThreadResponse();
                        response.Status = 408;
                        response.Message = "Timeout";

                        model.Response = response;
                    }
                    model.Event.Set();
                }
            }
        }

        public static void StartThreads()
        {
            while (true)
            {
                if (concurrentQueue.IsEmpty)
                {
                    queueEvent.WaitOne();
                }

                if (concurrentQueue.TryDequeue(out string modelName)
                    && concurrentDictionary.TryRemove(modelName, out ThreadModel model))
                {
                    Thread t = CreateThread(semaphore,
                        () => ProgressAsync(model));

                    t.Start();
                }
            }

        }

        public static bool AddThreadRequest(ThreadModel model)
        {
            if (!concurrentDictionary.TryAdd(model.Name, model))
            {
                return false;
            }

            concurrentQueue.Enqueue(model.Name);
            queueEvent.Set();

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

                    //set reponse
                    model.Response = new ThreadResponse()
                    {
                        Status = 200,
                        Message = "Have data"
                    };
                }

                model.Event.Set();
                //Common.WriteLog(model);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {model.Number}-------------------- {ex.Message}");
                //Log ex
            }
            finally
            {
                int availableThreads = semaphore.Release();
                Console.WriteLine($"                        ---> AvailableThreads:{availableThreads} || Queue:{concurrentQueue.Count()} || Dictiondary:{concurrentDictionary.Count()}");

            }

        }
    }
}
