using System.IO;
using System.Text;

namespace GomokuBase.Log
{
    public static class Logger
    {
        public enum Type
        {
            Log,
            Warning,
            Error
        }

        private static StreamWriter OutStream_ = null;

        static Logger()
        {
            //OutStream_ = new StreamWriter("log.txt", true, Encoding.UTF8);
        }

        public static void Write(string Msg, Type Level = Type.Log)
        {
        }
    }
}
