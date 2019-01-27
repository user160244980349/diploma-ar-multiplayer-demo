using Events;
using Network.Configurations;
using Network.Delegates;
using Network.Messages;
using Network.States;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        public ClientState State { get; private set; }
        public int BroadcastKey { get; set; }
        public ConnectionConfiguration ConnectionConfig { get; set; }

        public OnClientStart OnStart;
        public OnClientFallback OnFallback;
        public OnClientShutdown OnShutdown;

        private GameObject _socketPrefab;

        private Socket _socket;
        private int _connection;

        private Timer _switch;

        #region MonoBehaviour
        private void Start()
        {
            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageSend, Send);

            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;
            _switch = gameObject.AddComponent<Timer>();
            gameObject.name = "NetworkClient";

            Init();
        }
        private void Update()
        {
            switch (State)
            {
                case ClientState.StartingUp:
                    StartingUp();
                    break;

                case ClientState.Up:
                    Up();
                    break;
                    
                case ClientState.WaitingReconnect:
                    WaitingReconnect();
                    break;

                case ClientState.ShuttingDown:
                    ShuttingDown();
                    break;

                case ClientState.Down:
                    Down();
                    break;
            }
        }
        private void OnDestroy()
        {
            EventManager.Singleton.UnregisterListener(GameEventType.NetworkMessageSend, Send);
        }
        #endregion

        #region Lifecycle
        private void Init()
        {
            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            var socketScript = socketObject.GetComponent<Socket>();
            socketScript.OnSocketOpened = OnSocketStart;
            socketScript.OnConnected = OnConnectEvent;
            socketScript.OnDataReceived = OnDataEvent;
            socketScript.OnBroadcastReceived = OnBroadcastEvent;
            socketScript.OnDisconnected = OnDisconnectEvent;
            socketScript.OnSocketClosed = OnSocketShutdown;
            socketScript.Configuration = new SocketConfiguration
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                maxConnections = 1,
                port = 8001 + DateTime.Now.Second,
                packetSize = 1024,
            };
            Debug.LogFormat("CLIENT::Boot on port {0}", socketScript.Configuration.port);
        }
        private void StartingUp()
        {
            State = ClientState.Up;
            OnStart(this);
        }
        private void Up()
        {

        }
        private void WaitingReconnect()
        {
            if (!_switch.Elapsed) return;
            Debug.Log("CLIENT::Falling back");
            OnFallback(BroadcastKey);
            Shutdown();
        }
        private void ShuttingDown()
        {
            if (_socket != null) return;
            State = ClientState.Down;
        }
        private void Down()
        {
            Debug.Log("CLIENT::Shutdown");
            OnShutdown();
            Destroy(gameObject);
        }
        #endregion

        public void Shutdown()
        {
            State = ClientState.ShuttingDown;
            _socket.Close();
        }

        private void Send(object message)
        {
            // Debug.Log("CLIENT::Sending data");
            _socket.Send(_connection, 0, message as ANetworkMessage);
        }
        private void OnSocketStart(Socket socket)
        {
            _socket = socket;
            socket.OpenConnection(ConnectionConfig);
        }
        private void OnBroadcastEvent(ConnectionConfiguration cc, ANetworkMessage message)
        {
            switch (message.networkMessageType)
            {
                case NetworkMessageType.FallbackHostReady:
                {
                    if (State != ClientState.WaitingReconnect) break;
                    State = ClientState.Up;
                    _socket.OpenConnection(cc);
                    break;
                }
            }
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log("CLIENT::Connected to host");
            _connection = connection;
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
            Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection, _socket.Id));
            switch (message.networkMessageType)
            {
                case NetworkMessageType.Beep:
                {
                    Debug.Log("CLIENT::Boop from network layer");
                    break;
                }

                case NetworkMessageType.FallbackInfo:
                {
                    var fallbackInfo = (FallbackInfo)message;
                    BroadcastKey = fallbackInfo.netKey;
                    _switch.Remains = fallbackInfo.switchDelay;
                    _socket.SetBroadcastReceiveKey(BroadcastKey);
                    Debug.LogFormat("CLIENT::Got broadcast key {0}", BroadcastKey);
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
            Debug.Log("CLIENT::Disconnected from host");
            _switch.Running = true;
            State = ClientState.WaitingReconnect;
        }
        private void OnSocketShutdown(int socketId)
        {
            _socket = null;
        }
    }
}