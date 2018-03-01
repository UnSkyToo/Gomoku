using System;
using GomokuBase.Base;
using GomokuBase.Event;
using GomokuBase.Msg;
using GomokuConsole.Event;
using GomokuConsole.Network;

namespace GomokuConsole.Page
{
    public class BoardPage : PageBase
    {
        private enum BoardPageState
        {
            Main,
            KeyDown1,
            Win,
            Lose,
            WaitMsg,
        }

        private readonly int BoardID_ = -1;
        private readonly Board Board_ = null;
        private PointType CurrentType_;
        private BoardPageState State_;
        private int ChessDownX_ = -1;
        private int ChessDownY_ = -1;
        private bool IsAutoDown_ = false;

        public BoardPage(params object[] Params)
        {
            BoardID_ = (int)Params[0];
            Console.Title = "Gomoku Game - " + BoardID_;
            Board_ = new Board((int)Params[1], (int)Params[2], (GameMode)Params[3]);
            CurrentType_ = PointType.Black;
        }

        protected override void OnInitialize()
        {
            EventManager.Register<DisconnectedEvent>(OnDisconnectedEvent);
            NetManager.RegisterMsg<S2C_GetBestValuePoints>(OnS2C_GetBestValuePoints);
            NetManager.RegisterMsg<S2C_DoPass>(OnS2C_DoPass);
            ChangeState(BoardPageState.Main);
        }

        protected override void OnDeinitalize()
        {
            EventManager.UnRegister<DisconnectedEvent>(OnDisconnectedEvent);
            NetManager.UnRegisterMsg<S2C_GetBestValuePoints>(OnS2C_GetBestValuePoints);
            NetManager.UnRegisterMsg<S2C_DoPass>(OnS2C_DoPass);
        }

        protected override void OnUpdate(float DeltaTime)
        {
        }

        protected override void OnRender()
        {
            Console.WriteLine("   1 2 3 4 5 6 7 8 9 A B C D E F");
            for (var Y = -1; Y < Board_.Height_ + 1; ++Y)
            {
                for (var X = -1; X < Board_.Width_ + 1; ++X)
                {
                    if (Y == -1)
                    {
                        if (X == -1)
                        {
                            Console.Write(" ┌");
                        }
                        else if (X == Board_.Width_)
                        {
                            Console.WriteLine('┐');
                        }
                        else
                        {
                            Console.Write('┬');
                        }
                    }
                    else if (Y == Board_.Height_)
                    {
                        if (X == -1)
                        {
                            Console.Write(" └");
                        }
                        else if (X == Board_.Width_)
                        {
                            Console.WriteLine('┘');
                        }
                        else
                        {
                            Console.Write('┴');
                        }
                    }
                    else
                    {
                        if (X == -1)
                        {
                            Console.Write("{0:X}├", Y + 1);
                        }
                        else if (X == Board_.Width_)
                        {
                            Console.WriteLine('┤');
                        }
                        else
                        {
                            if (Board_[X, Y] == PointType.White)
                            {
                                Console.Write('○');
                            }
                            else if (Board_[X, Y] == PointType.Black)
                            {
                                Console.Write('●');
                            }
                            else
                            {
                                Console.Write('┼');
                            }
                        }
                    }
                }
            }

            if (State_ == BoardPageState.Main)
            {
                Console.WriteLine("Press the '1-F' key to determine the 'X' coordinates");
            }
            else if (State_ == BoardPageState.KeyDown1)
            {
                Console.WriteLine("Press the '1-F' key to determine the 'Y' coordinates, or press 'Backspace' to go back to the previous step.");
            }
            else if (State_ == BoardPageState.Win)
            {
                Console.WriteLine("You Win! press N to new game");
            }
            else if (State_ == BoardPageState.Lose)
            {
                Console.WriteLine("You Lose! press N to new game");
            }
        }

