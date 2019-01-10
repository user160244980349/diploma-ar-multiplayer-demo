using Events;
using Events.EventTypes;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkClient : MonoBehaviour
    {
        public static NetworkClient Singleton { get; private set; }
        public ClientState State { get; private set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;
        private int _socketId;
        private int _connectionId;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkClient";
            State = ClientState.Down;
        }
        private void Start()
        {
            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();
        }
        #endregion

        public void Boot()
        {
            Debug.Log("CLIENT::Boot");
            State = ClientState.Ready;

            var sc = new SocketConfiguration
            {
                channels = new QosType[2] {QosType.Reliable, QosType.Unreliable},
                maxConnections = 16,
                maxMessagesForSend = 16,
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent
            };

            _socketId = NetworkManager.Singleton.OpenSocket(sc);
        }
        public void Shutdown()
        {
            Debug.Log("CLIENT::Shutdown");
            State = ClientState.Down;
            NetworkManager.Singleton.CloseSocket(_socketId);
        }
        public void Connect(ConnectionConfiguration cc)
        {
            State = ClientState.Connecting;
            _snm.Subscribe(Send);
            NetworkManager.Singleton.OpenConnection(_socketId, cc);
        }
        public void Disconnect()
        {
            State = ClientState.Disconnecting;
            _snm.Unsubscribe(Send);
            NetworkManager.Singleton.CloseConnection(_socketId, _connectionId);
        }
        public void Send(ANetworkMessage message)
        {
            // Debug.Log("CLIENT::Sending data");
            NetworkManager.Singleton.Send(_socketId, _connectionId, 0, message);
        }
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
            // Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection, _socketId));
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
            Debug.Log("CLIENT::Disconnected from host");
            State = ClientState.Ready;
            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
    }
}