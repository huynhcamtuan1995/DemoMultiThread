using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleThread
{
    class Program
    {
        class A
        {
            Semaphore s = new Semaphore(3, 10);
            public void RunThread()
            {
                for (int i = 0; i <= 100; i++)
                {
                    Thread t = new Thread(() => Test(i));

                    t.Name = i.ToString();
                    t.Start();
                }
            }
            public void Test(int i)
            {
                try
                {
                    Console.WriteLine("int number: " + i.ToString());
                }
                finally
                {
                    Thread t = Thread.CurrentThread;
                    Console.WriteLine("thread name: " + t.Name);
                }
            }
        }

        class B
        {

            public async Task TestTask()
            {
                Task<string> test1 = Task.Run<string>(() =>
                {
                    Console.WriteLine($"task A before sleep  Thread {Thread.CurrentThread.ManagedThreadId} ");
                    Thread.Sleep(6000);
                    Console.WriteLine($"AAAAAAAAAAAAAA after sleep  \rThread {Thread.CurrentThread.ManagedThreadId} ");

                    return "aaaaaaaaa";
                });

                Task<string> test2 = Task.Run<string>(() =>
                {
                    Console.WriteLine($"task B before sleep Thread {Thread.CurrentThread.ManagedThreadId} ");
                    Thread.Sleep(3000);
                    Console.WriteLine($"BBBBBBBBBBBBBB after sleep \rThread{Thread.CurrentThread.ManagedThreadId} ");

                    return "bbbbbbb";
                });

                List<Task<string>> listOCRTask = new List<Task<string>>() {
                    test1,
                    test2,
                };

                var data = await Task.WhenAll(listOCRTask);

                Console.WriteLine(data[0]);
                Console.WriteLine(data[1]);
            }

        }


        static void Main(string[] args)
        {
            //A a = new A();
            //a.RunThread();

            B b = new B();
            b.TestTask().Wait();

            Console.ReadKey();
        }
    }
}
