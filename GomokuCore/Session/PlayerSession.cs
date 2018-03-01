using System.Collections.Generic;
using GomokuBase.Base;
using GomokuBase.Msg;
using GomokuBase.Network;
using GomokuCore.Core;

namespace GomokuCore.Session
{
    public class PlayerSession : SessionBase
    {
        private Dictionary<int, Board> BoardList_ = new Dictionary<int, Board>();

        public PlayerSession(NetClient Client)
            : base(Client)
        {
        }

        protected override void RegisterAllMsg()
        {
            RegisterMsg<C2S_StartNewGame>(OnC2S_StartNewGame);
            RegisterMsg<C2S_JoinGame>(OnC2S_JoinGame);
            RegisterMsg<C2S_GetBestValuePoints>(OnC2S_GetBestValuePoints);
            RegisterMsg<C2S_DoPass>(OnC2S_DoPass);
            RegisterMsg<C2S_UndoPass>(OnC2S_UndoPass);
        }

        protected override void UnRegisterAllMsg()
        {
            UnRegisterMsg<C2S_StartNewGame>(OnC2S_StartNewGame);
            UnRegisterMsg<C2S_JoinGame>(OnC2S_JoinGame);
            UnRegisterMsg<C2S_GetBestValuePoints>(OnC2S_GetBestValuePoints);
            UnRegisterMsg<C2S_DoPass>(OnC2S_DoPass);
            UnRegisterMsg<C2S_UndoPass>(OnC2S_UndoPass);
        }

        private Board GetBoardWithID(int BoardID)
        {
            if (BoardList_.ContainsKey(BoardID))
            {
                return BoardList_[BoardID];
            }

            return null;
        }

        private void OnC2S_StartNewGame(C2S_StartNewGame Msg)
        {
            var NewBoard = new Board(Msg.Width_, Msg.Height_, Msg.Mode_);
            var NewID = IDGenerator.Next();
            BoardList_.Add(NewID, NewBoard);

            var MsgSend = new S2C_StartNewGame();
            MsgSend.BoardID_ = NewID;
            MsgSend.Width_ = Msg.Width_;
            MsgSend.Height_ = Msg.Height_;
            MsgSend.Mode_ = Msg.Mode_;
            SendMsg(MsgSend);
        }

        private void OnC2S_JoinGame(C2S_JoinGame Msg)
        {
            var CurrentBoard = GetBoardWithID(Msg.BoardID_);
            var MsgSend = new S2C_JoinGame();
            MsgSend.BoardID_ = Msg.BoardID_;

            if (CurrentBoard != null)
            {
                MsgSend.Succeed_ = true;
            }
            else
            {
                MsgSend.Succeed_ = false;
            }

            SendMsg(MsgSend);
        }

        private void OnC2S_GetBestValuePoints(C2S_GetBestValuePoints Msg)
        {
            var CurrentBoard = GetBoardWithID(Msg.BoardID_);
            var MsgSend = new S2C_GetBestValuePoints();
            MsgSend.BoardID_ = Msg.BoardID_;

            if (CurrentBoard != null)
            {
                var Points = CoreVersion.GetBestValuePoints(CurrentBoard, CurrentBoard.CurrentType_);
                MsgSend.Points_ = Points;
            }
            else
            {
                MsgSend.Points_ = new BoardPoint[0];
            }

            SendMsg(MsgSend);
        }

        private void OnC2S_DoPass(C2S_DoPass Msg)
        {
            var CurrentBoard = GetBoardWithID(Msg.BoardID_);
            var MsgSend = new S2C_DoPass();
            MsgSend.BoardID_ = Msg.BoardID_;
            MsgSend.X_ = Msg.X_;
            MsgSend.Y_ = Msg.Y_;

            if (CurrentBoard != null)
            {
                CurrentBoard.DoPass(Msg.X_, Msg.Y_);
                MsgSend.Succeed_ = true;
            }
            else
            {
                MsgSend.Succeed_ = false;
            }

            SendMsg(MsgSend);
        }

        private void OnC2S_UndoPass(C2S_UndoPass Msg)
        {
            var CurrentBoard = GetBoardWithID(Msg.BoardID_);
            var MsgSend = new S2C_UndoPass();
            MsgSend.BoardID_ = Msg.BoardID_;

            if (CurrentBoard != null)
            {
                CurrentBoard.UndoPass();
                MsgSend.Succeed_ = true;
            }
            else
            {
                MsgSend.Succeed_ = false;
            }

            SendMsg(MsgSend);
        }
    }
}