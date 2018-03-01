using System;
using System.Collections.Generic;
using GomokuBase.Base;

namespace GomokuCore.Core
{
    public static class CoreVersion
    {
        private static TupleParser Parser_ = new TupleParser();

        public static BoardPoint[] GetBestValuePoints(Board ExpectBoard, PointType ExpectType)
        {
            var Points = Parser_.GetBestPointsWithType(ExpectBoard, ExpectType);

            return Points.ToArray();
        }
    }
}
