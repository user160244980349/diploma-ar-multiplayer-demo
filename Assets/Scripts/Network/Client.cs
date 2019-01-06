using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        public static Client Instance { get; private set; }

        private ClientState _state;
        private int _socketId;
        private int _connectionId;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            _state = ClientState.Down;
        }

        private void Start()
        {

        }

        private void Update()
        {
            
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
            _socketId = NetworkManager.Instance.OpenSocket(sc);
        }

        public void Connect(ConnectionConfiguration cc)
        {
            _state = ClientState.Connecting;
            NetworkManager.Instance.OpenConnection(_socketId, cc);
        }

        public void Disconnect()
        {
            _state = ClientState.Disconnecting;
            NetworkManager.Instance.CloseConnection(_socketId, _connectionId);
        }

        private void OnBroadcastEvent(int connection)
        {

        }

        private void OnConnectEvent(int connection)
        {
            Debug.Log("CLIENT::Connected to host");
            _state = ClientState.Connected;
            _connectionId = connection;
            ApplicationManager.Instance.LoadScene("Playground");
        }

        private void OnDataEvent(int connection, byte[] data, int dataSize)
        {
            Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection, _socketId));
        }

        private void OnDisconnectEvent(int connection)
        {
            Debug.Log("CLIENT::Disconnected from host");
            _state = ClientState.Ready;
            ApplicationManager.Instance.LoadScene("MainMenu");
        }

        public void Send(NetworkMessage m)
        {
            Debug.Log("CLIENT::Sending data");
            NetworkManager.Instance.Send(_socketId, _connectionId, Formatter.Serialize(m), m.length);
        }

        public void Shutdown()
        {
            Debug.Log("CLIENT::Shutdown");
            _state = ClientState.Down;
            NetworkManager.Instance.CloseSocket(_socketId);
        }
    }
}