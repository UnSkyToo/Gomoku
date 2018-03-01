using System;
using GomokuBase.Base;
using GomokuBase.Event;
using GomokuBase.Msg;
using GomokuConsole.Event;
using GomokuConsole.Network;
using GomokuConsole.Util;

namespace GomokuConsole.Page
{
    public class MenuPage : PageBase
    {
        private enum MenuState
        {
            Main = 0,
            JoinGame = 1,
            Help = 2,
            WaitPlayer = 3,
        }

        private MenuState State_ = MenuState.Main;

        public MenuPage()
        {
        }

        protected override void OnInitialize()
        {
            EventManager.Register<DisconnectedEvent>(OnDisconnectedEvent);
            NetManager.RegisterMsg<S2C_StartNewGame>(OnS2C_StartNewGame);
        }

        protected override void OnDeinitalize()
        {
            EventManager.UnRegister<DisconnectedEvent>(OnDisconnectedEvent);
            NetManager.UnRegisterMsg<S2C_StartNewGame>(OnS2C_StartNewGame);
        }

        protected override void OnUpdate(float DeltaTime)
        {
        }

        protected override void OnRender()
        {
            MiscUtil.WriteTitle();

            if (State_ == MenuState.Main)
            {
                Console.WriteLine("1. Create PvC");
                Console.WriteLine("2. Create PvP");
                Console.WriteLine("3. Join Game");
                Console.WriteLine("4. Help");
            }
            else if (State_ == MenuState.JoinGame)
            {
                Console.Write("Please enter board id:");
                var BoardID = Console.ReadLine();
                JoinGame(int.Parse(BoardID));
            }
            else if (State_ == MenuState.Help)
            {
                Console.WriteLine("H - help");
                Console.WriteLine("Esc - exit game");
                Console.WriteLine("Backspace - back to menu");
            }
            else if (State_ == MenuState.WaitPlayer)
            {
                Console.WriteLine("Wait another player");
            }
        }

        protected override void OnKeyDown(ConsoleKey Key)
        {
            if (Key == ConsoleKey.D1)
            {
                StartNewGame(GameMode.PvC);
                Repaint();
            }
            else if (Key == ConsoleKey.D2)
            {
                StartNewGame(GameMode.PvP);
                Repaint();
            }
            else if (Key == ConsoleKey.D3)
            {
                State_ = MenuState.JoinGame;
                Repaint();
            }
            else if (Key == ConsoleKey.D4)
            {
                State_ = MenuState.Help;
                Repaint();
            }
            else if (Key == ConsoleKey.Backspace && State_ == MenuState.Help)
            {
                State_ = MenuState.Main;
                Repaint();
            }
        }

        private void OnDisconnectedEvent(DisconnectedEvent Event)
        {
            PageManager.Change<ConnectPage>();
        }

        private void StartNewGame(GameMode Mode)
        {
            var MsgSend = new C2S_StartNewGame
            {
                Width_ = 15,
                Height_ = 15,
                Mode_ = Mode
            };

            NetManager.SendMsg(MsgSend);
        }

        private void OnS2C_StartNewGame(S2C_StartNewGame Msg)
        {
            if (Msg.Mode_ == GameMode.PvC)
            {
                PageManager.Change<BoardPage>(Msg.BoardID_, Msg.Width_, Msg.Height_, Msg.Mode_);
            }
            else if (Msg.Mode_ == GameMode.PvP)
            {
                State_ = MenuState.WaitPlayer;
                Repaint();
            }
        }

        private void JoinGame(int BoardID)
        {
        }
    }
}