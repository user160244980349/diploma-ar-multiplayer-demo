using Network.Configurations;
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

        private Host _host;
        private Client _client;
        private Socket[] _sockets;
        private ushort _maxSockets = 16;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkManager";
            _sockets = new Socket[_maxSockets];

            var config = new GlobalConfig
            {
                MaxHosts = _maxSockets,
                NetworkEventAvailable = NetworkEventAvailable,
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
            _sockets[socketId].EventsReady = true;
        }

        public void SpawnHost()
        {
            if (HostBooted) return;

            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<Host>();
            _host.OnStart = HostStart;
            _host.OnShutdown = HostShutdown;
        }
        public void DespawnHost()
        {
            _host.Shutdown();
        }
        private void HostStart(Host networkHost)
        {
            ApplicationManager.Singleton.LoadScene("Playground");
            HostBooted = true;
        }
        private void HostShutdown()
        {
            HostBooted = false;
            _host = null;
        }

        public void SpawnClient()
        {
            if (ClientBooted) return;

            var clientObject = Instantiate(_clientPrefab, gameObject.transform);
            _client = clientObject.GetComponent<Client>();
            _client.OnStart = ClientStart;
            _client.OnShutdown = ClientShutdown;

            _client.ConnectionConfig = new ConnectionConfiguration
            {
                ip = "127.0.0.1",
                port = 8000,
            };
        }
        public void DespawnClient()
        {
            _client.Shutdown();
        }
        private void ClientStart(Client networkClient)
        {
            ClientBooted = true;
        }
        private void ClientShutdown()
        {
            ClientBooted = false;
            _client = null;
        }
    }
}