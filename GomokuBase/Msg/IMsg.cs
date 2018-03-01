using GomokuBase.Base;
using GomokuBase.Serialize;

namespace GomokuBase.Msg
{
    public interface IMsg
    {
        int ID { get; }
    }

    public class MsgBase : IMsg
    {
        public int ID
        {
            get
            {
                var FullName = GetType().FullName;
                return (int) Crc32.Calculate(FullName);
            }
        }
    }
}