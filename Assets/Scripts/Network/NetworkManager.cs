using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }
        public bool HostBooted { get; private set; }
        public bool ClientBooted { get; private set; }

        private GameObject _hostPrefab;
        private GameObject _clientPrefab;

        private int _maxSockets = 16;
        private Socket[] _sockets;

        private NetworkHost _host;
        private NetworkClient _client;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkManager";
            _sockets = new Socket[_maxSockets];

            var config = new GlobalConfig {
                NetworkEventAvailable = NetworkEventAvailable,
                ConnectionReadyForSend = ConnectionReadyForSend,
            };
            NetworkTransport.Init(config);

            _hostPrefab = (GameObject)Resources.Load("Networking/NetworkHost");
            _clientPrefab = (GameObject)Resources.Load("Networking/NetworkClient");
        }
        private void OnDestroy()
        {
            NetworkTransport.Shutdown();
        }
        #endregion

        public void SpawnHost()
        {
            if (HostBooted) return;

            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<NetworkHost>();

            _host.HostConfiguration = new NetworkHostConfiguration
            {
                onNetworkHostStart = HostStart,
                onNetworkHostShutdown = HostShutdown,
            };
        }
        public void DespawnHost()
        {
            _host.Shutdown();
        }
        public void SpawnClient()
        {
            if (ClientBooted) return;

            var clientObject = Instantiate(_clientPrefab, gameObject.transform);
            _client = clientObject.GetComponent<NetworkClient>();

            _client.ClientConfiguration = new NetworkClientConfiguration
            {
                onNetworkClientStart = ClientStart,
                onNetworkClientShutdown = ClientShutdown,
            };

            _client.Configuration = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                port = 8000,
            };
        }
        public void DespawnClient()
        {
            _client.Shutdown();
        }

        public void RegisterSocket(Socket socket)
        {
            _sockets[socket.Id] = socket;
        }
        public void UnregisterSocket(Socket socket)
        {
            _sockets[socket.Id] = null;
        }

        private void NetworkEventAvailable(int socketId)
        {
            _sockets[socketId].EventsReady();
        }
        private void ConnectionReadyForSend(int socketId, int connectionId)
        {
            _sockets[socketId].ConnectionReady(connectionId);
        }

        private void ClientStart()
        {
            ClientBooted = true;
        }
        private void ClientShutdown()
        {
            ClientBooted = false;
            Destroy(_client.gameObject);
            _client = null;
        }
        private void HostStart()
        {
            HostBooted = true;
        }
        private void HostShutdown()
        {
            HostBooted = false;
            Destroy(_host.gameObject);
            _host = null;
        }
    }
}