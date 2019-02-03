using Events;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }

        private GameObject _hostPrefab;
        private GameObject _clientPrefab;

        private Host _host;
        private Client _client;
        private Socket[] _sockets;
        private ushort _maxSockets = 16;

        private void OnRegisterSocket(object info)
        {
            var socket = info as Socket;
            _sockets[socket.Id] = socket;
        }
        private void OnUnregisterSocket(object info)
        {
            var socket = info as Socket;
            _sockets[socket.Id] = null;
        }
        private void OnSwitch(object info)
        {
            _client.Close();

            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<Host>();
            _host.BroadcastKey = (int)info;
        }
        private void OnStartHost(object info)
        {
            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<Host>();
        }
        private void OnDestroyHost(object info)
        {
            if (_host != null) _host.Close();
        }
        private void OnStartClient(object info)
        {
            var clientObject = Instantiate(_clientPrefab, gameObject.transform);
            _client = clientObject.GetComponent<Client>();
        }
        private void OnDestroyClient(object info)
        {
            if (_client != null) _client.Close();
        }

        private void Awake()
        {
            name = "NetworkManager";
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

            _hostPrefab = (GameObject)Resources.Load("Networking/NetworkHost");
            _clientPrefab = (GameObject)Resources.Load("Networking/NetworkClient");

            _sockets = new Socket[_maxSockets];

            var config = new GlobalConfig
            {
                MaxHosts = _maxSockets,
                NetworkEventAvailable = NetworkEventAvailable,
                ConnectionReadyForSend = ConnectionReadyForSend,
            };
            NetworkTransport.Init(config);

            EventManager.Singleton.Subscribe(GameEventType.Switch, OnSwitch);
            EventManager.Singleton.Subscribe(GameEventType.StartHost, OnStartHost);
            EventManager.Singleton.Subscribe(GameEventType.DestroyHost, OnDestroyHost);
            EventManager.Singleton.Subscribe(GameEventType.StartClient, OnStartClient);
            EventManager.Singleton.Subscribe(GameEventType.DestroyClient, OnDestroyClient);
            EventManager.Singleton.Subscribe(GameEventType.RegisterSocket, OnRegisterSocket);
            EventManager.Singleton.Subscribe(GameEventType.UnregisterSocket, OnUnregisterSocket);
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Unsubscribe(GameEventType.Switch, OnSwitch);
            EventManager.Singleton.Unsubscribe(GameEventType.StartHost, OnStartHost);
            EventManager.Singleton.Unsubscribe(GameEventType.DestroyHost, OnDestroyHost);
            EventManager.Singleton.Unsubscribe(GameEventType.StartClient, OnStartClient);
            EventManager.Singleton.Unsubscribe(GameEventType.DestroyClient, OnDestroyClient);
            EventManager.Singleton.Unsubscribe(GameEventType.RegisterSocket, OnRegisterSocket);
            EventManager.Singleton.Unsubscribe(GameEventType.UnregisterSocket, OnUnregisterSocket);
            NetworkTransport.Shutdown();
        }

        private void NetworkEventAvailable(int socketId)
        {
            _sockets[socketId].EventsReady = true;
        }
        private void ConnectionReadyForSend(int socketId, int connectionId)
        {
            _sockets[socketId].ConnectionReadyForSend(connectionId);
        }
    }
}