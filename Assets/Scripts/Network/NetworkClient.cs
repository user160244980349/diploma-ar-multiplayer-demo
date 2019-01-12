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
        public NetworkClientConfiguration ClientConfiguration { get; set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;

        private GameObject _socketPrefab;

        private Socket _socket;
        private int _connectionId;

        private bool _shutteddown;
        private float _timeToShutdown = 1f;

        #region Delegates
        public delegate void OnNetworkClientStart();
        public delegate void OnNetworkClientShutdown();
        #endregion

        #region Configurations
        private OnNetworkClientStart _onNetworkClientStart;
        private OnNetworkClientShutdown _onNetworkClientShutdown;
        #endregion

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            gameObject.name = "NetworkClient";
        }
        private void Start()
        {
            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();

            _socketPrefab = (GameObject)Resources.Load("Networking/Socket");

            _onNetworkClientStart = ClientConfiguration.onNetworkClientStart;
            _onNetworkClientShutdown = ClientConfiguration.onNetworkClientShutdown;

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            var socketScript = socketObject.GetComponent<Socket>();
            socketScript.Configuration = new SocketConfiguration
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                maxConnections = 16,
                maxMessagesForSend = 16,
                onSocketStart = OnSocketStart,
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent,
                onSocketDestroy = OnSocketShutdown,
            };

            _snm.Subscribe(Send);
            _onNetworkClientStart();
            Debug.Log("CLIENT::Boot");
        }
        private void Update()
        {
            if (_shutteddown)
            {
                if (_socket != null) return;
                _timeToShutdown -= Time.deltaTime;
                // Debug.Log("Shutting down client");
                if (_timeToShutdown > 0) return;
                Debug.Log("CLIENT::Shutdown");
                _onNetworkClientShutdown();
                return;
            }
        }
        private void OnDestroy()
        {
            _snm.Unsubscribe(Send);
        }
        #endregion

        public void Shutdown()
        {
            _shutteddown = true;
            _socket.Shutdown();
        }
        private void OnSocketStart(Socket socket)
        {
            _socket = socket;
            socket.OpenConnection(Configuration);
            Debug.LogFormat(" >> Socket opended {0}", socket.Id);
        }
        private void OnSocketShutdown(Socket socket)
        {
            _socket = null;
            Destroy(socket.gameObject);
            Debug.LogFormat(" >> Socket closed {0}", socket.Id);
        }

        public void Send(ANetworkMessage message)
        {
            if (_socket == null) return;
            Debug.Log("CLIENT::Sending data");
            _socket.Send(_connectionId, 0,message);
        }
        private void Disconnect()
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", _connectionId, _socket.Id));
            _socket.CloseConnection(_connectionId);
        }

        private void OnBroadcastEvent(int connection)
        {
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
            ApplicationManager.Singleton.LoadScene("Loading");
            Shutdown();
        }
    }
}