using Events;
using Network.Messages;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Socket : MonoBehaviour
    {
        public int Id { get; private set; }
        public bool EventsReady { get; set; }
        public NetworkError DisconnectError { get; set; }

        private GameObject _connectionPrefab;

        private bool _started;
        private bool _closing;

        private QosType[] _channels;
        private int _port;
        private ushort _maxConnections;
        private int _packetSize;

        private Formatter _formatter;
        private Queue<MessageWrapper> _messages;
        private byte[] _packet;
        private byte[] _broadcastPacket;
        private Connection[] _connections;

        public bool ImmediateStart(SocketSettings settings)
        {
            if (_started) return false;
            _started = true;

            _channels = settings.channels;
            _port = settings.port;
            _maxConnections = settings.maxConnections;
            _packetSize = settings.packetSize;

            _formatter = new Formatter();
            _messages = new Queue<MessageWrapper>();

            _connectionPrefab = Resources.Load("Networking/Connection") as GameObject;

            _packet = new byte[_packetSize];
            _broadcastPacket = new byte[_packetSize];

            _connections = new Connection[_maxConnections + 1];

            var connectionConfig = new ConnectionConfig
            {
                ConnectTimeout = 100,
                MaxConnectionAttempt = 20,
                DisconnectTimeout = 2000,
            };

            for (var i = 0; i < _channels.Length; i++)
                connectionConfig.AddChannel(_channels[i]);

            var topology = new HostTopology(connectionConfig, _maxConnections);

            Id = NetworkTransport.AddHost(topology, _port);
            if (Id >= 0)
            {
                EventManager.Singleton.Publish(GameEventType.RegisterSocket, this);
                return true;
            }
            Destroy(gameObject);
            return false;
        }
        public bool Connect(string ip, int port)
        {
            var connection = NetworkTransport.Connect(Id, ip, port, 0, out byte error);
            ParseError("Failed to connect", error);
            if (connection > 0)
                return true;
            return false;
        }
        public void ConnectionReadyForSend(int id)
        {
            _connections[id].ReadyForSend = true;
        }
        public bool Send(int connection, int channel, ANetworkMessage message)
        {
            var packet = _formatter.Serialize(message);
            return _connections[connection].QueueMessage(channel, packet);
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
            ParseError("Failed to set credentials", error);
        }
        public bool StartBroadcast(int key, int port, ANetworkMessage message)
        {
            var packet = _formatter.Serialize(message);
            var broabcast = NetworkTransport.StartBroadcastDiscovery(Id, port, key, 1, 0, packet, packet.Length, 10, out byte error);
            ParseError("Failed to start broadcast", error);
            return broabcast;
        }
        public bool StopBroadcast()
        {
            if (!NetworkTransport.IsBroadcastDiscoveryRunning()) return false;
            NetworkTransport.StopBroadcastDiscovery();
            return true;
        }
        public void Disconnect(int id)
        {
            _connections[id].Disconnect();
        }
        public void Close()
        {
            _closing = true;
            for (var i = 0; i <= _maxConnections; i++)
            {
                if (_connections[i] == null) continue;
                _connections[i].Disconnect();
            }
        }

        private void Start()
        {
            name = string.Format("Socket<{0}>", Id);
        }
        private void Update()
        {
            if (_closing)
            {
                bool openedConnections = false;
                for (int i = 0; i <= _maxConnections; i++)
                {
                    if (_connections[i] == null) continue;
                    openedConnections = true;
                    break;
                }
                if (!openedConnections) Destroy(gameObject);
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
                    _packetSize,
                    out int dataSize,
                    out byte error
                );
                ParseError("Failed to poll event", error);

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                    {
                        var connectionObject = Instantiate(_connectionPrefab, transform);
                        _connections[connectionId] = connectionObject.GetComponent<Connection>();
                        _connections[connectionId].ImmediateStart(Id, connectionId);

                        var wrapper = new MessageWrapper
                        {
                            message = new Connect(),
                            connection = connectionId,
                            ip = _connections[connectionId].Ip,
                            port = _connections[connectionId].Port,
                        };

                        _messages.Enqueue(wrapper);
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
                        ParseError("Failed to get ping", error);
                        _messages.Enqueue(wrapper);
                        break;
                    }
                    case NetworkEventType.BroadcastEvent:
                    {
                        NetworkTransport.GetBroadcastConnectionMessage(Id, _broadcastPacket, _packetSize, out int size, out error);
                        ParseError("Failed to get broadcast message", error);
                        var wrapper = new MessageWrapper
                        {
                            message = _formatter.Deserialize(_broadcastPacket),
                        };
                        NetworkTransport.GetBroadcastConnectionInfo(Id, out wrapper.ip, out wrapper.port, out error);
                        ParseError("Failed to get broadcast connection info", error);
                        _messages.Enqueue(wrapper);
                        break;
                    }
                    case NetworkEventType.DisconnectEvent:
                    {
                        DisconnectError = (NetworkError)error;
                        var wrapper = new MessageWrapper
                        {
                            message = new Disconnect(),
                            connection = connectionId,
                        };
                        if (_connections[connectionId] != null)
                        {
                            wrapper.ip = _connections[connectionId].Ip;
                            wrapper.port = _connections[connectionId].Port;
                            Destroy(_connections[connectionId].gameObject);
                        }
                        _messages.Enqueue(wrapper);
                        break;
                    }
                }
            }
            while (networkEvent != NetworkEventType.Nothing);
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Publish(GameEventType.UnregisterSocket, this);
            NetworkTransport.RemoveHost(Id);
        }

        private void ParseError(string message, byte rawError)
        {
            var error = (NetworkError)rawError;
            if (error != NetworkError.Ok)
            {
                Debug.LogErrorFormat("SOCKET_{0}::{1}: {2}", Id, error, message);
            }
        }
    }
}
