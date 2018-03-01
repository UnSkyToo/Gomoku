using System;
using GomokuBase.Base;
using GomokuBase.Msg;
using GomokuBase.Network;

namespace GomokuCore.Session
{
    public abstract class SessionBase
    {
        public int ID { get; private set; }

        public bool IsAlive { get; private set; }

        private NetClient Client_;

        public SessionBase(NetClient Client)
        {
            ID = IDGenerator.Next();
            Client_ = Client;
            IsAlive = true;
        }

        public void Initialize()
        {
            Client_.OnDisconnected += OnDisconnected;
            RegisterAllMsg();
        }

        public void Deinitialize()
        {
            Client_.Disconnect();
            UnRegisterAllMsg();
        }

        private void OnDisconnected()
        {
            Client_.OnDisconnected -= OnDisconnected;
            IsAlive = false;
        }

        public virtual void Update(float DeltaTime)
        {
            Client_.Update(DeltaTime);
        }

        public void SendMsg<T>(T Msg) where T : IMsg
        {
            Client_.SendMsg(Msg);
        }

        public void RegisterMsg<T>(Action<T> Callback) where T : IMsg
        {
            Client_.RegisterMsgHandler(Callback);
        }

        public void UnRegisterMsg<T>(Action<T> Callback) where T : IMsg
        {
            Client_.UnRegisterMsgHandler(Callback);
        }

        protected abstract void RegisterAllMsg();

        protected abstract void UnRegisterAllMsg();
    }
}