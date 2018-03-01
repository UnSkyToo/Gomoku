using System;
using System.Threading;
using GomokuBase.Base;
using GomokuCore.Core;
using GomokuCore.Session;

namespace GomokuCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start Gomoku Server...");
            SessionManager.Initialize();
            SessionManager.Listen(24001);

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

                SessionManager.Update(0.1f);
                Thread.Sleep(100);
            }
            SessionManager.Deinitialize();

            Console.WriteLine("Quit...");
        }
    }
}
