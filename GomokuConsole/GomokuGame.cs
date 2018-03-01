using System;
using GomokuConsole.Page;
using GomokuConsole.Network;

namespace GomokuConsole
{
    public class GomokuGame : ConsoleGameBase
    {
        public GomokuGame()
        {
            Console.Title = "Gomoku Game";
            PageManager.Change<ConnectPage>();
        }

        protected override void Update(float DeltaTime)
        {
            PageManager.Update(DeltaTime);
            NetManager.Update(DeltaTime);
        }

        protected override void Render()
        {
            PageManager.Render(); 
        }

        protected override void OnKeyDown(ConsoleKey Key)
        {
            PageManager.OnKeyDown(Key);
        }
    }
}
