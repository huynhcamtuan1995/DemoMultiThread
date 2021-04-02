using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ProcessThread;

namespace WebThread.Validation
{
    public class ValidationQueueLimit : ActionFilterAttribute
    {
        public ValidationQueueLimit()
        {
            
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (QueueThread.ConcurrentQueue.Count >= QueueThread.QueueSize)
            {
                context.Result = new ContentResult()
                {
                    StatusCode = (int)HttpStatusCode.BadGateway,
                    Content = "Server limit access. Pls try access later.",
                    ContentType = "application/json"
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
