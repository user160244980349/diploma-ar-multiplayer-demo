using UnityEngine.Networking;
using UnityEngine;

namespace Network.Connection
{
    public class NetworkConnection : MonoBehaviour
    {
        public int Id { get; private set; }
        public int SocketId { get; private set; }
        public int Port { get; private set; }
        public string Ip { get; private set; }
        public ConnectionSettings Settings { get; set; }

        public ConnectionState State { get; private set; }
        public NetworkError Error { get { return (NetworkError)_error; } }

        private int _queueLength;
        private Timer _send;
        private Timer _connect;
        private Timer _disconnect;
        private byte _error;

        #region MonoBehaviour
        private void Start()
        {
            Id = Settings.id;
            SocketId = Settings.socketId;
            Ip = Settings.ip;
            Port = Settings.port;

            _connect = gameObject.AddComponent<Timer>();
            _connect.Duration = Settings.connectDelay;
            _send = gameObject.AddComponent<Timer>();
            _send.Duration = Settings.sendRate;
            _disconnect = gameObject.AddComponent<Timer>();
            _disconnect.Duration = Settings.disconnectDelay;

            gameObject.name = string.Format("Connection{0}", Id);

            State = ConnectionState.ReadyToConnect;
        }
        private void Update()
        {
            ManageConnection();
        }
        #endregion

        public void Connect()
        {
            if (State != ConnectionState.ReadyToConnect) return;
            State = ConnectionState.Connecting;
        }
        public void Confirm()
        {
            if (State != ConnectionState.WaitingConfirm) return;
            State = ConnectionState.Connected;
        }
        public void Up()
        {
            if (State != ConnectionState.Connected) return;
            State = ConnectionState.Up;
            _send.Discard();
            _send.Running = true;
        }
        public void Disconnect(bool incoming)
        {
            if (State != ConnectionState.Up) return;
            if (incoming)
            {
                State = ConnectionState.Disconnected;
                return;
            }
            State = ConnectionState.Disconnecting;
            _disconnect.Discard();
            _disconnect.Running = true;
        }
        public void QueueMessage(int channelId, byte[] packet)
        {
            if (State != ConnectionState.Up) return;
            _queueLength++;
            NetworkTransport.QueueMessageForSending(SocketId, Id, channelId, packet, packet.Length, out _error);
            ShowErrorIfThrown();
        }

        private void ManageConnection()
        {
            switch (State)
            {
                case ConnectionState.Connecting:
                {
                    if (Id == 0)
                    {
                        State = ConnectionState.WaitingConfirm;
                        Id = NetworkTransport.Connect(SocketId, Ip, Port, 0, out _error);
                        gameObject.name = string.Format("Connection{0}", Id);
                        ShowErrorIfThrown();
                    }
                    else
                    {
                        State = ConnectionState.WaitingDelay;
                        _connect.Discard();
                        _connect.Running = true;
                    }
                    break;
                }

                case ConnectionState.WaitingConfirm:
                {
                    break;
                }

                case ConnectionState.WaitingDelay:
                {
                    if (_connect.Elapsed)
                    {
                        State = ConnectionState.Connected;
                    }
                    break;
                }

                case ConnectionState.Connected:
                {
                    break;
                }

                case ConnectionState.Up:
                {
                    if (!_send.Elapsed) return;
                    _send.Discard();
                    if (_queueLength > 0)
                    {
                        _queueLength = 0;
                        NetworkTransport.SendQueuedMessages(SocketId, Id, out _error);
                        ShowErrorIfThrown();
                    }
                    break;
                }

                case ConnectionState.Disconnecting:
                {
                    if (!_disconnect.Elapsed) return;
                    State = ConnectionState.Disconnected;
                    NetworkTransport.Disconnect(SocketId, Id, out _error);
                    ShowErrorIfThrown();
                    break;
                }

                case ConnectionState.Disconnected:
                {
                    break;
                }
            }
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}