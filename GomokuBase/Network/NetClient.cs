using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using GomokuBase.Base;
using GomokuBase.Log;
using GomokuBase.Msg;
using GomokuBase.Serialize;

namespace GomokuBase.Network
{
    public class NetClient
    {
        public event Action<NetClient> OnAccept;
        public event Action OnConnected; 
        public event Action OnDisconnected;

        public enum ConnectedCode
        {
            None = 0,
            Succeeded = 1,
            Failed = 2,
            Disconnect = 3
        }

        public string Name { get; private set; }
        
        public string IP { get; private set; }
        
        public int Port { get; private set; }

        public bool Connected
        {
            get { return Socket_ != null && Socket_.Connected; }
        }

        private Socket Socket_ = null;
        private int ConnectedStateCode_ = (int)ConnectedCode.None;
        private readonly ObjectPool<SocketAsyncEventArgs> AsyncEventPool_ = null;

        private readonly Dictionary<int, string> MsgID2Name_ = new Dictionary<int, string>();
        private readonly Dictionary<int, MsgListenerBase> MsgHandler_ = new Dictionary<int, MsgListenerBase>();

        private readonly object RecvQueueLock_ = new object();
        private readonly Queue<MsgDecoder> RecvQueue_ = new Queue<MsgDecoder>();
        private readonly Queue<MsgEncoderBase> SendQueue_ = new Queue<MsgEncoderBase>();

        private readonly float SendInterval_ = 0.2f;
        private float LastSendTime_ = 0.0f;

        public NetClient(string ClientName)
        {
            Name = ClientName;
            AsyncEventPool_ = new ObjectPool<SocketAsyncEventArgs>();
            AsyncEventPool_.OnSpawn += OnAsyncEventSpawn;
            AsyncEventPool_.OnRecycle += OnAsyncEventRecycle;
        }

        public NetClient(string ClientName, Socket ClientSocket)
            : this(ClientName)
        {
            Socket_ = ClientSocket;
            IP = (Socket_.RemoteEndPoint as IPEndPoint).Address.ToString();
            Port = (Socket_.RemoteEndPoint as IPEndPoint).Port;
        }

        public bool Connect(string ConnectIP, int ConnectPort)
        {
            if (Connected)
            {
                Disconnect();
            }

            IP = ConnectIP;
            Port = ConnectPort;

            try
            {
                Socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                var AsyncEvent = AsyncEventPool_.Spawn();
                AsyncEvent.RemoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), Port);

                if (!Socket_.ConnectAsync(AsyncEvent))
                {
                    ProcessConnect(this, AsyncEvent);
                }

