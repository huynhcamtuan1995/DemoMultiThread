using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace LockingThread
{
    public class ThreadModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;

        public Mutex mutex = new Mutex();
    }


}
