using Network.Delegates;
using Network.Messages;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Socket : MonoBehaviour
    {
        public int Id { get; private set; }
        public SocketConfiguration Config { get; set; }

        private int _activeConnections;
        private Connection[] _connections;

        private HostTopology _topology;
        private ConnectionConfig _config;

        private OnStart _onStart;
        private OnConnectEvent _onConnectEvent;
        private OnDataEvent _onDataEvent;
        private OnBroadcastEvent _onBroadcastEvent;
        private OnDisconnectEvent _onDisconnectEvent;
        private OnClose _onClose;

        private const int PacketSize = 1024;
        private byte[] _packet;
        private bool _eventsReady;

        private bool _closing;
        private int _closingConnection;

        private byte _error;

        #region MonoBehaviour
        private void Start()
        {
            var connectionConfig = new ConnectionConfig();

            for (var i = 0; i < Config.channels.Length; i++) connectionConfig.AddChannel(Config.channels[i]);

            var hostTopology = new HostTopology(connectionConfig, Config.maxConnections);
            var id = NetworkTransport.AddHost(hostTopology, Config.port);

            _topology = hostTopology;
            _config = connectionConfig;
            _connections = new Connection[Config.maxConnections];

            _packet = new byte[PacketSize];

            _onStart = Config.onStart;
            _onBroadcastEvent = Config.onBroadcastEvent;
            _onConnectEvent = Config.onConnectEvent;
            _onDataEvent = Config.onDataEvent;
            _onDisconnectEvent = Config.onDisconnectEvent;
            _onClose = Config.onClose;

            NetworkManager.Singleton.RegisterSocket(this);
            Debug.LogFormat(" >> Socket opended {0}", id);

            gameObject.name = string.Format("Socket{0}", Id);
            _onStart();
        }
        private void Update()
        {
            if (_closing)
            {
                if (_activeConnections == 0)
                {
                    Destroy(gameObject);
                    _onClose();
                }
                else
                {
                    while (_connections[_closingConnection] == null)
                    {
                        _closingConnection++;
                    }
                    CloseConnection(_closingConnection);
                    return;
                }
            }

            NetworkEventType networkEvent;
            if (!_eventsReady) return;
            _eventsReady = false;
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
                        var message = Formatter.Deserialize(_packet);
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
                        _connections[connectionId] = null;
                        _onDisconnectEvent(connectionId);
                        break;

                    case NetworkEventType.Nothing:
                        break;
                }
            } while (networkEvent != NetworkEventType.Nothing);
        }
        private void OnDestroy()
        {
            Debug.LogFormat(" >> Socket closed {0}", Id);
            NetworkManager.Singleton.UnregisterSocket(this);
            NetworkTransport.RemoveHost(Id);
        }
        #endregion

        public void ConnectionReady(int connectionId)
        {
            if (_connections[connectionId] == null) return;
            _connections[connectionId].ReadyToSend();
        }
        public void EventsReady()
        {
            _eventsReady = true;
        }
        public int OpenConnection(ConnectionConfiguration cc)
        {
            _activeConnections++;
            var newConnection = new Connection(Id, cc);
            _connections[newConnection.id] = newConnection;
            return newConnection.id;
        }
        public void OpenConnection(int connectionId)
        {
            if (_connections[connectionId] == null)
            {
                _activeConnections++;
                var newConnection = new Connection(Id, connectionId);
                _connections[newConnection.id] = newConnection;
            }
            else
            {
                _connections[connectionId].id = connectionId;
            }
        }
        public void CloseConnection(int connectionId)
        {
            _activeConnections--;
            _connections[connectionId].Disconnect();
            _connections[connectionId] = null;
        }
        public void Send(int connectionId, int channelId, ANetworkMessage message)
        {
            if (_connections[connectionId] == null) return;
            _connections[connectionId].QueueMessage(channelId, message);
        }
        public void Close()
        {
            _closing = true;
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}
