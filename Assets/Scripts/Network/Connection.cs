using UnityEngine.Networking;
using UnityEngine;
using Network.Messages;
using Network.Delegates;

namespace Network
{
    public class Connection : MonoBehaviour
    {
        public int Id { get; private set; }
        public bool Incoming { get; set; }
        public ConnectionConfiguration Configuration { get; set; }

        private byte _error;
        private bool _shutteddown;
        private bool _readyToSend;
        private float _sendRate = 0.02f;
        private float _lastSendTime;
        private float _timeToDisconnect = 1f;
        private int _queueLength;

        #region Configuration
        private int _socketId;
        private string _ip;
        private int _port;
        private OnConnectionStart _onConnectionStart;
        private OnConnectionShutdown _onConnectionShutdown;
        #endregion

        #region MonoBehaviour
        private void Start()
        {
            Id = Configuration.id;
            _socketId = Configuration.socketId;
            _ip = Configuration.ip;
            _port = Configuration.port;
            _readyToSend = false;

            _onConnectionStart = Configuration.onConnectionStart;
            _onConnectionShutdown = Configuration.onConnectionDestroy;

            if (!Incoming)
            {
                Id = NetworkTransport.Connect(_socketId, _ip, _port, 0, out _error);
                ShowErrorIfThrown();
            }

            gameObject.name = string.Format("Connection{0}", Id);
            _onConnectionStart(this);
        }
        private void Update()
        {
            if (_shutteddown)
            {
                _timeToDisconnect -= Time.deltaTime;
                // Debug.Log("Disconnecting");
                if (_timeToDisconnect > 0) return;
                _onConnectionShutdown(this);
                return;
            }

            if (Time.time - _lastSendTime < _sendRate) return;
            _lastSendTime = Time.time;

            if (_queueLength > 0)
            {
                _queueLength = 0;
                NetworkTransport.SendQueuedMessages(_socketId, Id, out _error);
                ShowErrorIfThrown();
            }
        }
        #endregion

        public void Shutdown()
        {
            _shutteddown = true;

            NetworkTransport.Disconnect(_socketId, Id, out _error);
            ShowErrorIfThrown();
        }
        public void QueueMessage(int channelId, byte[] packet)
        {
            if (_shutteddown) return;

            _queueLength++;
            NetworkTransport.QueueMessageForSending(_socketId, Id, channelId, packet, packet.Length, out _error);
            ShowErrorIfThrown();
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}