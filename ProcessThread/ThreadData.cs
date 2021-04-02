using System;
using System.Threading;

namespace ProcessThread
{
    /// <summary>
    /// Thread data in progress
    /// </summary>
    public class ThreadModel
    {
        /// <summary>
        /// Use for Key in Queue Request
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// To counter threads (to Test)
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Request time in
        /// </summary>
        public DateTime CreateAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Set value to use for sleep time (to Test)
        /// </summary>
        public int Sleep { get; set; }

        /// <summary>
        /// Response to client
        /// </summary>
        public ThreadResponse Response { get; set; }

        /// <summary>
        /// Check is expired request
        /// </summary>
        /// <returns></returns>
        public bool IsExpire()
        {
            return CreateAt.AddSeconds(30) <= DateTime.Now;
        }

        /// <summary>
        /// Event to Suspend request until run finish isolate thread
        /// </summary>
        public AutoResetEvent Event = new AutoResetEvent(false);

        /// <summary>
        /// Cancellation token provide to cancel running thread
        /// </summary>
        public CancellationToken CancelToken { get; set; }

        /// <summary>
        /// For testing
        /// Use for ThreadingController/SendMultipleRequestAsync
        /// </summary>
        public CountdownEvent CountEvent { get; set; }
    }

    /// <summary>
    /// Thread result Response
    /// </summary>
    public class ThreadResponse
    {
        public int Status { get; set; }
        public string Message { get; set; }
    }
}
