using Network.Client;
using Network.Host;
using Network.Socket;
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

        private NetworkHost _host;
        private Client.NetworkClient _client;
        private NetworkSocket[] _sockets;
        private ushort _maxSockets = 16;

        private bool _fallbackMode;
        private int _broadcastKey;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkManager";
            _sockets = new NetworkSocket[_maxSockets];

            var config = new GlobalConfig
            {
                MaxHosts = _maxSockets,
                NetworkEventAvailable = NetworkEventAvailable,
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
                    case ClientState.WaitingSwitch:
                    {
                        var broadcastKey = _client.BroadcastKey;
                        Destroy(_client.gameObject);

                        var hostObject = Instantiate(_hostPrefab, gameObject.transform);
                        _host = hostObject.GetComponent<NetworkHost>();
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
                        Destroy(_client.gameObject);
                        break;
                    }
                }
            }
        }
        private void OnDestroy()
        {
            NetworkTransport.Shutdown();
        }
        #endregion

        public void RegisterSocket(NetworkSocket socket)
        {
            _sockets[socket.Id] = socket;
        }
        public void UnregisterSocket(NetworkSocket socket)
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
            _host = hostObject.GetComponent<NetworkHost>();
        }
        public void DespawnHost()
        {
            _host.Shutdown();
        }
        public void SpawnClient()
        {
            if (ClientBooted) return;

            var clientObject = Instantiate(_clientPrefab, gameObject.transform);
            _client = clientObject.GetComponent<Client.NetworkClient>();
        }
        public void DespawnClient()
        {
            _client.Shutdown();
        }

        // ApplicationManager.Singleton.LoadScene("Playground");
        // ApplicationManager.Singleton.LoadScene("MainMenu");
        // ApplicationManager.Singleton.LoadScene("Loading");
    }
}