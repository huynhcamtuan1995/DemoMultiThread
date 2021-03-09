using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ProcessThread
{
    public class QueueThread
    {
        private static ConcurrentDictionary<string, ThreadModel> ConcurrentDictionary = new ConcurrentDictionary<string, ThreadModel>();
        private static ConcurrentQueue<string> ConcurrentQueue = new ConcurrentQueue<string>();
        private static Semaphore Semaphore = new Semaphore(10, 10, "MySemaphore");
        private static AutoResetEvent QueueEvent = new AutoResetEvent(false);

        internal static Timer TimerExpired;

        static QueueThread()
        {
            TimerRun();

            new Thread(() => StartThreads()).Start();
        }

        /// <summary>
        /// Timer to run every 5s, to progress expired request
        /// </summary>
        public static void TimerRun()
        {
            TimerExpired = new Timer(1000 * 5);
            TimerExpired.Elapsed += ProcessOnRemoveExpired;
            TimerExpired.AutoReset = true;
            TimerExpired.Start();
        }

        /// <summary>
        /// Remove expired request
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ProcessOnRemoveExpired(object source, ElapsedEventArgs e)
        {
            //get all expired request in Dictionary store
            List<string> expiredModels = ConcurrentDictionary.Values
                .Where(x => x.IsExpire())
                .OrderBy(x => x.CreateAt)
                .Select(x => x.Name)
                .ToList();

            foreach (string modelKey in expiredModels)
            {
                if (ConcurrentDictionary.TryRemove(modelKey, out ThreadModel model))
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

        /// <summary>
        /// isolate thread to running thread
        /// </summary>
        public static void StartThreads()
        {
            while (true)
            {
                if (ConcurrentQueue.IsEmpty)
                {
                    //make this thread sleep while request in queue empty
                    QueueEvent.WaitOne();
                }

                //dequeue from queue and progress 
                if (ConcurrentQueue.TryDequeue(out string modelName)
                    && ConcurrentDictionary.TryRemove(modelName, out ThreadModel model))
                {
                    Thread t = CreateThread(Semaphore,
                        () => ProgressAsync(model));

                    t.Start();
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
            if (!ConcurrentDictionary.TryAdd(model.Name, model))
            {
                return false;
            }

            ConcurrentQueue.Enqueue(model.Name);
            QueueEvent.Set();

            return true;
        }

        /// <summary>
        ///  managed max thread to progress by semaphore
        /// </summary>
        /// <param name="semaphore"></param>
        /// <param name="threadStart"></param>
        /// <returns></returns>
        public static Thread CreateThread(Semaphore semaphore, ThreadStart threadStart)
        {
            semaphore.WaitOne();
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
                int availableThreads = Semaphore.Release();
                Console.WriteLine($"                        ---> AvailableThreads:{availableThreads} || Queue:{ConcurrentQueue.Count()} || Dictiondary:{ConcurrentDictionary.Count()}");

            }

        }
    }
}
