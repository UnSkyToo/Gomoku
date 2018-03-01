using System;

namespace GomokuConsole.Util
{
    public static class MiscUtil
    {
        public static void WriteLeftLine()
        {
            Console.Write("--------------------");
        }

        public static void WriteRightLine()
        {
            WriteLeftLine();
            Console.WriteLine();
        }

        public static void WriteTitle()
        {
            WriteLeftLine();
            Console.Write("Gomoku Game 1.00");
            WriteRightLine();
        }

        public static void WriteOneLine()
        {
            WriteLeftLine();
            WriteRightLine();
        }
    }
}