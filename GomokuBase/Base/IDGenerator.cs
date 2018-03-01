namespace GomokuBase.Base
{
    public static class IDGenerator
    {
        private static int ID_ = 1;

        public static int Next()
        {
            return ID_++;
        }
    }
}
