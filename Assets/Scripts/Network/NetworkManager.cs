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
        private int _maxSockets = 16;
        private Socket[] _sockets;

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

            _host.HostConfig = new HostConfiguration
            {
                onHostStart = HostStart,
                onHostShutdown = HostShutdown,
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
            _client = clientObject.GetComponent<Client>();

            _client.ClientConfig = new ClientConfiguration
            {
                onClientStart = ClientStart,
                onClientShutdown = ClientShutdown,
            };

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
        private void ClientShutdown(Client networkClient)
        {
            ClientBooted = false;
            Destroy(_client.gameObject);
            _client = null;

            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
        private void HostStart(Host networkHost)
        {
            ApplicationManager.Singleton.LoadScene("Playground");
            HostBooted = true;
        }
        private void HostShutdown(Host networkHost)
        {
            HostBooted = false;
            Destroy(_host.gameObject);
            _host = null;

            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
    }
}