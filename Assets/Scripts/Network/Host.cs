using System.Collections.Generic;
using Events;
using Events.EventTypes;
using Network.Configurations;
using Network.Delegates;
using Network.Messages;
using Network.States;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        public NetworkUnitState State { get; private set; }
        public OnHostStart OnStart;
        public OnHostShutdown OnShutdown;

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;
        private GameObject _socketPrefab;

        private Socket _socket;
        private List<int> _connections;

        private NetworkIdGenerator _gen;
        private string _networkId;
        private int _fallbackConnection;

        #region MonoBehaviour
        private void Start()
        {
            Debug.Log("HOST::Booted");
            _gen = new NetworkIdGenerator(20);
            _networkId = _gen.Generate();

            _connections = new List<int>();

            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();

            _socketPrefab = (GameObject)Resources.Load("Networking/Socket");

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            var socketScript = socketObject.GetComponent<Socket>();
            socketScript.OnSocketOpened = OnSocketOpened;
            socketScript.OnConnected = OnConnectEvent;
            socketScript.OnDataReceived = OnDataEvent;
            socketScript.OnBroadcastReceived = OnBroadcastEvent;
            socketScript.OnDisconnected = OnDisconnectEvent;
            socketScript.OnSocketClosed = OnSocketClosed;
            socketScript.Configuration = new SocketConfiguration
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8000,
                maxConnections = 16,
                packetSize = 1024,
            };
            _snm.Subscribe(Send);

            gameObject.name = "NetworkHost";
        }
        private void Update()
        {
            switch (State)
            {
                case NetworkUnitState.StartingUp:
                    State = NetworkUnitState.Up;
                    OnStart(this);
                    break;

                case NetworkUnitState.Up:

                    break;

                case NetworkUnitState.ShuttingDown:
                    if (_socket != null) break;
                    State = NetworkUnitState.Down;
                    break;

                case NetworkUnitState.Down:
                    OnShutdown();
                    Destroy(gameObject);
                    break;
            }
        }
        private void OnDestroy()
        {
            _snm.Unsubscribe(Send);
            Debug.Log("HOST::Shutdown");
            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
        #endregion

        public void Shutdown()
        {
            State = NetworkUnitState.ShuttingDown;
            _socket.Close();
        }

        private void Send(ANetworkMessage message, int connectionId)
        {
            // Debug.Log("    HOST::Sending data");
            _socket.Send(connectionId, 1, message);
        }
        private void Send(ANetworkMessage message)
        {
            // Debug.Log("    HOST::Sending data");
            for (var i = 0; i < _connections.Count; i++) _socket.Send(_connections[i], 1, message);
        }
        private void OnSocketOpened(Socket socket)
        {
            _socket = socket;
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socket.Id));
            _connections.Add(connection);

            Send(new Beep(), connection); // netid
            if (_fallbackConnection != 0)
            {
                _fallbackConnection = connection;
                Send(new Beep(), _fallbackConnection); // fallback token
            }
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
                    Debug.Log("HOST::Boop from network layer");
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
        private void OnSocketClosed(int socketId)
        {
            _socket = null;
        }
    }
}