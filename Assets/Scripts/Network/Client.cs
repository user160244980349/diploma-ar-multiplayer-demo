using Events;
using Events.EventTypes;
using Multiplayer;
using Multiplayer.Messages;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        private static Client _instance;

        private ClientState _state;
        private int _socketId;
        private int _connectionId;

        private MultiplayerMessageReady _mmr;
        private ReceivedMultiplayerMessage _rmm;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            _state = ClientState.Down;
        }
        private void Start()
        {
            _mmr = EventManager.GetInstance().GetEvent<MultiplayerMessageReady>();
            _rmm = EventManager.GetInstance().GetEvent<ReceivedMultiplayerMessage>();
            _mmr.Subscribe(Send);
        }
        private void Update()
        {

        }
        #endregion

        public static Client GetInstance()
        {
            return _instance;
        }
        public ClientState GetState()
        {
            return _state;
        }
        public void Boot()
        {
            Debug.Log("CLIENT::Boot");
            _state = ClientState.Ready;
            var sc = new SocketConfiguration
            {
                channels = new QosType[2] {QosType.Reliable, QosType.Reliable},
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent
            };
            _socketId = NetworkManager.GetInstance().OpenSocket(sc);
        }
        public void Connect(ConnectionConfiguration cc)
        {
            _state = ClientState.Connecting;
            NetworkManager.GetInstance().OpenConnection(_socketId, cc);
        }
        public void Disconnect()
        {
            _state = ClientState.Disconnecting;
            NetworkManager.GetInstance().CloseConnection(_socketId, _connectionId);
        }
        public void Send(ANetworkMessage message)
        {
            Debug.Log("CLIENT::Sending data");
            NetworkManager.GetInstance().Send(_socketId, _connectionId, 0, message);
        }
        public void Shutdown()
        {
            Debug.Log("CLIENT::Shutdown");
            _state = ClientState.Down;
            NetworkManager.GetInstance().CloseSocket(_socketId);
        }

        #region Network events
        private void OnBroadcastEvent(int connection)
        {

        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log("CLIENT::Connected to host");
            _state = ClientState.Connected;
            _connectionId = connection;
            ApplicationManager.GetInstance().LoadScene("Playground");
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
            Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection, _socketId));
            switch (message.networkMessageType)
            {
                case NetworkMessageType.Beep:
                    Debug.Log(" > Boop from network layer");
                    break;

                case NetworkMessageType.Service:

                    break;

                case NetworkMessageType.Higher:
                    _rmm.Publish(message);
                    break;

                default:
                    break;
            }
        }
        private void OnDisconnectEvent(int connection)
        {
            Debug.Log("CLIENT::Disconnected from host");
            _state = ClientState.Ready;
            ApplicationManager.GetInstance().LoadScene("MainMenu");
        }
        #endregion
    }
}
