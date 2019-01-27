using System.Collections.Generic;
using System.Threading;
using Events;
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
        public HostState State { get; private set; }
        public int BroadcastKey { get; set; }

        public OnHostStart OnStart;
        public OnHostShutdown OnShutdown;

        private GameObject _socketPrefab;

        private Socket _socket;
        private List<int> _connections;

        private Timer _discovery;

        private float _timeForDiscovery = 10;
        private float _switchDelay = 30;

        #region MonoBehaviour
        private void Start()
        {
            _connections = new List<int>();

            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageSend, SendToAll);

            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;
            _discovery = gameObject.AddComponent<Timer>();
            gameObject.name = "NetworkHost";

            Init();
        }
        private void Update()
        {
            switch (State)
            {
                case HostState.StartingUp:
                    StartingUp();
                    break;

                case HostState.Up:
                    Up();
                    break;

                case HostState.FallingBack:
                    FallingBack();
                    break;

                case HostState.ShuttingDown:
                    ShuttingDown();
                    break;

                case HostState.Down:
                    Down();
                    break;
            }
        }
        private void OnDestroy()
        {
            EventManager.Singleton.UnregisterListener(GameEventType.NetworkMessageSend, SendToAll);
        }
        #endregion

        #region Lifecycle
        private void Init()
        {
            if (BroadcastKey == 0)
                BroadcastKey = KeyGenerator.Generate();
            else
            {
                _discovery.Remains = 5;
                _discovery.Running = true;
                State = HostState.FallingBack;
            }

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
            Debug.LogFormat("HOST::Boot on port {0}", socketScript.Configuration.port);
        }
        private void StartingUp()
        {
            State = HostState.Up;
            OnStart(this);
        }
        private void FallingBack()
        {
            if (!_discovery.Elapsed) return;
            // _socket.StopBroadcast();
            Debug.Log("HOST::Finished broadcasting to 8001 port");
            State = HostState.Up;
            OnStart(this);
        }
        private void Up()
        {

        }
        private void ShuttingDown()
        {
            if (_socket != null) return;
            State = HostState.Down;
        }
        private void Down()
        {
            OnShutdown();
            Destroy(gameObject);
            Debug.Log("HOST::Shutdown");
        }
        #endregion

        public void Shutdown()
        {
            State = HostState.ShuttingDown;
            _socket.Close();
        }

        private void Send(object message, int connectionId)
        {
            // Debug.Log("    HOST::Sending data");
            _socket.Send(connectionId, 1, message as ANetworkMessage);
        }
        private void SendToAll(object message)
        {
            // Debug.Log("    HOST::Sending data");
            for (var i = 0; i < _connections.Count; i++) _socket.Send(_connections[i], 1, message as ANetworkMessage);
        }
        private void OnSocketOpened(Socket socket)
        {
            _socket = socket;
            if (State == HostState.FallingBack)
            {
                for (var i = 0; i < 60; i++)
                {
                    Thread.Sleep(10);
                    Debug.LogFormat("HOST::Broadcasting to {1} port with key {0}", BroadcastKey, 8001 + i);
                    _socket.StartBroadcast(BroadcastKey, 8001 + i, new FallbackHostReady());
                    Thread.Sleep(100);
                    _socket.StopBroadcast();
                }
            }
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socket.Id));
            _connections.Add(connection);

            Send(new FallbackInfo(BroadcastKey, (connection - 1) * _switchDelay), connection);
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
                    EventManager.Singleton.Publish(GameEventType.NetworkMessageReceived, message);
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
    }
}