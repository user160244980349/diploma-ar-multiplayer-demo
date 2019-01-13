using Network.Configurations;
using Network.Delegates;
using Network.Messages;
using Network.States;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Socket : MonoBehaviour
    {
        public int Id { get; private set; }
        public SocketConfiguration Configuration { get; set; }
        public bool EventsReady { get; set; }
        public NetworkUnitState State { get; private set; }
        public OnSocketStart OnSocketOpened;
        public OnConnectEvent OnConnected;
        public OnDataEvent OnDataReceived;
        public OnBroadcastEvent OnBroadcastReceived;
        public OnDisconnectEvent OnDisconnected;
        public OnSocketShutdown OnSocketClosed;

        #region Configuration
        private QosType[] _channels;
        private int _port;
        private ushort _maxConnections;
        private int _packetSize;
        #endregion

        private Formatter _formatter;
        private GameObject _connectionPrefab;
        private Connection[] _connections;
        private HostTopology _topology;
        private ConnectionConfig _connectionConfig;
        private byte[] _packet;
        private int _activeConnections;
        private byte _error;

        #region MonoBehaviour
        private void Start()
        {
            Debug.LogFormat("Socket opened {0}", Id);

            _port = Configuration.port;
            _channels = Configuration.channels;
            _maxConnections = Configuration.maxConnections;
            _packetSize = Configuration.packetSize;

            _packet = new byte[_packetSize];
            _connections = new Connection[_maxConnections];

            _formatter = new Formatter();
            _connectionPrefab = (GameObject)Resources.Load("Networking/Connection");

            _connectionConfig = new ConnectionConfig();
            for (var i = 0; i < _channels.Length; i++)
                _connectionConfig.AddChannel(_channels[i]);

            _topology = new HostTopology(_connectionConfig, _maxConnections);

            gameObject.name = string.Format("Socket{0}", Id);
        }
        private void Update()
        {
            switch (State)
            {
                case NetworkUnitState.StartingUp:
                    Id = NetworkTransport.AddHost(_topology, _port);
                    NetworkManager.Singleton.RegisterSocket(this);
                    State = NetworkUnitState.Up;
                    OnSocketOpened(this);
                    break;

                case NetworkUnitState.Up:
                    if (!EventsReady) break;
                    EventsReady = false;
                    NetworkEventType networkEvent;
                    do
                    {
                        networkEvent = NetworkTransport.ReceiveFromHost(
                            Id,
                            out int connectionId,
                            out int channelId,
                            _packet,
                            _packetSize,
                            out int dataSize,
                            out _error
                        );
                        ShowErrorIfThrown();

                        switch (networkEvent)
                        {
                            case NetworkEventType.ConnectEvent:
                                OpenConnection(connectionId);
                                break;

                            case NetworkEventType.DataEvent:
                                if (_connections[connectionId] == null) return;
                                var message = _formatter.Deserialize(_packet);
                                message.ping = NetworkTransport.GetRemoteDelayTimeMS(Id, connectionId, message.timeStamp, out _error);
                                ShowErrorIfThrown();
                                OnDataReceived(connectionId, message);
                                break;

                            case NetworkEventType.BroadcastEvent:
                                if (_connections[connectionId] == null) return;
                                OnBroadcastReceived(connectionId);
                                break;

                            case NetworkEventType.DisconnectEvent:
                                _connections[connectionId].IncomingDisconnection = true;
                                CloseConnection(connectionId);
                                break;

                            case NetworkEventType.Nothing:
                                break;
                        }
                    } while (networkEvent != NetworkEventType.Nothing);
                    break;

                case NetworkUnitState.ShuttingDown:
                    if (_activeConnections != 0) break;
                    NetworkManager.Singleton.UnregisterSocket(this);
                    NetworkTransport.RemoveHost(Id);
                    State = NetworkUnitState.Down;
                    break;

                case NetworkUnitState.Down:
                    OnSocketClosed(Id);
                    Destroy(gameObject);
                    break;
            }
        }
        private void OnDestroy()
        {
            Debug.LogFormat("Socket closed {0}", Id);
        }
        #endregion

        public void Close()
        {
            State = NetworkUnitState.ShuttingDown;
            for (var i = 0; i < _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                _connections[i].Disconnect();
            }
        }
        public void OpenConnection(ConnectionConfiguration cc)
        {
            cc.socketId = Id;

            var connectionObject = Instantiate(_connectionPrefab, gameObject.transform);
            var connectionScript = connectionObject.GetComponent<Connection>();
            connectionScript.Configuration = cc;
            connectionScript.OnConnect = OnConnect;
            connectionScript.OnDisconnect = OnDisconnect;
        }
        public void Send(int connectionId, int channelId, ANetworkMessage message)
        {
            if (_connections[connectionId] == null) return;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            _connections[connectionId].QueueMessage(channelId, packet);
        }
        public void CloseConnection(int connectionId)
        {
            if (_connections[connectionId] == null) return;
            _connections[connectionId].Disconnect();
        }

        private void OpenConnection(int connectionId)
        {
            if (_connections[connectionId] != null) return;

            ConnectionConfiguration cc = new ConnectionConfiguration
            {
                socketId = Id,
                id = connectionId,
            };

            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection>();
            connectionScript.IncomingConnection = true;
            connectionScript.Configuration = cc;
            connectionScript.OnConnect = OnConnect;
            connectionScript.OnDisconnect = OnDisconnect;
        }
        private void OnConnect(Connection connection)
        {
            if (0 < connection.Id && connection.Id < _maxConnections + 1)
            {
                _activeConnections++;
                _connections[connection.Id] = connection;
                OnConnected(connection.Id);
            }
            else
                connection.Disconnect();
        }
        private void OnDisconnect(int connectionId)
        {
            _activeConnections--;
            _connections[connectionId] = null;
            OnDisconnected(connectionId);
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}
