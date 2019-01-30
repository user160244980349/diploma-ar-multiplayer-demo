using Network.Messages;
using Network.Connection;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Types;

namespace Network.Socket
{
    public class NetworkSocket : MonoBehaviour
    {
        public int Id { get; private set; }
        public bool EventsReady { get; set; }
        public SocketState State { get; private set; }
        public NetworkError Error { get { return (NetworkError)_error; } }
        public SocketSettings Settings { get; set; }

        private GameObject _connectionPrefab;

        private Queue<MessageWrapper> _messages;
        private Formatter _formatter;
        private HostTopology _topology;
        private ConnectionConfig _connectionConfig;
        private ushort _maxConnections;
        private Connection.NetworkConnection[] _connections;
        private QosType[] _channels;
        private int _port;
        private int _packetSize;
        private byte[] _packet;
        private int _activeConnections;
        private byte _error;

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
            _connections = new Connection.NetworkConnection[_maxConnections];

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
        
        public void OpenConnection(string ip, int port)
        {
            if (_connections[0] != null && State != SocketState.Up) return;
            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection.NetworkConnection>();
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
        }
        public void CloseConnection(int connectionId)
        {
            if (State != SocketState.Up) return;
            if (_connections[connectionId].State != ConnectionState.Up) return;
            _connections[connectionId].Disconnect(false);
        }
        public void Send(int connectionId, int channelId, ANetworkMessage message)
        {
            if (State != SocketState.Up) return;
            if (_connections[connectionId].State != ConnectionState.Up) return;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            _connections[connectionId].QueueMessage(channelId, packet);
        }
        public bool PollMessage(out MessageWrapper wrapper)
        {
            if (_messages.Count == 0)
            {
                wrapper = null;
                return false;
            }
            wrapper = _messages.Dequeue();
            return true;
        }
        public void ReceiveBroadcast(int key)
        {
            NetworkTransport.SetBroadcastCredentials(Id, key, 1, 0, out _error);
            ShowErrorIfThrown();
        }
        public void StartBroadcast(int key, int port, ANetworkMessage message)
        {
            if (State != SocketState.Up) return;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var packet = _formatter.Serialize(message);
            NetworkTransport.StartBroadcastDiscovery(Id, port, key, 1, 0, packet, packet.Length, 10, out _error);
            ShowErrorIfThrown();
        }
        public void StopBroadcast()
        {
            if (State != SocketState.Up) return;
            NetworkTransport.StopBroadcastDiscovery();
        }
        public void Open()
        {
            if (State != SocketState.ReadyToOpen) return;
            State = SocketState.Opening;
        }
        public void Up()
        {
            if (State != SocketState.Opened) return;
            State = SocketState.Up;
        }
        public void Close()
        {
            if (State != SocketState.Up) return;
            State = SocketState.Closing;
            for (var i = 0; i < _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                _connections[i].Disconnect(false);
            }
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
                        _connections[i].Connect();
                        break;
                    }

                    case ConnectionState.Connected:
                    {
                        _activeConnections++;
                        var wrapper = new MessageWrapper
                        {
                            message = new Connect(),
                            connection = i,
                            ip = "",
                            ping = 0,
                            port = 0,
                        };
                        _messages.Enqueue(wrapper);
                        _connections[i].Up();
                        break;
                    }

                    case ConnectionState.Disconnected:
                    {
                        _activeConnections--;
                        var wrapper = new MessageWrapper
                        {
                            message = new Disconnect(),
                            connection = i,
                            ip = "",
                            ping = 0,
                            port = 0,
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
                    State = SocketState.Opened;
                    Id = NetworkTransport.AddHost(_topology, _port);
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
                    if (_activeConnections != 0) break;
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
                    out _error
                );
                ShowErrorIfThrown();

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
                        if (_connections[connectionId] == null)
                            IncomingOpenConnection(connectionId);
                        break;
                    }

                    case NetworkEventType.DataEvent:
                    {
                        var wrapper = new MessageWrapper
                        {
                            message = _formatter.Deserialize(_packet),
                            connection = connectionId,
                        };

                        wrapper.ping = NetworkTransport.GetRemoteDelayTimeMS(Id, connectionId, wrapper.message.timeStamp, out _error);
                        ShowErrorIfThrown();

                        NetworkTransport.GetConnectionInfo(Id, connectionId, out wrapper.ip, out wrapper.port, out NetworkID network, out NodeID end, out _error);
                        ShowErrorIfThrown();

                        _messages.Enqueue(wrapper);
                        break;
                    }

                    case NetworkEventType.BroadcastEvent:
                    {
                        NetworkTransport.GetBroadcastConnectionMessage(Id, _packet, _packetSize, out int size, out _error);

                        var wrapper = new MessageWrapper
                        {
                            message = _formatter.Deserialize(_packet),
                            connection = connectionId,
                            ping = 0,
                        };

                        NetworkTransport.GetBroadcastConnectionInfo(Id, out wrapper.ip, out wrapper.port, out _error);
                        ShowErrorIfThrown();

                        _messages.Enqueue(wrapper);
                        break;
                    }

                    case NetworkEventType.DisconnectEvent:
                    {
                        IncomingCloseConnection(connectionId);
                        break;
                    }

                    case NetworkEventType.Nothing:
                    {
                        break;
                    }
                }
            }
            while (networkEvent != NetworkEventType.Nothing);
        }
        private void IncomingOpenConnection(int connectionId)
        {
            var connectionObject = Instantiate(_connectionPrefab, transform);
            var connectionScript = connectionObject.GetComponent<Connection.NetworkConnection>();
            connectionScript.Settings = new ConnectionSettings
            {
                id = connectionId,
                socketId = Id,
                sendRate = 0.02f,
                connectDelay = 0.5f,
                disconnectDelay = 0.5f,
            };
            _connections[connectionId] = connectionScript;
        }
        private void IncomingCloseConnection(int connectionId)
        {
            _connections[connectionId].Disconnect(true);
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}
