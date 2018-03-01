using System.Collections.Generic;
using GomokuBase.Base;

namespace GomokuCore.Core
{
    /// <summary>
    /// 棋组类型
    /// </summary>
    public enum TupleType
    {
        None = 0,
        Both = 1,
        Blank = 2,
        Own_One = 3,
        Own_Two = 4,
        Own_Three = 5,
        Own_Four = 6,
        Own_Five = 7,
        Other_One = 8,
        Other_Two = 9,
        Other_Three = 10,
        Other_Four = 11,
        Other_Five = 12,
    }

    /// <summary>
    /// 棋组的方向
    /// </summary>
    public enum TupleOrientation
    {
        LtoR,
        RtoL,
        TtoB,
        BtoT,
        LTtoRB,
        RBtoLT,
        LBtoRT,
        RTtoLB,
    }

    public struct TupleData
    {
        public List<BoardPoint> Points_;
        public TupleType Type_;
        public int Value_;

        public TupleData(TupleType Type, int Value)
        {
            Type_ = Type;
            Value_ = Value;
            Points_ = new List<BoardPoint>();
        }

        public bool ContainPoint(BoardPoint Point)
        {
            return ContainPoint(Point.X_, Point.Y_);
        }

        public bool ContainPoint(int X, int Y)
        {
            if (Points_ == null)
            {
                return false;
            }

            foreach (var Point in Points_)
            {
                if (Point.X_ == X && Point.Y_ == Y)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
