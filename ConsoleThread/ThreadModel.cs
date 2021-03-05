using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConsoleThread
{
    public class ThreadModel
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public int Sleep { get; set; }

        public AutoResetEvent Event = new AutoResetEvent(false);

        public bool IsExpire()
        {
            return CreateAt.AddSeconds(30) <= DateTime.Now;
        }
    }
}
