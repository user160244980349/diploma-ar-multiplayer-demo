using Events;
using Events.EventTypes;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        private int _connectionId;
        private MultiplayerMessageReady _mmr;
        private ReceivedMultiplayerMessage _rmm;
        private int _socketId;
        
        public static Client Singleton { get; private set; }
        public ClientState State { get; private set; }
        
        public void Boot()
        {
            Debug.Log("CLIENT::Boot");
            State = ClientState.Ready;

            var sc = new SocketConfiguration
            {
                channels = new QosType[2] {QosType.Reliable, QosType.Unreliable},
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent
            };
        
            _socketId = NetworkManager.Singleton.OpenSocket(sc);
        }
        public void Connect(ConnectionConfiguration cc)
        {
            State = ClientState.Connecting;
            _mmr.Subscribe(Send);
            NetworkManager.Singleton.OpenConnection(_socketId, cc);
        }
        public void Disconnect()
        {
            State = ClientState.Disconnecting;
            _mmr.Unsubscribe(Send);
            NetworkManager.Singleton.CloseConnection(_socketId, _connectionId);
        }
        public void Send(ANetworkMessage message)
        {
//            Debug.Log("CLIENT::Sending data");
            NetworkManager.Singleton.Send(_socketId, _connectionId, 0, message);
        }
        public void Shutdown()
        {
            Debug.Log("CLIENT::Shutdown");
            State = ClientState.Down;     
            NetworkManager.Singleton.CloseSocket(_socketId);
        }

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            State = ClientState.Down;
        }
        private void Start()
        {
            _mmr = EventManager.Singleton.GetEvent<MultiplayerMessageReady>();
            _rmm = EventManager.Singleton.GetEvent<ReceivedMultiplayerMessage>();
        }
        #endregion

        #region Network events
        private void OnBroadcastEvent(int connection)
        {
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log("CLIENT::Connected to host");
            State = ClientState.Connected;
            _connectionId = connection;
            ApplicationManager.Singleton.LoadScene("Playground");
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
//            Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection,
//                _socketId));

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
            }
        }
        private void OnDisconnectEvent(int connection)
        {
            Debug.Log("CLIENT::Disconnected from host");
            State = ClientState.Ready;
            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
        #endregion
    }
}