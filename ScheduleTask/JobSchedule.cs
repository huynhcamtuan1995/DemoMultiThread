using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ScheduleTask
{
    public class JobSchedule
    {
        private static readonly SemaphoreSlim ScheduleSemaphore = new SemaphoreSlim(10, 10);
        internal static Timer ScheduleTimer;
        static JobSchedule()
        {
            TimerRun();
        }

        /// <summary>
        /// starting timer
        /// </summary>
        public static void TimerRun()
        {
            ScheduleTimer = new Timer(1000 * 10);
            ScheduleTimer.Elapsed += ProcessOnExecute;
            ScheduleTimer.AutoReset = true;
            ScheduleTimer.Start();
        }

        /// <summary>
        /// timer run every 5s, to progress to empty task to break
        /// </summary>
        /// <param name="source"></param>
        /// <param name="e"></param>
        private static void ProcessOnExecute(object source, ElapsedEventArgs e)
        {
            //stop to run until end of list data
            ScheduleTimer.Stop();

            try
            {
                Next:
                {
                    //select top in list (request or db) to process
                    //where some condition

                    List<string> listRequests = new List<string>();
                    if (listRequests.Count == 0)
                    {
                        goto End;
                    }

                    foreach (string lead in listRequests)
                    {
                        // Set Stage is Inprogress
                        //UpdateState(lead, LeadStateEnum.Inprogress);

                        // Split Thread
                        Thread thread = CreateThread(
                            ScheduleSemaphore,
                            () =>
                            {
                                ProcessTask(lead);
                            });
                        thread.Start();
                    }

                    //repeat 
                    goto Next;
                }


                End:
                {
                    return;
                }
            }
            catch (Exception exception)
            {
                //log ex
            }
            finally
            {
                //start timer after stop
                ScheduleTimer.Start();
            }
        }

        /// <summary>
        /// managed max thread to progress by semaphore
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
        /// function process task
        /// </summary>
        /// <param name="model"></param>
        private static void ProcessTask(string model)
        {
            //Do process model
        }
    }

}
