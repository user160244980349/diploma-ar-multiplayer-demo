using Network.Delegates;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Socket : MonoBehaviour
    {
        public int Id { get; private set; }
        public SocketConfiguration Configuration { get; set; }
        public bool EventsReady { get; set; }

        #region Configuration
        private QosType[] _channels;
        private int _port;
        private ushort _maxConnections;
        private ushort _maxMessagesForSend;
        private OnSocketStart _onSocketStart;
        private OnConnectEvent _onConnectEvent;
        private OnDataEvent _onDataEvent;
        private OnBroadcastEvent _onBroadcastEvent;
        private OnDisconnectEvent _onDisconnectEvent;
        private OnSocketShutdown _onSocketShutdown;
        #endregion

        private Formatter _formatter;
        private GameObject _connectionPrefab;
        private Connection[] _connections;

        private HostTopology _topology;
        private ConnectionConfig _connectionConfig;

        private const int PacketSize = 1024;
        private byte[] _packet;

        private bool _shutteddown;
        private int _activeConnections;

        private byte _error;

        #region MonoBehaviour
        private void Start()
        {
            _formatter = new Formatter();
            _connectionPrefab = (GameObject)Resources.Load("Networking/Connection");

            _channels = Configuration.channels;
            _port = Configuration.port;
            _maxConnections = Configuration.maxConnections;
            _maxMessagesForSend = Configuration.maxMessagesForSend;

            _onSocketStart = Configuration.onSocketStart;
            _onBroadcastEvent = Configuration.onBroadcastEvent;
            _onConnectEvent = Configuration.onConnectEvent;
            _onDataEvent = Configuration.onDataEvent;
            _onDisconnectEvent = Configuration.onDisconnectEvent;
            _onSocketShutdown = Configuration.onSocketDestroy;

            _connectionConfig = new ConnectionConfig();

            for (var i = 0; i < _channels.Length; i++)
                _connectionConfig.AddChannel(_channels[i]);

            _topology = new HostTopology(_connectionConfig, _maxConnections);
            Id = NetworkTransport.AddHost(_topology, _port);

            if (Id >= 0)
            {
                _onSocketStart(this);
            }
            else
            {
                _onSocketShutdown(this);
            }

            _connections = new Connection[Configuration.maxConnections];
            _packet = new byte[PacketSize];

            NetworkManager.Singleton.RegisterSocket(this);
            gameObject.name = string.Format("Socket{0}", Id);
        }
        private void Update()
        {
            if (_shutteddown)
            {
                if (_activeConnections != 0) return;
                NetworkTransport.RemoveHost(Id);
                _onSocketShutdown(this);
                return;
            }

            if (!EventsReady) return;
            EventsReady = false;
            NetworkEventType networkEvent;
            do
            {
                networkEvent = NetworkTransport.ReceiveFromHost(
                    Id,
                    out int connectionId,
                    out int channelId,
                    _packet,
                    PacketSize,
                    out int dataSize,
                    out _error
                );
                ShowErrorIfThrown();

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        OpenConnection(connectionId);
                        _onConnectEvent(connectionId);
                        break;

                    case NetworkEventType.DataEvent:
                        if (_connections[connectionId] == null) continue;
                        var message = _formatter.Deserialize(_packet);
                        message.ping = NetworkTransport.GetRemoteDelayTimeMS(Id, connectionId, message.timeStamp, out _error);
                        ShowErrorIfThrown();
                        _onDataEvent(connectionId, message);
                        break;

                    case NetworkEventType.BroadcastEvent:
                        if (_connections[connectionId] == null) continue;
                        _onBroadcastEvent(connectionId);
                        break;

                    case NetworkEventType.DisconnectEvent:
                        if (_connections[connectionId] == null) continue;
                        OnConnectionShutdown(_connections[connectionId]);
                        _onDisconnectEvent(connectionId);
                        break;

                    case NetworkEventType.Nothing:
                        break;
                }
            } while (networkEvent != NetworkEventType.Nothing);
        }
        private void OnDestroy()
        {
            NetworkManager.Singleton.UnregisterSocket(this);
        }
        #endregion

        public void Shutdown()
        {
            _shutteddown = true;
            for (var i = 0; i < _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                _connections[i].Shutdown();
            }
        }
        private void OnConnectionStart(Connection connection)
        {
            _activeConnections++;
            _connections[connection.Id] = connection;
            Debug.LogFormat(" >> Connection opened {0}", connection.Id);
        }
        private void OnConnectionShutdown(Connection connection)
        {
            _activeConnections--;
            _connections[connection.Id] = null;
            Destroy(connection.gameObject);
            Debug.LogFormat(" >> Connection closed {0}", connection.Id);
        }

        public void OpenConnection(ConnectionConfiguration cc)
        {
            cc.socketId = Id;
            cc.onConnectionStart += OnConnectionStart;
            cc.onConnectionDestroy += OnConnectionShutdown;

            var newConnection = Instantiate(_connectionPrefab, gameObject.transform);
            var connection = newConnection.GetComponent<Connection>();
            connection.Configuration = cc;
        }
        public void OpenConnection(int connectionId)
        {
            if (_connections[connectionId] != null) return;

            ConnectionConfiguration cc = new ConnectionConfiguration
            {
                socketId = Id,
                id = connectionId,
                onConnectionStart = OnConnectionStart,
                onConnectionDestroy = OnConnectionShutdown,
            };

            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection>();

            connectionScript.Incoming = true;
            connectionScript.Configuration = cc;
        }
        public void CloseConnection(int connectionId)
        {
            if (_connections[connectionId] == null) return;
            _connections[connectionId].Shutdown();
        }
        public void Send(int connectionId, int channelId, ANetworkMessage message)
        {
            if (_connections[connectionId] == null) return;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            _connections[connectionId].QueueMessage(channelId, packet);
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}
