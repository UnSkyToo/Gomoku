using System;
using GomokuBase.Msg;
using GomokuBase.Event;
using GomokuBase.Network;
using GomokuManual.Event;

namespace GomokuManual.Network
{
    public static class NetManager
    {
        private static NetClient Client_ = new NetClient("Console Client");

        public static bool Connected
        {
            get { return Client_.Connected; }
        }

        static NetManager()
        {
            Client_.OnConnected += OnClientConnected;
        }

        public static void Connect(string IP, int Port)
        {
            if (Connected)
            {
                return;
            }

            Client_.Connect(IP, Port);
        }

        public static void Disconnect()
        {
            if (!Connected)
            {
                return;
            }

            Client_.Disconnect();
            Client_ = null;
        }

        public static void Update(float DeltaTime)
        {
            Client_.Update(DeltaTime);
        }

        public static void RegisterMsg<T>(Action<T> Handler) where T : IMsg
        {
            Client_.RegisterMsgHandler(Handler);
        }

        public static void UnRegisterMsg<T>(Action<T> Handler) where T : IMsg
        {
            Client_.UnRegisterMsgHandler(Handler);
        }

        public static void SendMsg<T>(T Msg) where T : IMsg
        {
            Client_.SendMsg(Msg);
        }

        private static void OnClientConnected()
        {
            EventManager.Send<ConnectedEvent>();
        }
    }
}