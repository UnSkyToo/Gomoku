using System;
using System.Collections.Generic;
using GomokuBase.Base;

namespace GomokuCore.Core
{
    #region Tuple Parser
    /// <summary>
    /// 棋组估值器
    /// </summary>
    public class TupleParser
    {
        #region Const
        /// <summary>
        /// 棋组价值表
        /// </summary>
        private static readonly int[,] TupleValueTable_ =
        {
            {
                0,          // None
                0,          // Both
                7,          // Blank
                35,         // B
                800,        // BB
                15000,      // BBB
                800000,     // BBBB
                999999999,  // BBBBB
                15,         // W
                400,        // WW
                1800,       // WWW
                100000,     // WWWW
                999999999,  // WWWWW
            },
            {
                0,          // None
                0,          // Both
                7,          // Blank
                35,         // W
                800,        // WW
                15000,      // WWW
                800000,     // WWWW
                999999999,  // WWWWW
                15,         // B
                400,        // BB
                1800,       // BBB
                100000,     // BBBB
                999999999,  // BBBBB
            },
        };
        #endregion

        #region Variable
        private List<TupleData>[] TupleMap_ = new List<TupleData>[2];
        #endregion

        #region Method

        #region Public Method
        public TupleParser()
        {
            for (var Index = 0; Index < TupleMap_.Length; ++Index)
            {
                TupleMap_[Index] = new List<TupleData>();
            }
        }

        /// <summary>
        /// 更新指定类型的棋组价值图
        /// </summary>
        /// <param name="ExpectBoard">棋盘数据</param>
        /// <param name="ExpectType">玩家当前的棋子类型</param>
        public void UpdateTupleMapWithType(Board ExpectBoard, PointType ExpectType)
        {
            TupleMap_[(int)ExpectType].Clear();

            #region Horizontal

            for (var Y = 0; Y < ExpectBoard.Height_; ++Y)
            {
                for (var X = 0; X <= ExpectBoard.Width_ - 5; ++X)
                {
                    var Data = new TupleData();
                    Data.Type_ = GetTupleTypeWithOrientation(ExpectBoard, ExpectType, X, Y, TupleOrientation.LtoR);
                    Data.Value_ = GetTupleValueWithType(Data.Type_, ExpectType);
                    Data.Points_ = new List<BoardPoint>();

                    for (var Index = 0; Index < 5; ++Index)
                    {
                        Data.Points_.Add(new BoardPoint(X + Index, Y));
                    }

                    TupleMap_[(int)ExpectType].Add(Data);
                }
            }

            #endregion

            #region Vertical

            for (var X = 0; X < ExpectBoard.Width_; ++X)
            {
                for (var Y = 0; Y <= ExpectBoard.Height_ - 5; ++Y)
                {
                    var Data = new TupleData();
                    Data.Type_ = GetTupleTypeWithOrientation(ExpectBoard, ExpectType, X, Y, TupleOrientation.TtoB);
                    Data.Value_ = GetTupleValueWithType(Data.Type_, ExpectType);
                    Data.Points_ = new List<BoardPoint>();

                    for (var Index = 0; Index < 5; ++Index)
                    {
                        Data.Points_.Add(new BoardPoint(X, Y + Index));
                    }

                    TupleMap_[(int)ExpectType].Add(Data);
                }
            }

            #endregion

            #region LT-RB Corner

            for (var X = 0; X <= ExpectBoard.Width_ - 5; ++X)
            {
                for (var Y = X; Y <= ExpectBoard.Height_ - 5; ++Y)
                {
                    var Data = new TupleData();
                    Data.Type_ = GetTupleTypeWithOrientation(ExpectBoard, ExpectType, X, Y, TupleOrientation.LTtoRB);
                    Data.Value_ = GetTupleValueWithType(Data.Type_, ExpectType);
                    Data.Points_ = new List<BoardPoint>();

                    for (var Index = 0; Index < 5; ++Index)
                    {
                        Data.Points_.Add(new BoardPoint(X + Index, Y + Index));
                    }

                    TupleMap_[(int)ExpectType].Add(Data);
                }
            }

            for (var Y = 0; Y <= ExpectBoard.Height_ - 5; ++Y)
            {
                for (var X = Y + 1; X <= ExpectBoard.Width_ - 5; ++X)
                {
                    var Data = new TupleData();
                    Data.Type_ = GetTupleTypeWithOrientation(ExpectBoard, ExpectType, X, Y, TupleOrientation.LTtoRB);
                    Data.Value_ = GetTupleValueWithType(Data.Type_, ExpectType);
                    Data.Points_ = new List<BoardPoint>();

                    for (var Index = 0; Index < 5; ++Index)
                    {
                        Data.Points_.Add(new BoardPoint(X + Index, Y + Index));
                    }

                    TupleMap_[(int)ExpectType].Add(Data);
                }
            }

            #endregion

            #region LB-RT Corner

            for (var X = 1; X <= ExpectBoard.Width_ - 5; ++X)
            {
                for (var Y = ExpectBoard.Height_ - 1; Y >= ExpectBoard.Height_ - X; --Y)
                {
                    var Data = new TupleData();
                    Data.Type_ = GetTupleTypeWithOrientation(ExpectBoard, ExpectType, X, Y, TupleOrientation.LBtoRT);
                    Data.Value_ = GetTupleValueWithType(Data.Type_, ExpectType);
                    Data.Points_ = new List<BoardPoint>();

                    for (var Index = 0; Index < 5; ++Index)
                    {
                        Data.Points_.Add(new BoardPoint(X + Index, Y - Index));
                    }

                    TupleMap_[(int)ExpectType].Add(Data);
                }
            }

            for (var X = 0; X <= ExpectBoard.Width_ - 5; ++X)
            {
                for (var Y = ExpectBoard.Height_ - X - 1; Y >= 5 - 1; --Y)
                {
                    var Data = new TupleData();
                    Data.Type_ = GetTupleTypeWithOrientation(ExpectBoard, ExpectType, X, Y, TupleOrientation.LBtoRT);
                    Data.Value_ = GetTupleValueWithType(Data.Type_, ExpectType);
                    Data.Points_ = new List<BoardPoint>();

                    for (var Index = 0; Index < 5; ++Index)
                    {
                        Data.Points_.Add(new BoardPoint(X + Index, Y - Index));
                    }

                    TupleMap_[(int)ExpectType].Add(Data);
                }
            }

            #endregion
        }

