using System;
using System.Threading;


namespace GameServer
{
    class Program
    {
        private static bool isRunning = false;
        static void Main(string[] args)
        {
            Console.Title = "GameServer";
            isRunning = true;

            Thread mainThread = new Thread(new ThreadStart(MainThread));
            mainThread.Start();

            Server.Start(Convert.ToInt32(Console.ReadLine()), 5, 26950);
        }

        public static void MainThread()
        {
            Console.WriteLine($"Main Thread started Running");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning)
            {
                while(_nextLoop < DateTime.Now)
                {
                    GameLogic.Update();
                    _nextLoop = _nextLoop.AddMilliseconds(Constans.MS_PER_TICK);
                    if(_nextLoop > DateTime.Now)
                    {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}

