using GomokuBase.Base;
using GomokuBase.Serialize;

namespace GomokuBase.Msg
{
    public class C2S_StartNewGame : MsgBase
    {
        public GameMode Mode_;
        public int Width_;
        public int Height_;
    }

    public class S2C_StartNewGame : MsgBase
    {
        public int BoardID_;
        public GameMode Mode_;
        public int Width_;
        public int Height_;
    }

    public class C2S_JoinGame : MsgBase
    {
        public int BoardID_;
    }

    public class S2C_JoinGame : MsgBase
    {
        public int BoardID_;
        public bool Succeed_;
    }

    public class C2S_GetBestValuePoints : MsgBase
    {
        public int BoardID_;
    }

    public class S2C_GetBestValuePoints : MsgBase
    {
        public int BoardID_;
        public BoardPoint[] Points_;
    }

    public class C2S_DoPass : MsgBase
    {
        public int BoardID_;
        public int X_;
        public int Y_;
    }

    public class S2C_DoPass : MsgBase
    {
        public int BoardID_;
        public int X_;
        public int Y_;
        public bool Succeed_;
    }

    public class C2S_UndoPass : MsgBase
    {
        public int BoardID_;
    }

    public class S2C_UndoPass : MsgBase
    {
        public int BoardID_;
        public bool Succeed_;
    }
}