        /// <summary>
        /// 更新所有棋组价值图
        /// </summary>
        /// <param name="ExpectBoard">棋盘数据</param>
        public void UpdateTupleMap(Board ExpectBoard)
        {
            UpdateTupleMapWithType(ExpectBoard, PointType.Black);
            UpdateTupleMapWithType(ExpectBoard, PointType.White);
        }

        /// <summary>
        /// 获取指定坐标所有棋组价值
        /// </summary>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <param name="X">棋盘X坐标</param>
        /// <param name="Y">棋盘Y坐标</param>
        /// <returns>棋组价值</returns>
        public int GetPointTupleValueWithMap(PointType ExpectType, int X, int Y)
        {
            var Value = 0;

            foreach (var Data in TupleMap_[(int)ExpectType])
            {
                if (Data.ContainPoint(X, Y))
                {
                    Value += Data.Value_;
                }
            }

            return Value;
        }

        /// <summary>
        /// 获取棋盘所有坐标的棋组价值
        /// </summary>
        /// <param name="ExpectBoard">棋盘数据</param>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <returns>所有点的棋组价值</returns>
        public int[,] GetAllPointTupleValueWithMap(Board ExpectBoard, PointType ExpectType)
        {
            UpdateTupleMapWithType(ExpectBoard, ExpectType);
            var Value = new int[ExpectBoard.Width_, ExpectBoard.Height_];

            for (var X = 0; X < ExpectBoard.Width_; ++X)
            {
                for (var Y = 0; Y < ExpectBoard.Height_; ++Y)
                {
                    Value[X, Y] = GetPointTupleValueWithMap(ExpectType, X, Y);
                }
            }

            return Value;
        }

        /// <summary>
        /// 获取棋盘指定类型的棋最有价值的点的价值
        /// </summary>
        /// <param name="ExpectBoard">棋盘数据</param>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <returns>棋组价值</returns>
        public int GetBestPointValueWithType(Board ExpectBoard, PointType ExpectType)
        {
            var Value = GetAllPointTupleValueWithMap(ExpectBoard, ExpectType);
            var MaxValue = 0;

            for (var X = 0; X < ExpectBoard.Width_; ++X)
            {
                for (var Y = 0; Y < ExpectBoard.Height_; ++Y)
                {
                    if (MaxValue < Value[X, Y] && ExpectBoard[X, Y] == PointType.Blank)
                    {
                        MaxValue = Value[X, Y];
                    }
                }
            }

            return MaxValue;
        }

        /// <summary>
        /// 获取棋盘中指定类型的棋最有价值的所有点
        /// </summary>
        /// <param name="ExpectBoard">棋盘数据</param>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <returns>最优价值的点集合</returns>
        public List<BoardPoint> GetBestPointsWithType(Board ExpectBoard, PointType ExpectType)
        {
            var Value = GetAllPointTupleValueWithMap(ExpectBoard, ExpectType);
            var MaxValue = 0;
            var Points = new List<BoardPoint>();

            for (var X = 0; X < ExpectBoard.Width_; ++X)
            {
                for (var Y = 0; Y < ExpectBoard.Height_; ++Y)
                {
                    if (MaxValue < Value[X, Y] && ExpectBoard[X, Y] == PointType.Blank)
                    {
                        MaxValue = Value[X, Y];
                    }
                }
            }

            for (var X = 0; X < ExpectBoard.Width_; ++X)
            {
                for (var Y = 0; Y < ExpectBoard.Height_; ++Y)
                {
                    if (MaxValue == Value[X, Y] && ExpectBoard[X, Y] == PointType.Blank)
                    {
                        Points.Add(new BoardPoint(X, Y));
                    }
                }
            }

            return Points;
        }

