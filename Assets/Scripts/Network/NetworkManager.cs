using System;
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

        public void OnFallback(int broadcastKey)
        {
            _fallbackMode = true;
            _broadcastKey = broadcastKey;
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
            _sockets[socketId].EventsReady = true;
        }

        public void SpawnHost()
        {
            if (HostBooted) return;

            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<Host>();
            _host.OnStart = OnHostStart;
            _host.OnShutdown = OnHostShutdown;
        }
        public void SpawnHost(int key)
        {
            if (HostBooted) return;

            var hostObject = Instantiate(_hostPrefab, gameObject.transform);
            _host = hostObject.GetComponent<Host>();
            _host.BroadcastKey = key;
            _host.OnStart = OnHostStart;
            _host.OnShutdown = OnHostShutdown;
            _fallbackMode = false;
        }
        public void DespawnHost()
        {
            _host.Shutdown();
        }
        private void OnHostStart(Host networkHost)
        {
            if (_fallbackMode)
            {
                _fallbackMode = false;
            }
            else { }
            HostBooted = true;
        }
        private void OnHostShutdown()
        {
            HostBooted = false;
            _host = null;
        }

        // ApplicationManager.Singleton.LoadScene("Playground");
        // ApplicationManager.Singleton.LoadScene("MainMenu");
        // ApplicationManager.Singleton.LoadScene("Loading");

        public void SpawnClient()
        {
            if (ClientBooted) return;

            var clientObject = Instantiate(_clientPrefab, gameObject.transform);
            _client = clientObject.GetComponent<Client>();
            _client.OnStart = OnClientStart;
            _client.OnFallback = OnFallback;
            _client.OnShutdown = OnClientShutdown;

            _client.ConnectionConfig = new ConnectionConfiguration
            {
                ip = "192.168.1.2",
                port = 8000,
            };
        }
        public void DespawnClient()
        {
            _client.Shutdown();
        }
        private void OnClientStart(Client networkClient)
        {
            ClientBooted = true;
        }
        private void OnClientShutdown()
        {
            _client = null;
            if (_fallbackMode)
            {
                SpawnHost(_broadcastKey);
            }
            else { }
        }
    }
}