using Events;
using Events.EventTypes;
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
        public NetworkUnitState State { get; private set; }
        public ConnectionConfiguration ConnectionConfig { get; set; }
        public OnClientStart OnStart;
        public OnClientShutdown OnShutdown;

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;
        private GameObject _socketPrefab;

        private Socket _socket;
        private int _connectionId;

        private int _networkKey;
        private int _fallbackPos;
        private float _timeToSwitch;
        private float _fallbackDelay = 30;

        #region MonoBehaviour
        private void Start()
        {

            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();

            _socketPrefab = (GameObject)Resources.Load("Networking/Socket");

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
            _snm.Subscribe(Send);

            gameObject.name = "NetworkClient";
            Debug.LogFormat("CLIENT::Boot on port {0}", socketScript.Configuration.port);
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

                case NetworkUnitState.FallingBack:
                    _timeToSwitch -= Time.deltaTime;
                    if (_timeToSwitch < 0)
                    {
                        Debug.Log("Falling back");
                        NetworkManager.Singleton.SpawnHost(true);
                        Shutdown();
                    }
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
            Debug.Log("CLIENT::Shutdown");
        }
        #endregion

        public void Shutdown()
        {
            State = NetworkUnitState.ShuttingDown;
            _socket.Close();
        }

        private void Send(ANetworkMessage message)
        {
            // Debug.Log("CLIENT::Sending data");
            _socket.Send(_connectionId, 0, message);
        }
        private void OnSocketStart(Socket socket)
        {
            _socket = socket;
            socket.OpenConnection(ConnectionConfig);
        }
        private void OnBroadcastEvent(ConnectionConfiguration cc, ANetworkMessage message)
        {
            Debug.Log(string.Format("CLIENT::Received broadcast data on socket {0}", _socket.Id));
            switch (message.networkMessageType)
            {
                case NetworkMessageType.FallbackHostReady:
                    if (_networkKey != ((FallbackHostReady)message).netKey) return;
                    State = NetworkUnitState.Up;
                    _socket.OpenConnection(cc);
                    break;
            }
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log("CLIENT::Connected to host");
            _connectionId = connection;
            ApplicationManager.Singleton.LoadScene("Playground");
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
                    NetworkTransport.SetBroadcastCredentials(_socket.Id, ((FallbackInfo)message).netKey, 1, 1, out byte error);
                    _networkKey = ((FallbackInfo)message).netKey;
                    Debug.Log(_networkKey);
                    _fallbackPos = ((FallbackInfo)message).queuePosition;
                    _timeToSwitch = (_fallbackPos - 1) * _fallbackDelay;
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
            Debug.Log("CLIENT::Disconnected from host");
            State = NetworkUnitState.FallingBack;
        }
        private void OnSocketShutdown(int socketId)
        {
            _socket = null;
        }
    }
}