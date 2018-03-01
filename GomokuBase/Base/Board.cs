using System.Collections.Generic;
using GomokuBase.Serialize;

namespace GomokuBase.Base
{
    public class Board : ISerialize
    {
        public int Width_;
        public int Height_;
        public PointType[,] Points_;
        public Stack<BoardPoint> UndoList_;
        public GameMode Mode_;
        public PointType CurrentType_;

        public PointType this[int X, int Y]
        {
            get
            {
                if (X < 0 || X >= Width_)
                {
                    return PointType.None;
                }
                if (Y < 0 || Y >= Height_)
                {
                    return PointType.None;
                }
                return Points_[X, Y];
            }
            set
            {
                if (X < 0 || X >= Width_)
                {
                    return;
                }
                if (Y < 0 || Y >= Height_)
                {
                    return;
                }
                Points_[X, Y] = value;
            }
        }
        
        public Board(int Width, int Height, GameMode Mode)
        {
            Width_ = Width;
            Height_ = Height;
            New(Mode);
        }

        public void New(GameMode Mode)
        {
            Points_ = new PointType[Width_, Height_];
            UndoList_ = new Stack<BoardPoint>();
            Mode_ = Mode;
            CurrentType_ = PointType.Black;

            for (var Y = 0; Y < Height_; ++Y)
            {
                for (var X = 0; X < Width_; ++X)
                {
                    Points_[X, Y] = PointType.Blank;
                }
            }
        }
        
        public void Serialize(SerializeHelper Helper)
        {
            Helper.WriteInt32(Width_);
            Helper.WriteInt32(Height_);

            for (var Y = 0; Y < Height_; ++Y)
            {
                for (var X = 0; X < Width_; ++X)
                {
                    Helper.WriteInt8((byte)Points_[X, Y]);
                }
            }
        }

        public void Deserialize(DeserializeHelper Helper)
        {
            Width_ = Helper.ReadInt32();
            Height_ = Helper.ReadInt32();
            Points_ = new PointType[Width_, Height_];

            for (var Y = 0; Y < Height_; ++Y)
            {
                for (var X = 0; X < Width_; ++X)
                {
                    Points_[X, Y] = (PointType)Helper.ReadInt8();
                }
            }
        }

        public bool DoPass(int DownX, int DownY)
        {
            if (this[DownX, DownY] != PointType.Blank)
            {
                return false;
            }

            Points_[DownX, DownY] = CurrentType_;
            UndoList_.Push(new BoardPoint(DownX, DownY));

            if (Win(CurrentType_, DownX, DownY))
            {
                return true;
            }

            RevertCurrentType();
            return false;
        }

        public void UndoPass()
        {
            if (Mode_ == GameMode.PvC)
            {
                if (UndoList_.Count > 1)
                {
                    var Point1 = UndoList_.Pop();
                    var Point2 = UndoList_.Pop();
                    this[Point1.X_, Point1.Y_] = PointType.Blank;
                    this[Point2.X_, Point2.Y_] = PointType.Blank;
                }

            }
            else if (Mode_ == GameMode.PvP)
            {
                if (UndoList_.Count > 0)
                {
                    var Point = UndoList_.Pop();
                    this[Point.X_, Point.Y_] = PointType.Blank;
                    RevertCurrentType();
                }
            }
        }

        private void RevertCurrentType()
        {
            if (CurrentType_ == PointType.Black)
            {
                CurrentType_ = PointType.White;
            }
            else if (CurrentType_ == PointType.White)
            {
                CurrentType_ = PointType.Black;
            }
        }

        public bool Win(PointType ExpectType, int CheckX, int CheckY)
        {
            if (WinLtoR(ExpectType, CheckX, CheckY) || WinTtoB(ExpectType, CheckX, CheckY))
            {
                return true;
            }

            if (WinLTtoRB(ExpectType, CheckX, CheckY) || WinLBtoRT(ExpectType, CheckX, CheckY))
            {
                return true;
            }

            return false;
        }

        private bool WinLtoR(PointType ExpectType, int CheckX, int CheckY)
        {
            for (var IX = CheckX - 4; IX <= CheckX; ++IX)
            {
                var Count = 0;

                for (var Index = 0; Index < 5; ++Index)
                {
                    var Type = this[IX + Index, CheckY];

                    if (Type == PointType.None)
                    {
                        continue;
                    }

                    if (Type == ExpectType)
                    {
                        Count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Count >= 5)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WinTtoB(PointType ExpectType, int CheckX, int CheckY)
        {
            for (var IY = CheckY - 4; IY <= CheckY; ++IY)
            {
                var Count = 0;

                for (var Index = 0; Index < 5; ++Index)
                {
                    var Type = this[CheckX, IY + Index];

                    if (Type == PointType.None)
                    {
                        continue;
                    }

                    if (Type == ExpectType)
                    {
                        Count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Count >= 5)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WinLTtoRB(PointType ExpectType, int CheckX, int CheckY)
        {
            for (int IX = CheckX - 4, IY = CheckY - 4; IX <= CheckX && IY <= CheckY; ++IX, ++IY)
            {
                var Count = 0;

                for (var Index = 0; Index < 5; ++Index)
                {
                    var Type = this[IX + Index, IY + Index];

                    if (Type == PointType.None)
                    {
                        continue;
                    }

                    if (Type == ExpectType)
                    {
                        Count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Count >= 5)
                {
                    return true;
                }
            }

            return false;
        }

        private bool WinLBtoRT(PointType ExpectType, int CheckX, int CheckY)
        {
            for (int IX = CheckX - 4, IY = CheckY + 4; IX <= CheckX && IY >= CheckY; ++IX, --IY)
            {
                var Count = 0;

                for (var Index = 0; Index < 5; ++Index)
                {
                    PointType type = this[IX + Index, IY - Index];

                    if (type == PointType.None)
                    {
                        continue;
                    }

                    if (type == ExpectType)
                    {
                        Count++;
                    }
                    else
                    {
                        break;
                    }
                }

                if (Count >= 5)
                {
                    return true;
                }
            }

            return false;
        }
    }
}