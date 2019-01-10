using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }

        private const int MaxSockets = 16;
        private const int MaxConnections = 16;
        private const int BufferSize = 1024;
        private byte[] _buffer;
        private byte _error;
        private Socket[] _sockets;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkManager";
            _sockets = new Socket[MaxSockets];

            for (var i = 0; i < MaxSockets; i++)
            {
                _sockets[i].inUse = false;

                _sockets[i].connections = new Connection[MaxConnections];
                for (var j = 0; j < MaxSockets; j++) _sockets[i].connections[j].inUse = false;
            }

            _buffer = new byte[BufferSize];

            var config = new GlobalConfig();
            NetworkTransport.Init(config);
        }
        private void Update()
        {
            Receive();
        }
        private void OnDestroy()
        {
            NetworkTransport.Shutdown();
        }
        #endregion

        private void Receive()
        {
            NetworkEventType networkEvent;

            do
            {
                int socketId;
                int connectionId;
                int channelId;
                int dataSize;

                networkEvent = NetworkTransport.Receive(
                    out socketId,
                    out connectionId,
                    out channelId,
                    _buffer,
                    BufferSize,
                    out dataSize,
                    out _error
                );

                if ((NetworkError)_error != NetworkError.Ok)
                    Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        ConnectionUsing(socketId, connectionId);
                        _sockets[socketId].onConnectEvent(connectionId);
                        break;

                    case NetworkEventType.DataEvent:
                        var message = Formatter.Deserialize(_buffer);

                        message.ping =
                            NetworkTransport.GetRemoteDelayTimeMS(socketId, connectionId, message.timeStamp,
                                out _error);

                        if ((NetworkError)_error != NetworkError.Ok)
                            Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);

                        _sockets[socketId].onDataEvent(connectionId, message);
                        break;

                    case NetworkEventType.BroadcastEvent:
                        _sockets[socketId].onBroadcastEvent(connectionId);
                        break;

                    case NetworkEventType.DisconnectEvent:
                        _sockets[socketId].onDisconnectEvent(connectionId);
                        ConnectionNotUsing(socketId, connectionId);
                        break;

                    case NetworkEventType.Nothing:
                        break;
                }
            } while (networkEvent != NetworkEventType.Nothing);
        }
        public int OpenSocket(SocketConfiguration sc)
        {
            var connectionConfig = new ConnectionConfig();
            connectionConfig.Channels.Clear();
            for (var i = 0; i < sc.channels.Length; i++) connectionConfig.AddChannel(sc.channels[i]);
            var hostTopology = new HostTopology(connectionConfig, MaxConnections);

            var freshSocketId = NetworkTransport.AddHost(
                hostTopology,
                sc.port);

            SocketUsing(freshSocketId, hostTopology, connectionConfig, sc);
            return freshSocketId;
        }
        public void CloseSocket(int socketId)
        {
            if (!_sockets[socketId].inUse) return;

            for (var i = 0; i < MaxConnections; i++)
            {
                if (!_sockets[socketId].connections[i].inUse) continue;
                ConnectionNotUsing(socketId, i);
            }

            SocketNotUsing(socketId);
            NetworkTransport.RemoveHost(socketId);
        }
        private void SocketUsing(int socketId, HostTopology t, ConnectionConfig cc, SocketConfiguration sc)
        {
            for (var i = 0; i < MaxConnections; i++) ConnectionNotUsing(socketId, i);

            _sockets[socketId].inUse = true;
            _sockets[socketId].topology = t;
            _sockets[socketId].config = cc;
            _sockets[socketId].onBroadcastEvent = sc.onBroadcastEvent;
            _sockets[socketId].onConnectEvent = sc.onConnectEvent;
            _sockets[socketId].onDataEvent = sc.onDataEvent;
            _sockets[socketId].onDisconnectEvent = sc.onDisconnectEvent;
        }
        private void SocketNotUsing(int socketId)
        {
            _sockets[socketId].inUse = false;
        }
        public void Send(int socketId, int connectionId, int channelId, ANetworkMessage message)
        {
            if (!_sockets[socketId].inUse) return;
            if (!_sockets[socketId].connections[connectionId].inUse) return;
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var binaryMessage = Formatter.Serialize(message);
            NetworkTransport.Send(socketId, connectionId, channelId, binaryMessage, binaryMessage.Length, out _error);

            if ((NetworkError) _error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError) _error);
        }
        public void OpenConnection(int socketId, ConnectionConfiguration cc)
        {
            var connectionId = NetworkTransport.Connect(
                socketId,
                cc.ip,
                cc.port,
                cc.exceptionConnectionId,
                out _error);

            ConnectionUsing(socketId, connectionId);

            if ((NetworkError) _error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError) _error);
        }
        public void CloseConnection(int socketId, int connectionId)
        {
            if (!_sockets[socketId].inUse) return;
            if (!_sockets[socketId].connections[connectionId].inUse) return;
            NetworkTransport.Disconnect(socketId, connectionId, out _error);

            if ((NetworkError) _error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError) _error);
        }
        private void ConnectionUsing(int socketId, int connectionId)
        {
            _sockets[socketId].connections[connectionId].inUse = true;
        }
        private void ConnectionNotUsing(int socketId, int connectionId)
        {
            _sockets[socketId].connections[connectionId].inUse = false;
        }
    }
}