                return true;
            }
            catch (Exception Ex)
            {
                Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Failed);
                Logger.Write(Ex.Message, Logger.Type.Error);
                return false;
            }
        }

        public bool Listen(int ListenPort, int Backlog)
        {
            Disconnect();

            IP = string.Empty;
            Port = ListenPort;

            try
            {
                Socket_ = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Socket_.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

                Socket_.Bind(new IPEndPoint(IPAddress.Any, Port));
                Socket_.Listen(Backlog);

                var AsyncEvent = AsyncEventPool_.Spawn();
                if (!Socket_.AcceptAsync(AsyncEvent))
                {
                    ProcessAccept(AsyncEvent);
                }

                return true;
            }
            catch (Exception Ex)
            {
                Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Failed);
                Logger.Write(Ex.Message, Logger.Type.Error);
                return false;
            }
        }
        
        public void Disconnect()
        {
            if (Socket_ == null)
            {
                return;
            }

            if (Connected)
            {
                try
                {
                    Socket_.Shutdown(SocketShutdown.Both);
                }
                catch (Exception Ex)
                {
                    Logger.Write(Ex.Message, Logger.Type.Error);
                }
                finally
                {
                    Socket_.Close();
                    Socket_.Dispose();
                    Socket_ = null;
                }
            }
            
            Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Disconnect);
        }

        private void SendBuffer(byte[] Buffer)
        {
            if (Connected && Buffer != null && Buffer.Length > 0)
            {
                var AsyncEvent = AsyncEventPool_.Spawn();
                AsyncEvent.SetBuffer(Buffer, 0, Buffer.Length);

                if (!Socket_.SendAsync(AsyncEvent))
                {
                    ProcessSend(AsyncEvent);
                }
            }
        }

        public void SendMsg<T>(T Msg) where T : IMsg
        {
            if (Connected)
            {
                SendQueue_.Enqueue(new MsgEncoder<T>(Msg));
            }
            else
            {
                Logger.Write("Not Connected");
            }
        }

        public bool GetMsgNameByID(int MsgID, out string MsgName)
        {
            return MsgID2Name_.TryGetValue(MsgID, out MsgName);
        }

        public void RegisterMsgHandler<T>(Action<T> Callback) where T : IMsg
        {
            var FullName = typeof(T).FullName;
            var MsgID = (int)Crc32.Calculate(FullName);
            MsgID2Name_[MsgID] = FullName;

            if (!MsgHandler_.ContainsKey(MsgID))
            {
                MsgHandler_.Add(MsgID, new MsgListener<T>());
            }

            (MsgHandler_[MsgID] as MsgListener<T>).OnMsg += Callback;
        }

        public void UnRegisterMsgHandler<T>(Action<T> Callback) where T : IMsg
        {
            var FullName = typeof(T).FullName;
            var MsgID = (int)Crc32.Calculate(FullName);

            if (MsgHandler_.ContainsKey(MsgID))
            {
                (MsgHandler_[MsgID] as MsgListener<T>).OnMsg -= Callback;
            }
        }

        private void DispatchMsg(object Msg)
        {
            var MsgName = Msg.GetType().FullName;
            var MsgID = (int)Crc32.Calculate(MsgName);

            if (MsgHandler_.ContainsKey(MsgID))
            {
                MsgHandler_[MsgID].Trigger(Msg);
            }
        }

        public void Update(float DeltaTime)
        {
            if (ConnectedStateCode_ == (int) ConnectedCode.Failed)
            {
                OnDisconnected?.Invoke();
                Interlocked.Exchange(ref ConnectedStateCode_, (int) ConnectedCode.None);
            }
            else if (ConnectedStateCode_ == (int) ConnectedCode.Succeeded)
            {
                OnConnected?.Invoke();
                Interlocked.Exchange(ref ConnectedStateCode_, (int) ConnectedCode.None);
            }
            else if (ConnectedStateCode_ == (int) ConnectedCode.Disconnect)
            {
                OnDisconnected?.Invoke();
                Interlocked.Exchange(ref ConnectedStateCode_, (int) ConnectedCode.None);
            }

            while (true)
            {
                try
                {
                    MsgDecoder Decoder = null;
                    lock (RecvQueueLock_)
                    {
                        if (RecvQueue_.Count == 0)
                        {
                            break;
                        }
                        Decoder = RecvQueue_.Dequeue();
                    }

                    var Msg = Decoder.Decode();
                    if (Msg != null)
                    {
                        DispatchMsg(Msg);
                    }
                }
                catch
                {
                    break;
                }
            }

            if (LastSendTime_ > SendInterval_ && SendQueue_.Count > 0)
            {
                var Encoder = SendQueue_.Dequeue();
                var Buffer = Encoder.Encode();
                SendBuffer(Buffer);
                LastSendTime_ = 0.0f;
            }
            else
            {
                LastSendTime_ += DeltaTime;
            }
        }

        private void OnAsyncEventSpawn(SocketAsyncEventArgs AsyncEvent)
        {
            AsyncEvent.Completed += OnCompleted;
        }

        private void OnAsyncEventRecycle(SocketAsyncEventArgs AsyncEvent)
        {
            AsyncEvent.Completed -= OnCompleted;
        }

        private void OnCompleted(object Sender, SocketAsyncEventArgs AsyncEvent)
        {
            if (AsyncEvent.SocketError == SocketError.AddressAlreadyInUse)
            {
                Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Failed);
                return;
            }

            if (AsyncEvent.SocketError != SocketError.Success)
            {
                Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Failed);
                return;
            }

            switch (AsyncEvent.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(this, AsyncEvent);
                    break;
                case SocketAsyncOperation.Accept:
                    ProcessAccept(AsyncEvent);
                    break;
                case SocketAsyncOperation.Disconnect:
                    ProcessDisconnect(AsyncEvent);
                    break;
                case SocketAsyncOperation.Send:
                    ProcessSend(AsyncEvent);
                    break;
                case SocketAsyncOperation.Receive:
                    ProcessReceive(AsyncEvent);
                    break;
                default:
                    break;
            }
        }

        private void ProcessConnect(NetClient Client, SocketAsyncEventArgs AsyncEvent)
        {
            var StateObject = new ReadStateObject(Client.Name, Client.Socket_, 4);
            AsyncEvent.UserToken = StateObject;
            AsyncEvent.SetBuffer(StateObject.Buffer_, 0, StateObject.Buffer_.Length);

            if (!Client.Socket_.ReceiveAsync(AsyncEvent))
            {
                ProcessReceive(AsyncEvent);
            }

            Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Succeeded);
        }

        private void ProcessAccept(SocketAsyncEventArgs AsyncEvent)
        {
            var Name = "Remote Client : " + (AsyncEvent.AcceptSocket.RemoteEndPoint as IPEndPoint).ToString();
            var Client = new NetClient(Name, AsyncEvent.AcceptSocket);
            AsyncEventPool_.Recycle(AsyncEvent);
            AsyncEvent = Client.AsyncEventPool_.Spawn();
            Client.ProcessConnect(Client, AsyncEvent);
            OnAccept?.Invoke(Client);
        }

        private void ProcessDisconnect(SocketAsyncEventArgs AsyncEvent)
        {
            AsyncEventPool_.Recycle(AsyncEvent);
            Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Disconnect);
        }

        private void ProcessSend(SocketAsyncEventArgs AsyncEvent)
        {
            AsyncEventPool_.Recycle(AsyncEvent);
        }

        private void ProcessReceive(SocketAsyncEventArgs AsyncEvent)
        {
            var StateObject = AsyncEvent.UserToken as ReadStateObject;

            if (StateObject.Client_ == null || !StateObject.Client_.Connected)
            {
                return;
            }

            var NumberOfBytesRead = AsyncEvent.BytesTransferred;
            StateObject.BytesRead_ += NumberOfBytesRead;
            StateObject.BytesNeed_ -= NumberOfBytesRead;

            if (NumberOfBytesRead > 0)
            {
                if (StateObject.BytesNeed_ == 0)
                {
                    if (StateObject.Step_ == ReadStateObject.ReadStep.ReadLength)
                    {
                        var BytesNeed = BitConverter.ToInt32(StateObject.Buffer_, 0);
                        StateObject.Reset(ReadStateObject.ReadStep.Read, BytesNeed);
                        AsyncEvent.SetBuffer(StateObject.Buffer_, 0, StateObject.Buffer_.Length);

                        if (!StateObject.Client_.ReceiveAsync(AsyncEvent))
                        {
                            ProcessReceive(AsyncEvent);
                        }
                    }
                    else
                    {
                        lock (RecvQueueLock_)
                        {
                            RecvQueue_.Enqueue(new MsgDecoder(this, StateObject.Buffer_));
                        }

                        StateObject.Reset(ReadStateObject.ReadStep.ReadLength, 4);
                        AsyncEvent.SetBuffer(StateObject.Buffer_, 0, StateObject.Buffer_.Length);

                        if (!StateObject.Client_.ReceiveAsync(AsyncEvent))
                        {
                            ProcessReceive(AsyncEvent);
                        }
                    }
                }
                else
                {
                    if (!StateObject.Client_.ReceiveAsync(AsyncEvent))
                    {
                        ProcessReceive(AsyncEvent);
                    }
                }
            }
            else
            {
                Interlocked.Exchange(ref ConnectedStateCode_, (int)ConnectedCode.Failed);
            }
        }

        private class ReadStateObject
        {
            public enum ReadStep
            {
                ReadLength,
                Read
            }

            public string Name_ = string.Empty;
            public Socket Client_ = null;
            public ReadStep Step_ = ReadStep.ReadLength;
            public int BytesRead_ = 0;
            public int BytesNeed_ = 0;
            public byte[] Buffer_ = null;

            public ReadStateObject(string Name, Socket Client, int BytesNeed)
            {
                Name_ = Name;
                Client_ = Client;
                Reset(ReadStep.ReadLength, BytesNeed);
            }

            public void Reset(ReadStep Step, int BytesNeed)
            {
                Step_ = Step;
                BytesRead_ = 0;
                BytesNeed_ = BytesNeed;
                Buffer_ = new byte[BytesNeed_];
            }
        }

        private abstract class MsgListenerBase
        {
            public abstract void Trigger(object Msg);
        }

        private class MsgListener<T> : MsgListenerBase
        {
            public event Action<T> OnMsg = null;

            public override void Trigger(object Msg)
            {
                OnMsg?.Invoke((T)Msg);
            }
        }

        private abstract class MsgEncoderBase
        {
            public abstract byte[] Encode();
        }

        private class MsgEncoder<T> : MsgEncoderBase where T : IMsg
        {
            private readonly T InternalMsg_;

            public MsgEncoder(T Msg)
            {
                InternalMsg_ = Msg;
            }

            public override byte[] Encode()
            {
                var Helper = new SerializeHelper();
                var Buffer = MsgSerialize.Serialize(InternalMsg_);
                
                var DataLength = sizeof(int) + sizeof(int) + Buffer.Length;
                Helper.WriteInt32(DataLength);
                Helper.WriteInt32(InternalMsg_.ID);
                Helper.WriteInt8Array(Buffer);

                return Helper.GetBuffer();
            }
        }

        private class MsgDecoder
        {
            private readonly NetClient InternalClient_;
            private readonly byte[] InternalBuffer_;

            public MsgDecoder(NetClient Client, byte[] Buffer)
            {
                InternalClient_ = Client;
                InternalBuffer_ = Buffer;
            }

            public object Decode()
            {
                var Helper = new DeserializeHelper(InternalBuffer_);
                var MsgID = Helper.ReadInt32();
                var MsgName = string.Empty;

                if (!InternalClient_.GetMsgNameByID(MsgID, out MsgName))
                {
                    Logger.Write("Can't not find msg name by msgid : " + MsgID, Logger.Type.Warning);
                    return null;
                }

                var Buffer = Helper.ReadInt8Array();
                var MsgType = Type.GetType(MsgName, true);
                //var Msg = Activator.CreateInstance(MsgType, true) as IMsg;
                var Msg = MsgSerialize.Deserialize(MsgType, Buffer);
                return Msg;
            }
        }
    }
}
