using System;
using System.Threading;
using System.Diagnostics;

namespace GomokuConsole
{
    public class ConsoleGameBase
    {
        public ConsoleGameBase()
        {
            Console.Title = "ConsoleGame";
            Console.CursorVisible = false;
        }

        public int Run()
        {
            var TimeWatch = new Stopwatch();
            var ElapsedTime = 0.0f;
            var ExpectElapsedTime = 1.0f / 60.0f;

            while (true)
            {
                TimeWatch.Restart();

                if (Console.KeyAvailable)
                {
                    var KeyInfo = Console.ReadKey(true);

                    if (KeyInfo.Key == ConsoleKey.Escape)
                    {
                        break;
                    }

                    OnKeyDown(KeyInfo.Key);
                }

                Update(ElapsedTime);
                Render();

                if (ElapsedTime < ExpectElapsedTime)
                {
                    Thread.Sleep((int)((ExpectElapsedTime - ElapsedTime) * 1000.0f));
                }

                TimeWatch.Stop();
                ElapsedTime = (float)TimeWatch.ElapsedMilliseconds / 1000.0f;
            }

            return 0;
        }

        protected virtual void Update(float DeltaTime)
        {
        }

        protected virtual void Render()
        {
        }

        protected virtual void OnKeyDown(ConsoleKey Key)
        {
        }
    }
}
