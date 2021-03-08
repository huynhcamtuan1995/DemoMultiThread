using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProcessThreading;
using System;
using System.Collections.Generic;

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

        [HttpGet("{thread}")]
        public IEnumerable<bool> SendMultipleRequest(int thread)
        {
            bool[] boolArr = new bool[] { };

            for (int i = 0; i < thread; i++)
            {
                ThreadModel model = new ThreadModel();
                string name = $"{Guid.NewGuid().ToString("N")}_{DateTime.Now.ToString("HHmmss")}";
                model.Name = name;
                model.Number = i;

                if (!QueueThread.AddThreadRequest(model))
                {
                    ThreadResponse response = new ThreadResponse();
                    response.Status = 400;
                    response.Message = "Bad Request";

                    return new bool[] { false };
                }
            }

            return boolArr;
        }


        [HttpGet]
        public ThreadResponse SendSingleRequest()
        {
            ThreadModel model = new ThreadModel();
            string name = $"{Guid.NewGuid().ToString("N")}_{DateTime.Now.ToString("HHmmss")}";
            model.Name = name;

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
