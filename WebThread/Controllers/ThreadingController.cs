using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProcessThread;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WebThread.Validation;

namespace WebThread.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class ThreadingController : ControllerBase
    {


        private readonly ILogger<ThreadingController> _logger;

        public ThreadingController(ILogger<ThreadingController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{numberOfThread}")]
        public async Task<IEnumerable<bool>> SendMultipleRequestAsync(int numberOfThread)
        {
            CountdownEvent countEvent = new CountdownEvent(numberOfThread);
            bool[] boolArr = new bool[] { };

            for (int i = 0; i < numberOfThread; i++)
            {
                ThreadModel model = new ThreadModel();
                string name = $"{Guid.NewGuid().ToString("N")}_{DateTime.Now.ToString("HHmmss")}";
                model.Name = name;
                model.Number = i;
                model.CountEvent = countEvent;

                //if cannot add request -> response bad request
                if (!QueueThread.AddThreadRequest(model))
                {
                    model.CountEvent.Signal();
                }
            }

            QueueThread.CoreThread.Start();
            countEvent.Wait();
            return boolArr;
        }


        [HttpGet]
        [ServiceFilter(typeof(ValidationQueueLimit))]
        public async Task<ThreadResponse> SendSingleRequestAsync(CancellationToken cancelToken)
        {
            ThreadModel model = new ThreadModel();
            string name = $"{Guid.NewGuid().ToString("N")}_{DateTime.Now.ToString("HHmmss")}";
            model.Name = name;
            model.CancelToken = cancelToken;

            //if cannot add request -> response bad request
            if (!QueueThread.AddThreadRequest(model))
            {
                ThreadResponse response = new ThreadResponse();
                response.Status = 400;
                response.Message = "Bad Request";

                return response;
            }

            model.Event.WaitOne();

            return model.Response;
        }
    }
}
