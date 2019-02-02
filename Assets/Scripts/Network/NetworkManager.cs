using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }
        public bool HostBooted { get { if (_host != null) return true; return false; } }
        public bool ClientBooted { get { if (_client != null) return true; return false; } }

        private GameObject _hostPrefab;
        private GameObject _clientPrefab;

        private Host _host;
        private Client _client;
        private Socket[] _sockets;
        private ushort _maxSockets = 16;

        public void RegisterSocket(Socket socket)
        {
            _sockets[socket.Id] = socket;
        }
        public void UnregisterSocket(Socket socket)
        {
            _sockets[socket.Id] = null;
        }
        public void SpawnHost()
        {
            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<Host>();
        }
        public void DespawnHost()
        {
            _host.Shutdown();
        }
        public void SpawnClient()
        {
            var clientObject = Instantiate(_clientPrefab, gameObject.transform);
            _client = clientObject.GetComponent<Client>();
        }
        public void DespawnClient()
        {
            _client.Shutdown();
        }

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
                ConnectionReadyForSend = ConnectionReadyForSend,
            };
            NetworkTransport.Init(config);

            _hostPrefab = (GameObject)Resources.Load("Networking/NetworkHost");
            _clientPrefab = (GameObject)Resources.Load("Networking/NetworkClient");
        }
        private void Update()
        {
            if (_client != null)
            {
                switch (_client.State)
                {
                    case ClientState.DownWithError:
                    {
                        var broadcastKey = _client.BroadcastKey;
                        Destroy(_client.gameObject);
                        
                        var hostObject = Instantiate(_hostPrefab, gameObject.transform);
                        _host = hostObject.GetComponent<Host>();
                        _host.BroadcastKey = broadcastKey;
                        break;
                    }
                    case ClientState.Down:
                    {
                        Destroy(_client.gameObject);
                        break;
                    }
                }
            }

            if (_host != null)
            {
                switch (_host.State)
                {
                    case HostState.Down:
                    {
                        Destroy(_host.gameObject);
                        break;
                    }
                }
            }
        }
        private void OnDestroy()
        {
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