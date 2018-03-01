using System;
using GomokuBase.Event;
using GomokuConsole.Event;
using GomokuConsole.Network;
using GomokuConsole.Util;

namespace GomokuConsole.Page
{
    public class ConnectPage : PageBase
    {
        public enum ConnectState
        {
            EnterIP,
            Connecting
        }

        private ConnectState State_ = ConnectState.EnterIP;

        public ConnectPage()
        {
        }

        protected override void OnInitialize()
        {
            EventManager.Register<ConnectedEvent>(OnConnectedEvent);
            EventManager.Register<DisconnectedEvent>(OnDisconnectedEvent);
        }

        protected override void OnDeinitalize()
        {
            EventManager.UnRegister<ConnectedEvent>(OnConnectedEvent);
            EventManager.UnRegister<DisconnectedEvent>(OnDisconnectedEvent);
        }

        protected override void OnUpdate(float DeltaTime)
        {
        }

        protected override void OnRender()
        {
            MiscUtil.WriteTitle();

            if (State_ == ConnectState.EnterIP)
            {
                Console.Write("Please enter server ip:");
                var HostIP = Console.ReadLine();
                ConnectHostServer(HostIP, 24001);
            }
            else if (State_ == ConnectState.Connecting)
            {
                Console.WriteLine("Connecting...");
            }
        }

        protected override void OnKeyDown(ConsoleKey Key)
        {
        }

        private void ConnectHostServer(string HostIP, int HostPort)
        {
            Console.WriteLine($"Connect Host Server : {HostIP}:{HostPort}");
            NetManager.Connect(HostIP, HostPort);
            State_ = ConnectState.Connecting;
            Repaint();
        }

        private void OnDisconnectedEvent(DisconnectedEvent Event)
        {
            State_ = ConnectState.EnterIP;
            Repaint();
        }

        private void OnConnectedEvent(ConnectedEvent Event)
        {
            PageManager.Change<MenuPage>();
        }
    }
}