        protected override void OnKeyDown(ConsoleKey Key)
        {
            if (State_ != BoardPageState.WaitMsg)
            {
                if (State_ != BoardPageState.Win && State_ != BoardPageState.Lose)
                {
                    if (IsCoordinateKey(Key))
                    {
                        if (State_ == BoardPageState.Main)
                        {
                            ChessDownX_ = CoordinateKeyToNumber(Key);
                            ChangeState(BoardPageState.KeyDown1);
                            Repaint();
                        }
                        else if (State_ == BoardPageState.KeyDown1)
                        {
                            CurrentType_ = PointType.Black;
                            ChessDownY_ = CoordinateKeyToNumber(Key);
                            OnChessDown(ChessDownX_, ChessDownY_);
                            ChangeState(BoardPageState.Main);
                            Repaint();
                        }
                    }
                    else if (Key == ConsoleKey.Z)
                    {
                        Board_.UndoPass();
                    }
                }

                if (Key == ConsoleKey.N)
                {
                    State_ = BoardPageState.Main;
                    Board_.New(Board_.Mode_);
                    Repaint();
                }
                else if (Key == ConsoleKey.I)
                {
                    if (!IsAutoDown_)
                    {
                        IsAutoDown_ = true;
                        DoPassBestValuePointWithType();
                    }
                    else
                    {
                        IsAutoDown_ = false;
                    }
                }
                else if (Key == ConsoleKey.Backspace)
                {
                    if (State_ == BoardPageState.KeyDown1)
                    {
                        ChessDownX_ = -1;
                        ChangeState(BoardPageState.Main);
                        Repaint();
                    }
                    else
                    {
                        PageManager.Change<MenuPage>();
                    }
                }
            }
        }

        private void ChangeState(BoardPageState State)
        {
            State_ = State;
        }

        private bool IsCoordinateKey(ConsoleKey Key)
        {
            if ((Key >= ConsoleKey.D1 && Key <= ConsoleKey.D9) ||
                (Key >= ConsoleKey.A && Key <= ConsoleKey.F) ||
                (Key >= ConsoleKey.NumPad1 && Key <= ConsoleKey.NumPad9))
            {
                return true;
            }

            return false;
        }

        private int CoordinateKeyToNumber(ConsoleKey Key)
        {
            if (Key >= ConsoleKey.D1 && Key <= ConsoleKey.D9)
            {
                return Key - ConsoleKey.D1;
            }
            if (Key >= ConsoleKey.NumPad1 && Key <= ConsoleKey.NumPad9)
            {
                return Key - ConsoleKey.NumPad1;
            }

            if (Key >= ConsoleKey.A && Key <= ConsoleKey.F)
            {
                return Key - ConsoleKey.A + 9;
            }

            return -1;
        }

        private void OnDisconnectedEvent(DisconnectedEvent Event)
        {
            PageManager.Change<ConnectPage>();
        }

        private void OnChessDown(int X, int Y)
        {
            if (Board_[X, Y] != PointType.Blank)
            {
                Console.WriteLine("{0},{1} not blank point", X, Y);
                return;
            }

            if (Board_.DoPass(X, Y))
            {
                ChangeState(BoardPageState.Win);
            }
            else
            {
                var MsgSend = new C2S_DoPass
                {
                    BoardID_ = BoardID_,
                    X_ = X,
                    Y_ = Y
                };
                NetManager.SendMsg(MsgSend);
                ChangeState(BoardPageState.WaitMsg);
            }

        }

        private void OnS2C_DoPass(S2C_DoPass Msg)
        {
            ChangeState(BoardPageState.Main);
            if (Msg.Succeed_)
            {
                if (CurrentType_ == PointType.Black)
                {
                    DoPassBestValuePointWithType();
                }
            }
            else
            {
                Board_.UndoPass();
            }
        }

        private void DoPassBestValuePointWithType()
        {
            var Msg = new C2S_GetBestValuePoints();
            Msg.BoardID_ = BoardID_;
            NetManager.SendMsg(Msg);
            ChangeState(BoardPageState.WaitMsg);
        }

        private void OnS2C_GetBestValuePoints(S2C_GetBestValuePoints Msg)
        {
            ChangeState(BoardPageState.Main);
            if (Msg.Points_.Length > 0)
            {
                var Index = Misc.Range(0, Msg.Points_.Length);
                var Point = Msg.Points_[Index];
                CurrentType_ = PointType.White;
                OnChessDown(Point.X_, Point.Y_);
                Repaint();
            }
            else
            {
                Console.WriteLine("Server can't calc a best point");
            }
        }
    }
}