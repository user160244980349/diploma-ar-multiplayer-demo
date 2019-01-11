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
        public ConnectionConfiguration Configuration { get; set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;

        private GameObject _socketObject;

        private Socket _socket;
        private int _connectionId;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkClient";
        }
        private void Start()
        {
            Debug.Log("CLIENT::Boot");

            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();

            _socketObject = (GameObject)Resources.Load("Networking/Socket");

            var socket = Instantiate(_socketObject, gameObject.transform);
            _socket = socket.GetComponent<Socket>();
            _socket.Config = new SocketConfiguration
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
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
            Debug.Log("CLIENT::Shutdown");
            _snm.Unsubscribe(Send);
        }
        #endregion

        public void Shutdown()
        {
            _socket.Close();
        }
        public void Send(ANetworkMessage message)
        {
            Debug.Log("CLIENT::Sending data");
            _socket.Send(_connectionId, 0,message);
        }

        private void OnStart()
        {
            _connectionId = _socket.OpenConnection(Configuration);
        }
        private void OnBroadcastEvent(int connection)
        {
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log("CLIENT::Connected to host");
            _connectionId = connection;
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
            Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection, _socket.Id));
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
            ApplicationManager.Singleton.LoadScene("MainMenu");
            Destroy(gameObject);
        }
        private void OnClose()
        {
            Destroy(gameObject);
        }
    }
}