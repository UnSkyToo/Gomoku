using System;
using System.Threading;
using GomokuManual.Network;

namespace GomokuManual
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start...");
            
            NetManager.Connect("127.0.0.1", 24001);

            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var KeyInfo = Console.ReadKey(true);

                    if (KeyInfo.Key == ConsoleKey.Escape)
                    {
                        break;
                    }
                }

                NetManager.Update(0.1f);
                Thread.Sleep(100);
            }

            Console.WriteLine("Wait Quit...");
        }
    }
}