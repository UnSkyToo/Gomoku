using System;

namespace GomokuBase.Base
{
    public static class Misc
    {
        private static readonly Random Rand_ = new Random((int)DateTime.Now.Ticks);

        public static int Range(int Min, int Max)
        {
            return Rand_.Next(Min, Max);
        }
    }
}
