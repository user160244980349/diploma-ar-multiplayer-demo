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
        public bool FallbackMode { get; set; }
        public NetworkUnitState State { get; private set; }
        public OnHostStart OnStart;
        public OnHostShutdown OnShutdown;

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;
        private GameObject _socketPrefab;

        private Socket _socket;
        private List<int> _connections;

        private int _networkKey;
        private float _timeForDiscovery = 10;
        private float _switchDelay = 30;

        #region MonoBehaviour
        private void Start()
        {
            _networkKey = KeyGenerator.Generate();

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
            Debug.LogFormat("HOST::Boot on port {0}", socketScript.Configuration.port);
        }
        private void Update()
        {
            switch (State)
            {
                case NetworkUnitState.StartingUp:
                {
                    State = NetworkUnitState.Up;
                    OnStart(this);
                    break;
                }

                case NetworkUnitState.Up:
                {
                    if (FallbackMode)
                    {
                        _timeForDiscovery -= Time.deltaTime;
                        if (_timeForDiscovery > 0) break;
                        _socket.StopBroadcast();
                        Debug.Log("HOST::Finished broadcasting to 8001 port");
                        FallbackMode = false;
                    }
                    break;
                }

                case NetworkUnitState.ShuttingDown:
                {
                    if (_socket != null) break;
                    State = NetworkUnitState.Down;
                    break;
                }

                case NetworkUnitState.Down:
                {
                    OnShutdown();
                    Destroy(gameObject);
                    break;
                }
            }
        }
        private void OnDestroy()
        {
            _snm.Unsubscribe(Send);
            Debug.Log("HOST::Shutdown");
        }
        #endregion

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
            if (FallbackMode)
            {
                Debug.Log("HOST::Broadcasting to 8001 port");
                _socket.StartBroadcast(_networkKey, 8001, new FallbackHostReady());
            }
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socket.Id));
            _connections.Add(connection);

            Send(new FallbackInfo(_networkKey, (connection - 1) * _switchDelay), connection);
        }
        private void OnBroadcastEvent(ConnectionConfiguration cc, ANetworkMessage message)
        {
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
            Debug.Log(string.Format("HOST::Received data from client {0} connected to socket {1}", connection, _socket.Id));
            switch (message.networkMessageType)
            {
                case NetworkMessageType.Beep:
                {
                    Debug.Log("HOST::Boop from network layer");
                    break;
                }

                case NetworkMessageType.Higher:
                {
                    _rnm.Publish(message);
                    break;
                }
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
        public void Shutdown()
        {
            State = NetworkUnitState.ShuttingDown;
            _socket.Close();
        }
    }
}