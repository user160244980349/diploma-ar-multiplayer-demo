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
        public ConnectionState State { get; private set; }
        public ConnectionConfiguration Configuration { get; set; }
        public OnConnectionStart OnConnect;
        public OnConnectionShutdown OnDisconnect;

        #region Configuration
        private int _socketId;
        private string _ip;
        private int _port;
        #endregion

        private int _queueLength;
        private float _lastSendTime;
        private float _sendRate = 0.02f;
        private float _timeBeforeConnect = 1f;
        private float _timeAfterDisconnect = 1f;
        private byte _error;

        #region MonoBehaviour
        private void Start()
        {
            Debug.LogFormat("Connection opened {0}", Id);

            Id = Configuration.id;
            _socketId = Configuration.socketId;
            _ip = Configuration.ip;
            _port = Configuration.port;

            gameObject.name = string.Format("Connection{0}", Id);
        }
        private void Update()
        {
            switch (State)
            {
                case ConnectionState.Connecting:
                    _timeBeforeConnect -= Time.deltaTime;
                    if (_timeBeforeConnect > 0) break;
                    if (!IncomingConnection)
                    {
                        Id = NetworkTransport.Connect(_socketId, _ip, _port, 0, out _error);
                        ShowErrorIfThrown();
                    }
                    State = ConnectionState.Connected;
                    OnConnect(this);
                    break;

                case ConnectionState.Connected:
                    if (Time.time - _lastSendTime < _sendRate) break;
                    _lastSendTime = Time.time;
                    if (_queueLength > 0)
                    {
                        _queueLength = 0;
                        NetworkTransport.SendQueuedMessages(_socketId, Id, out _error);
                        ShowErrorIfThrown();
                    }
                    break;

                case ConnectionState.Disconnecting:
                    _timeAfterDisconnect -= Time.deltaTime;
                    if (_timeAfterDisconnect > 0) break;
                    State = ConnectionState.Disconnected;
                    break;

                case ConnectionState.Disconnected:
                    OnDisconnect(Id);
                    Destroy(gameObject);
                    break;
            }
        }
        private void OnDestroy()
        {
            Debug.LogFormat("Connection closed {0}", Id);
        }
        #endregion

        public void Disconnect()
        {
            State = ConnectionState.Disconnecting;
            if (IncomingDisconnection) return;
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