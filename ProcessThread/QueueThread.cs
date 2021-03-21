using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace ProcessThread
{
    public class QueueThread
    {
        /// <summary>
        /// Name: DictionaryRequest
        /// Stored Request model wait for progress
        /// </summary>
        private static ConcurrentDictionary<string, ThreadModel> ConcurrentDictionary = new ConcurrentDictionary<string, ThreadModel>();

        /// <summary>
        /// Name: QueueRequest
        /// Stored Request key wait for progress
        /// </summary>
        private static ConcurrentQueue<string> ConcurrentQueue = new ConcurrentQueue<string>();

        /// <summary>
        /// Semaphore to limit threads join in progress
        /// Keep application not out of resource
        /// </summary>
        private static SemaphoreSlim Semaphore = new SemaphoreSlim(50, 50);
        //private static Semaphore Semaphore = new Semaphore(10, 10, "MySemaphore");

        /// <summary>
        /// event status to wakeup isolate running thread [StartThreads()] when QueueRequest is empty 
        /// </summary>
        private static AutoResetEvent QueueEvent = new AutoResetEvent(false);

        /// <summary>
        /// Timer to remove expired request
        /// </summary>
        internal static Timer TimerExpire;

        /// <summary>
        /// Log factory
        /// </summary>
        private static readonly ILoggerFactory _loggerFactory = new LoggerFactory();

        /// <summary>
        /// Logger
        /// </summary>
        private static readonly ILogger _logger;

        static QueueThread()
        {
            _logger = _loggerFactory.CreateLogger<QueueThread>();

            TimerExpireRun();

            //isolate thread to run thread
            new Thread(() => StartThreads()).Start();
        }

        /// <summary>
        /// Timer to run every 5s, to progress expired request
        /// </summary>
        public static void TimerExpireRun()
        {
            TimerExpire = new Timer(1000 * 5);
            TimerExpire.Elapsed += ProcessOnRemoveExpire;
            TimerExpire.AutoReset = true;
            TimerExpire.Start();
        }

        /// <summary>
        /// Remove expired request
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ProcessOnRemoveExpire(object source, ElapsedEventArgs e)
        {
            //stop the timer to run until end of request
            TimerExpire.Stop();
            try
            {
                Next:
                //get all expired request in Dictionary store
                List<string> expiredRequest = ConcurrentDictionary.Values
                    .Where(x => x.IsExpire())
                    .OrderBy(x => x.CreateAt)
                    .Select(x => x.Name)
                    .ToList();

                if (expiredRequest.Count() == 0)
                {
                    goto End;
                }

                foreach (string modelKey in expiredRequest)
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
                //repeat
                goto Next;

                End:
                return;
            }
            catch (Exception ex)
            {
                //log ex
                _logger.LogError($"Error ------------------------------- {ex.Message} {ex.InnerException}");
            }
            finally
            {
                //start timer after stop
                TimerExpire.Start();
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
                    Thread t = CreateThread(
                        Semaphore,
                        () => ProgressAsync(model));

                    t.Start();

                    //register cancellation to cancel running thread
                    model.CancelToken.Register(() => t.Interrupt());
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
                //using (HttpClientHandler handler = new HttpClientHandler())
                //{
                //    handler.CookieContainer = new CookieContainer();
                //    handler.CookieContainer.Add(baseAddress, new Cookie("name", "value")); // Adding a Cookie
                //    using (var client = new HttpClient(handler) {BaseAddress = baseAddress})
                //    {
                //        StringContent content = new StringContent(CreateJsonBody(data, function, requestID),
                //            Encoding.UTF8, "application/json");
                //        client.BaseAddress = new Uri(gatewayOptions.Url);
                //        client.DefaultRequestHeaders.Accept.Add(
                //            new MediaTypeWithQualityHeaderValue("application/json"));
                //        client.DefaultRequestHeaders.Add("Signature", signature);
                //        client.DefaultRequestHeaders.Add("Authorization", token);
                //        HttpResponseMessage responseTask = await client.PostAsync("", content);
                //    }
                //}

                int randomSleep = new Random().Next(0, 10);

                Console.WriteLine($"{model.Number} sleep {randomSleep * 1000}");

                //log
                _logger.LogInformation($"{model.Number} sleep {randomSleep * 1000}");

                Uri baseAddress = new Uri("https://localhost:5069");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = baseAddress;

                    HttpResponseMessage response = client.GetAsync($"WeatherForecast/ReceiveRequest/{randomSleep}")
                        .GetAwaiter()
                        .GetResult();

                    //set response
                    model.Response = new ThreadResponse()
                    {
                        Status = 200,
                        Message = "Have data"
                    };
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error {model.Number}-------------------- {ex.Message} {ex.InnerException}");

                //Log ex
                _logger.LogError($"Error {model.Number}-------------------- {ex.Message} {ex.InnerException}");
            }
            finally
            {
                model.Event.Set();
                int availableThreads = Semaphore.Release();
                Console.WriteLine($"                        ---> AvailableThreads:{availableThreads} || Queue:{ConcurrentQueue.Count()} || Dictiondary:{ConcurrentDictionary.Count()}");

                //log
                _logger.LogInformation($"                        ---> AvailableThreads:{availableThreads} || Queue:{ConcurrentQueue.Count()} || Dictiondary:{ConcurrentDictionary.Count()}");
            }
        }
    }
}
