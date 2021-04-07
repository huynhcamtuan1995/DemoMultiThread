using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ProcessThread;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
        public async Task<IEnumerable<bool>> SendMultipleRequestAsync(int thread)
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
        public async Task<ThreadResponse> SendSingleRequestAsync()
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
