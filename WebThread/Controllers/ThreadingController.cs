using ConsoleThread;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public IEnumerable<bool> SendMultipleRequest(int thread)
        {
            bool[] boolArr = new bool[] { };

            for (int i = 0; i < thread; i++)
            {
                string name = $"{Guid.NewGuid().ToString("N")}_{DateTime.Now.ToString("HHmmss")}";
                ThreadModel model = new ThreadModel();
                model.Name = name;
                model.Number = i;

                ThreadTest.AddThreadRequest(model);
            }

            ThreadTest.StartThreads();

            ThreadTest.autoResetEvent.WaitOne();
            return boolArr;
        }


        [HttpGet]
        public bool SendSingleRequest()
        {

            return true;
        }
    }
}