        #endregion

        #region Private Method
        /// <summary>
        /// 获取指定类型的棋组价值
        /// </summary>
        /// <param name="Type">棋组类型</param>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <returns>棋组价值</returns>
        private int GetTupleValueWithType(TupleType Type, PointType ExpectType)
        {
            if (ExpectType == PointType.Blank)
            {
                return 0;
            }

            return TupleValueTable_[(int)ExpectType, (int)Type];
        }

        /// <summary>
        /// 获取指定数据的棋组类型
        /// </summary>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <param name="BaseType">棋组的棋子类型</param>
        /// <param name="Count">棋组棋子数量</param>
        /// <returns>棋组类型</returns>
        private TupleType GetTupleTypeWithData(PointType ExpectType, PointType BaseType, int Count)
        {
            if (Count == 0)
            {
                return TupleType.Blank;
            }

            if (ExpectType == BaseType)
            {
                //return (TupleType)((int)TupleType.Own_One + count - 1);

                if (Count == 1)
                {
                    return TupleType.Own_One;
                }
                else if (Count == 2)
                {
                    return TupleType.Own_Two;
                }
                else if (Count == 3)
                {
                    return TupleType.Own_Three;
                }
                else if (Count == 4)
                {
                    return TupleType.Own_Four;
                }
                else if (Count == 5)
                {
                    return TupleType.Own_Five;
                }
            }
            else
            {
                //return (TupleType)((int)TupleType.Other_One + count - 1);

                if (Count == 1)
                {
                    return TupleType.Other_One;
                }
                else if (Count == 2)
                {
                    return TupleType.Other_Two;
                }
                else if (Count == 3)
                {
                    return TupleType.Other_Three;
                }
                else if (Count == 4)
                {
                    return TupleType.Other_Four;
                }
                else if (Count == 5)
                {
                    return TupleType.Other_Five;
                }
            }

            return TupleType.None;
        }

        /// <summary>
        /// 获取指定方向的棋组类型
        /// </summary>
        /// <param name="ExpectBoard">棋盘数据</param>
        /// <param name="ExpectType">当前玩家的棋子类型</param>
        /// <param name="X">棋盘X坐标</param>
        /// <param name="Y">棋盘Y坐标</param>
        /// <param name="Orientation">棋组方向</param>
        /// <returns>棋组类型</returns>
        private TupleType GetTupleTypeWithOrientation(Board ExpectBoard, PointType ExpectType, int X, int Y, TupleOrientation Orientation)
        {
            var BaseType = ExpectBoard[X, Y];

            if (BaseType == PointType.None)
            {
                return TupleType.None;
            }

            var Count = 0;

            for (var Index = 0; Index < 5; ++Index)
            {
                var Type = PointType.Blank;

                switch (Orientation)
                {
                    case TupleOrientation.LtoR:
                        Type = ExpectBoard[X + Index, Y];
                        break;
                    case TupleOrientation.RtoL:
                        Type = ExpectBoard[X - Index, Y];
                        break;
                    case TupleOrientation.TtoB:
                        Type = ExpectBoard[X, Y + Index];
                        break;
                    case TupleOrientation.BtoT:
                        Type = ExpectBoard[X, Y - Index];
                        break;
                    case TupleOrientation.LTtoRB:
                        Type = ExpectBoard[X + Index, Y + Index];
                        break;
                    case TupleOrientation.RBtoLT:
                        Type = ExpectBoard[X - Index, Y - Index];
                        break;
                    case TupleOrientation.LBtoRT:
                        Type = ExpectBoard[X + Index, Y - Index];
                        break;
                    case TupleOrientation.RTtoLB:
                        Type = ExpectBoard[X - Index, Y + Index];
                        break;
                    default:
                        break;
                }

                if (Type == PointType.None)
                {
                    return TupleType.None;
                }
                else if (Type == PointType.Blank)
                {
                    continue;
                }
                else if (Type != BaseType)
                {
                    if (BaseType == PointType.Blank)
                    {
                        BaseType = Type;
                    }
                    else
                    {
                        return TupleType.Both;
                    }
                }

                Count++;
            }

            return GetTupleTypeWithData(ExpectType, BaseType, Count);
        }
        #endregion

        #endregion
    }
    #endregion
}