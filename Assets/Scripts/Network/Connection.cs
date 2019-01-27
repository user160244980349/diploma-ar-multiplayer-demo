using UnityEngine.Networking;
using UnityEngine;
using Network.Delegates;
using Network.States;
using Network.Configurations;

namespace Network
{
    public class Connection : MonoBehaviour
    {
        public int Id { get; private set; }
        public bool IncomingConnection { get; set; }
        public bool IncomingDisconnection { get; set; }
        public bool Confirmed { get; set; }
        public ConnectionState State { get; private set; }
        public ConnectionConfiguration Configuration { get; set; }
        public ConnectionBindings Bindings { get; set; }

        public OnConnectionStart OnConnect;
        public OnConnectionWaitingConfirm OnWaitingConfirm;
        public OnConnectionShutdown OnDisconnect;

        private int _queueLength;
        private Timer _connect;
        private Timer _send;
        private Timer _disconnect;
        private const float _sendRate = 0.02f;
        private const float _connectDelay = 0.2f;
        private const float _disconnectDelay = 0.2f;
        private byte _error;

        #region Configuration
        private int _socketId;
        private string _ip;
        private int _port;
        #endregion

        #region MonoBehaviour
        private void Start()
        {
            Id = Bindings.id;
            _socketId = Bindings.socketId;
            _ip = Configuration.ip;
            _port = Configuration.port;

            if (!IncomingConnection)
            {
                Id = NetworkTransport.Connect(_socketId, _ip, _port, 0, out _error);
                ShowErrorIfThrown();
            }

            _send = gameObject.AddComponent<Timer>();
            _disconnect = gameObject.AddComponent<Timer>();
            _connect = gameObject.AddComponent<Timer>();
            _connect.Remains = _connectDelay;
            _connect.Running = true;
            gameObject.name = string.Format("Connection{0}", Id);
        }
        private void Update()
        {
            switch (State)
            {
                case ConnectionState.Connecting:
                    Connecting();
                    break;

                case ConnectionState.WaitingConfirm:
                    WaitingConfirm();
                    break;

                case ConnectionState.Connected:
                    Connected();
                    break;

                case ConnectionState.Disconnecting:
                    Disconnecting();
                    break;

                case ConnectionState.Disconnected:
                    Disconnected();
                    break;
            }
        }
        #endregion

        #region Lifecycle
        private void Connecting()
        {
            State = ConnectionState.WaitingConfirm;
            OnWaitingConfirm(this);
        }
        private void WaitingConfirm()
        {
            if ((IncomingConnection || Confirmed) && _connect.Elapsed)
            {
                State = ConnectionState.Connected;
                _send.Running = true;
                OnConnect(Id);
            }
        }
        private void Connected()
        {
            if (!_send.Elapsed) return;
            _send.Remains = _sendRate;
            if (_queueLength > 0)
            {
                _queueLength = 0;
                NetworkTransport.SendQueuedMessages(_socketId, Id, out _error);
                ShowErrorIfThrown();
            }
        }
        private void Disconnecting()
        {
            if (!_disconnect.Elapsed) return;
            State = ConnectionState.Disconnected;
        }
        private void Disconnected()
        {
            OnDisconnect(Id);
            Destroy(gameObject);
        }
        #endregion

        public void Disconnect()
        {
            State = ConnectionState.Disconnecting;
            _disconnect.Running = true;
            if (IncomingDisconnection)
                return;
            else
                _disconnect.Remains = _disconnectDelay;
            NetworkTransport.Disconnect(_socketId, Id, out _error);
            ShowErrorIfThrown();
        }
        public void QueueMessage(int channelId, byte[] packet)
        {
            if (State == ConnectionState.Connected)
            {
                _queueLength++;
                NetworkTransport.QueueMessageForSending(_socketId, Id, channelId, packet, packet.Length, out _error);
                ShowErrorIfThrown();
            }
        }

        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}