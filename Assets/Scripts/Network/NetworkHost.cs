using System.Collections.Generic;
using Events;
using Events.EventTypes;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkHost : MonoBehaviour
    {
        public static NetworkHost Singleton { get; private set; }
        public NetworkHostConfiguration HostConfiguration { get; set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;

        private GameObject _socketPrefab;

        private Socket _socket;
        private List<int> _connections;
        private bool _shutteddown;
        private float _timeToShutdown = 1f;

        #region Delegates
        public delegate void OnNetworkHostStart();
        public delegate void OnNetworkHostShutdown();
        #endregion

        #region Configurations
        private OnNetworkHostStart _onNetworkHostStart;
        private OnNetworkHostShutdown _onNetworkHostShutdown;
        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            gameObject.name = "NetworkHost";
        }
        private void Start()
        {
            _connections = new List<int>();

            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();

            _socketPrefab = (GameObject)Resources.Load("Networking/Socket");

            _onNetworkHostStart = HostConfiguration.onNetworkHostStart;
            _onNetworkHostShutdown = HostConfiguration.onNetworkHostShutdown;

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            var socketScript = socketObject.GetComponent<Socket>();
            socketScript.Configuration = new SocketConfiguration
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8000,
                maxConnections = 16,
                maxMessagesForSend = 16,
                onSocketStart = OnSocketStart,
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent,
                onSocketDestroy = OnSocketShutdown,
            };

            _snm.Subscribe(Send);
            _onNetworkHostStart();
            Debug.Log("HOST::Boot");
        }
        private void Update()
        {
            if (_shutteddown)
            {
                if (_socket != null) return;
                // Debug.Log("Shutting down host");
                _timeToShutdown -= Time.deltaTime;
                if (_timeToShutdown > 0) return;
                Debug.Log("HOST::Shutdown");
                _onNetworkHostShutdown();
                return;
            }
        }
        private void OnDestroy()
        {
            _snm.Unsubscribe(Send);
        }
        #endregion

        public void Shutdown()
        {
            _shutteddown = true;
            _socket.Shutdown();
        }
        private void OnSocketStart(Socket socket)
        {
            _socket = socket;
            Debug.LogFormat(" >> Socket opened {0}", socket.Id);
        }
        private void OnSocketShutdown(Socket socket)
        {
            _socket = null;
            Destroy(socket.gameObject);
            Debug.LogFormat(" >> Socket closed {0}", socket.Id);
        }

        public void Send(ANetworkMessage message)
        {
            Debug.Log("HOST::Sending data");
            for (var i = 0; i < _connections.Count; i++) _socket.Send(_connections[i], 1, message);
        }

        private void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socket.Id));
            _connections.Add(connection);
        }
        private void OnBroadcastEvent(int connection)
        {
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
            Debug.Log(string.Format("HOST::Received data from client {0} connected to socket {1}", connection, _socket.Id));
            switch (message.networkMessageType)
            {
                case NetworkMessageType.Beep:
                    Debug.Log(" > Boop from network layer");
                    break;

                case NetworkMessageType.Service:

                    break;

                case NetworkMessageType.Higher:
                    _rnm.Publish(message);
                    break;
            }
        }
        private void OnDisconnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", connection, _socket.Id));
            _connections.Remove(connection);
        }
    }
}