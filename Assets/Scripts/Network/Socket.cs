using Network.Messages;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Socket : MonoBehaviour
    {
        public int Id { get; private set; }
        public bool EventsReady { get; set; }
        public SocketState State { get; private set; }
        public NetworkError DisconnectError { get; private set; }
        public SocketSettings Settings { get; set; }

        private GameObject _connectionPrefab;
        private Queue<MessageWrapper> _messages;
        private Formatter _formatter;
        private HostTopology _topology;
        private ConnectionConfig _connectionConfig;
        private ushort _maxConnections;
        private Connection[] _connections;
        private QosType[] _channels;
        private int _port;
        private int _packetSize;
        private byte[] _packet;
        private byte[] _broadcastPacket;
        private int _connectionsCount;

        #region MonoBehaviour
        private void Start()
        {
            _formatter = new Formatter();
            _messages = new Queue<MessageWrapper>();

            _connectionPrefab = Resources.Load("Networking/Connection") as GameObject;

            _channels = Settings.channels;
            _port = Settings.port;
            _maxConnections = (ushort)(Settings.maxConnections + 1);
            _packetSize = Settings.packetSize;
            _packet = new byte[_packetSize];
            _broadcastPacket = new byte[_packetSize];
            _connections = new Connection[_maxConnections];

            _connectionConfig = new ConnectionConfig
            {
                ConnectTimeout = 100,
                MaxConnectionAttempt = 20,
                DisconnectTimeout = 2000,
            };

            for (var i = 0; i < _channels.Length; i++)
                _connectionConfig.AddChannel(_channels[i]);

            _topology = new HostTopology(_connectionConfig, _maxConnections);

            gameObject.name = string.Format("Socket{0}", Id);
            State = SocketState.ReadyToOpen;
        }
        private void Update()
        {
            ManageConnections();
            ManageSocket();
        }
        #endregion

        public bool OpenConnection(string ip, int port)
        {
            if (_connections[0] != null && State != SocketState.Up) return false;
            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection>();
            connectionScript.Settings = new ConnectionSettings
            {
                socketId = Id,
                ip = ip,
                port = port,
                sendRate = 0.02f,
                connectDelay = 0.5f,
                disconnectDelay = 0.5f,
            };
            _connections[0] = connectionScript;
            return true;
        }
        public bool CloseConnection(int connectionId)
        {
            if (_connections[connectionId] != null) return false;
            _connections[connectionId].Disconnect(false);
            return true;
        }
        public bool Send(int connectionId, int channelId, ANetworkMessage message)
        {
            if (_connections[connectionId] != null && State != SocketState.Up) return false;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            return _connections[connectionId].QueueMessage(channelId, packet);
        }
        public bool PollMessage(out MessageWrapper wrapper)
        {
            if (_messages.Count == 0)
            {
                wrapper = new MessageWrapper();
                return false;
            }
            wrapper = _messages.Dequeue();
            return true;
        }
        public void ReceiveBroadcast(int key)
        {
            NetworkTransport.SetBroadcastCredentials(Id, key, 1, 0, out byte error);
            ParseError(error, NetworkEventType.BroadcastEvent);
        }
        public bool StartBroadcast(int key, int port, ANetworkMessage message)
        {
            if (State != SocketState.Up) return false;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            var broabcastAccepted = NetworkTransport.StartBroadcastDiscovery(Id, port, key, 1, 0, packet, packet.Length, 10, out byte error);
            ParseError(error, NetworkEventType.BroadcastEvent);
            return broabcastAccepted;
        }
        public bool StopBroadcast()
        {
            if (State != SocketState.Up && !NetworkTransport.IsBroadcastDiscoveryRunning()) return false;
            NetworkTransport.StopBroadcastDiscovery();
            return true;
        }
        public bool Open()
        {
            if (State != SocketState.ReadyToOpen) return false;
            State = SocketState.Opening;
            return true;
        }
        public bool Up()
        {
            if (State != SocketState.Opened) return false;
            State = SocketState.Up;
            return true;
        }
        public void Close()
        {
            State = SocketState.Closing;
            for (var i = 0; i < _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                _connections[i].Disconnect(false);
            }
        }
        public bool ConnectionReadyForSend(int connectionId)
        {
            if (_connections[connectionId] != null && State != SocketState.Up) return false;
            _connections[connectionId].ReadyForSend = true;
            return true;
        }

        private void ManageConnections()
        {
            for (var i = 0; i < _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                switch (_connections[i].State)
                {
                    case ConnectionState.ReadyToConnect:
                    {
                        _connectionsCount++;
                        _connections[i].Connect();
                        break;
                    }
                    case ConnectionState.Connected:
                    {
                        var wrapper = new MessageWrapper
                        {
                            message = new Connect(),
                            connection = i,
                            ip = _connections[i].Ip,
                            port = _connections[i].Port,
                        };
                        _messages.Enqueue(wrapper);
                        _connections[i].Up();
                        break;
                    }
                    case ConnectionState.Disconnected:
                    {
                        _connectionsCount--;
                        var wrapper = new MessageWrapper
                        {
                            message = new Disconnect(),
                            connection = i,
                            ip = _connections[i].Ip,
                            port = _connections[i].Port,
                        };
                        _messages.Enqueue(wrapper);
                        Destroy(_connections[i].gameObject);
                        break;
                    }
                }
            }
        }
        private void ManageSocket()
        {
            switch (State)
            {
                case SocketState.ReadyToOpen:
                {
                    break;
                }
                case SocketState.Opening:
                {
                    Id = NetworkTransport.AddHost(_topology, _port);
                    if (Id < 0)
                    {
                        State = SocketState.Closed;
                        break;
                    }
                    State = SocketState.Opened;
                    gameObject.name = string.Format("Socket{0}", Id);
                    NetworkManager.Singleton.RegisterSocket(this);
                    break;
                }
                case SocketState.Opened:
                {
                    break;
                }
                case SocketState.Up:
                {
                    ParseMessages();
                    break;
                }
                case SocketState.Closing:
                {
                    if (_connectionsCount != 0) break;
                    State = SocketState.Closed;
                    NetworkManager.Singleton.UnregisterSocket(this);
                    NetworkTransport.RemoveHost(Id);
                    break;
                }
                case SocketState.Closed:
                {
                    break;
                }
            }
        }
        private void ParseMessages()
        {
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
                    _packetSize,
                    out int dataSize,
                    out byte error
                );
                ParseError(error, networkEvent);

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                    {
                        if (_connections[0] != null &&
                            _connections[0].State == ConnectionState.WaitingConfirm)
                        {
                            _connections[connectionId] = _connections[0];
                            _connections[connectionId].Confirm();
                            _connections[0] = null;
                            break;
                        }
                        IncomingOpenConnection(connectionId);
                        break;
                    }
                    case NetworkEventType.DataEvent:
                    {
                        var wrapper = new MessageWrapper
                        {
                            message = _formatter.Deserialize(_packet),
                            connection = connectionId,
                            ip = _connections[connectionId].Ip,
                            port = _connections[connectionId].Port,
                        };
                        wrapper.ping = NetworkTransport.GetRemoteDelayTimeMS(Id, connectionId, wrapper.message.timeStamp, out error);
                        ParseError(error, NetworkEventType.DataEvent);
                        _messages.Enqueue(wrapper);
                        break;
                    }
                    case NetworkEventType.BroadcastEvent:
                    {
                        NetworkTransport.GetBroadcastConnectionMessage(Id, _broadcastPacket, _packetSize, out int size, out error);
                        ParseError(error, NetworkEventType.BroadcastEvent);
                        var wrapper = new MessageWrapper
                        {
                            message = _formatter.Deserialize(_broadcastPacket),
                        };
                        NetworkTransport.GetBroadcastConnectionInfo(Id, out wrapper.ip, out wrapper.port, out error);
                        ParseError(error, NetworkEventType.BroadcastEvent);
                        _messages.Enqueue(wrapper);
                        break;
                    }
                    case NetworkEventType.DisconnectEvent:
                    {
                        if (_connections[connectionId] != null)
                        {
                            IncomingCloseConnection(connectionId);
                            break;
                        }
                        IncomingCloseConnection(0);
                        break;
                    }
                }
            }
            while (networkEvent != NetworkEventType.Nothing);
        }
        private void IncomingOpenConnection(int connectionId)
        {
            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection>();
            connectionScript.Settings = new ConnectionSettings
            {
                id = connectionId,
                socketId = Id,
                sendRate = 0.02f,
            };
            _connections[connectionId] = connectionScript;
        }
        private void IncomingCloseConnection(int connectionId)
        {
            _connections[connectionId].Disconnect(true);
        }
        private void ParseError(byte rawError, NetworkEventType messageType)
        {
            var error = (NetworkError)rawError;
            if (error != NetworkError.Ok)
            {
                if (error == NetworkError.Timeout) DisconnectError = error;
                Debug.LogErrorFormat("NetworkError {0}", error);
            }
        }
    }
}
