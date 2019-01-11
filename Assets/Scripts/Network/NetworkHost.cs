using System.Collections.Generic;
using System.Threading;
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
        public ConnectionConfiguration Configuration { get; set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;

        private GameObject _socketObject;

        private Socket _socket;
        private List<int> _connections;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkHost";
        }
        private void Start()
        {
            Debug.Log("HOST::Boot");

            _connections = new List<int>();

            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();

            _socketObject = (GameObject)Resources.Load("Networking/Socket");

            var socket = Instantiate(_socketObject, gameObject.transform);
            _socket = socket.GetComponent<Socket>();
            _socket.Config = new SocketConfiguration
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8000,
                maxConnections = 16,
                maxMessagesForSend = 16,
                onStart = OnStart,
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent,
                onClose = OnClose,
            };

            _snm.Subscribe(Send);
        }
        private void OnDestroy()
        {
            Debug.Log("HOST::Shutdown");
            _snm.Unsubscribe(Send);
        }
        #endregion

        public void Shutdown()
        {
            _socket.Close();
        }
        public void Send(ANetworkMessage message)
        {
            Debug.Log("HOST::Sending data");
            for (var i = 0; i < _connections.Count; i++) _socket.Send(_connections[i], 1, message);
        }
        private void Disconnect(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socket.Id));
            _socket.CloseConnection(connection);
            _connections.Remove(connection);
        }

        private void OnStart()
        {
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
        private void OnClose()
        {
            Destroy(gameObject);
        }
    }
}