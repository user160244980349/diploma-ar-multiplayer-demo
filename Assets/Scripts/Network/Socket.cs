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
        public SocketState State { get; private set; }
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
            _port = Configuration.port;
            _channels = Configuration.channels;
            _maxConnections = Configuration.maxConnections;
            _packetSize = Configuration.packetSize;

            _packet = new byte[_packetSize];
            _connections = new Connection[_maxConnections + 1];

            _formatter = new Formatter();
            _connectionPrefab = (GameObject)Resources.Load("Networking/Connection");

            _connectionConfig = new ConnectionConfig {
                ConnectTimeout = 100,
                MaxConnectionAttempt = 20,
                DisconnectTimeout = 2000,
            };
            for (var i = 0; i < _channels.Length; i++)
                _connectionConfig.AddChannel(_channels[i]);

            _topology = new HostTopology(_connectionConfig, _maxConnections);

            Id = NetworkTransport.AddHost(_topology, _port);
            NetworkManager.Singleton.RegisterSocket(this);

            gameObject.name = string.Format("Socket{0}", Id);
            //Debug.LogFormat("Socket opened {0}", Id);
        }
        private void Update()
        {
            switch (State)
            {
                case SocketState.StartingUp:
                {
                    State = SocketState.Up;
                    OnSocketOpened(this);
                    break;
                }

                case SocketState.OpeningConnection:
                {

                    break;
                }

                case SocketState.Up:
                {
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
                            {
                                OpenConnection(new ConnectionConfiguration(), connectionId, true);
                                break;
                            }

                            case NetworkEventType.DataEvent:
                            {
                                var message = _formatter.Deserialize(_packet);
                                message.ping = NetworkTransport.GetRemoteDelayTimeMS(Id, connectionId, message.timeStamp, out _error);
                                ShowErrorIfThrown();
                                OnDataReceived(connectionId, message);
                                break;
                            }

                            case NetworkEventType.BroadcastEvent:
                            {
                                NetworkTransport.GetBroadcastConnectionMessage(Id, _packet, _packetSize, out int size, out _error);
                                var message = _formatter.Deserialize(_packet);
                                message.ping = NetworkTransport.GetRemoteDelayTimeMS(Id, connectionId, message.timeStamp, out _error);
                                ShowErrorIfThrown();
                                ConnectionConfiguration cc;
                                NetworkTransport.GetBroadcastConnectionInfo(Id, out cc.ip, out cc.port, out _error);
                                ShowErrorIfThrown();
                                OnBroadcastReceived(cc, message);
                                break;
                            }

                            case NetworkEventType.DisconnectEvent:
                            {
                                CloseConnection(connectionId, true);
                                break;
                            }

                            case NetworkEventType.Nothing:
                                break;
                        }
                    } while (networkEvent != NetworkEventType.Nothing);
                    break;
                }

                case SocketState.ShuttingDown:
                {
                    if (_activeConnections != 0) break;
                    NetworkManager.Singleton.UnregisterSocket(this);
                    NetworkTransport.RemoveHost(Id);
                    State = SocketState.Down;
                    break;
                }

                case SocketState.Down:
                {
                    OnSocketClosed(Id);
                    Destroy(gameObject);
                    break;
                }
            }
        }
        private void OnDestroy()
        {
            //Debug.LogFormat("Socket closed {0}", Id);
        }
        #endregion

        public void OpenConnection(ConnectionConfiguration cc)
        {
            OpenConnection(cc, 0, false);
        }
        public void Send(int connectionId, int channelId, ANetworkMessage message)
        {
            if (_connections[connectionId] == null) return;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            _connections[connectionId].QueueMessage(channelId, packet);
        }
        public void SetBroadcastReceiveKey(int key)
        {
            Debug.LogFormat("SOCKET::Broadcast key is {0}", key);
            NetworkTransport.SetBroadcastCredentials(Id, key, 1, 0, out _error);
            ShowErrorIfThrown();
        }
        public void StartBroadcast(int key, int port, ANetworkMessage message)
        {
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            NetworkTransport.StartBroadcastDiscovery(Id, port, key, 1, 0, packet, packet.Length, 10, out _error);
            ShowErrorIfThrown();
        }
        public void StopBroadcast()
        {
            NetworkTransport.StopBroadcastDiscovery();
        }
        public void CloseConnection(int connectionId)
        {
            CloseConnection(connectionId, false);
        }
        private void OpenConnection(ConnectionConfiguration cc, int connectionId, bool incoming)
        {
            if (_connections[connectionId] != null)
            {
                _connections[connectionId].Confirmed = true;
                return;
            }
            State = SocketState.OpeningConnection;

            ConnectionBindings cb = new ConnectionBindings
            {
                socketId = Id,
                id = connectionId,
            };

            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection>();
            connectionScript.IncomingConnection = incoming;
            connectionScript.Bindings = cb;
            connectionScript.Configuration = cc;
            connectionScript.OnConnect = OnConnect;
            connectionScript.OnWaitingConfirm = OnWaitingConfirm;
            connectionScript.OnDisconnect = OnDisconnect;
        }
        private void CloseConnection(int connectionId, bool incoming)
        {
            if (_connections[connectionId] == null) return;
            _connections[connectionId].IncomingDisconnection = incoming;
            _connections[connectionId].Disconnect();
        }
        private void OnWaitingConfirm(Connection connection)
        {
            State = SocketState.Up;
            _activeConnections++;
            _connections[connection.Id] = connection;
        }
        private void OnConnect(int connectionId)
        {
            OnConnected(connectionId);
        }
        private void OnDisconnect(int connectionId)
        {
            _activeConnections--;
            _connections[connectionId] = null;
            OnDisconnected(connectionId);
        }
        public void Close()
        {
            State = SocketState.ShuttingDown;
            for (var i = 0; i <= _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                _connections[i].Disconnect();
            }
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}
