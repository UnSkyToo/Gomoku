using System;
using System.Collections.Generic;
using GomokuBase.Msg;
using GomokuBase.Network;

namespace GomokuCore.Session
{
    public static class SessionManager
    {
        private static readonly NetClient Server_ = new NetClient("Server Host");
        private static readonly Dictionary<int, SessionBase> Sessions_ = new Dictionary<int, SessionBase>();
        private static readonly List<SessionBase> InactiveSessions_ = new List<SessionBase>();

        static SessionManager()
        {
        }

        public static void Initialize()
        {
            Server_.OnAccept += OnAccept;
        }

        public static void Deinitialize()
        {
            Server_.OnAccept -= OnAccept;
            Server_.Disconnect();

            foreach (var Session in Sessions_)
            {
                Session.Value.Deinitialize();
            }
            Sessions_.Clear();
        }

        public static void Listen(int ListenPort)
        {
            Server_.Listen(ListenPort, 5);
            Console.WriteLine("Listen Port : " + ListenPort);
        }

        public static void Update(float DeltaTime)
        {
            Server_.Update(DeltaTime);

            foreach (var Session in Sessions_)
            {
                Session.Value.Update(DeltaTime);

                if (!Session.Value.IsAlive)
                {
                    InactiveSessions_.Add(Session.Value);
                }
            }

            if (InactiveSessions_.Count > 0)
            {
                foreach (var Session in InactiveSessions_)
                {
                    Session.Deinitialize();
                    Sessions_.Remove(Session.ID);
                }

                InactiveSessions_.Clear();
            }
        }

        public static void Send<T>(int SessionID, T Msg) where T : IMsg
        {
            if (Sessions_.ContainsKey(SessionID))
            {
                Sessions_[SessionID].SendMsg(Msg);
            }
        }

        public static void SendToAll<T>(T Msg) where T : IMsg
        {
            foreach (var Session in Sessions_)
            {
                Session.Value.SendMsg(Msg);
            }
        }

        private static void OnAccept(NetClient Client)
        {
            var Session = new PlayerSession(Client);
            Session.Initialize();
            Sessions_.Add(Session.ID, Session);
            Console.WriteLine("Accept Client : " + Client.IP + "-" + Client.Port);
        }
    }
}
