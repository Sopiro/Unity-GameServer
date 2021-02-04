using System;
using System.Collections.Generic;
using System.Threading;

namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;
        private static readonly List<DateTime> times = new List<DateTime>();

        static void Main(string[] args)
        {
            Console.Title = "Game Server";
            isRunning = true;

            Server.Start(10, 1234);

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();
        }

        private static void MainThread()
        {
            Console.WriteLine($"Main thread started. Running at {Constants.TICKS_PER_SECOND} ticks per second");

            DateTime nextUpdate = DateTime.Now;

            while (isRunning)
            {
                while (nextUpdate < DateTime.Now)
                {
                    GameLogic.Update();

                    nextUpdate = nextUpdate.AddMilliseconds(Constants.MS_PER_TICK);

                    times.Add(DateTime.Now);

                    while (times.Count > 0 && DateTime.Now.Subtract(times[0]).TotalMilliseconds > 1000.0f)
                        times.RemoveAt(0);

                    if (nextUpdate > DateTime.Now)
                        Thread.Sleep(nextUpdate - DateTime.Now);
                }

                //Console.WriteLine($"Server tick rates: {times.Count}");
            }
        }
    }
}
