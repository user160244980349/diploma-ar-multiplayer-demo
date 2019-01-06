using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        private const int MaxSockets = 16;
        private const int MaxConnections = 16;
        private const int MaxChannels = 16;
        private const int BufferSize = 1024;
        private byte[] _buffer;
        private Socket[] _sockets;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            _sockets = new Socket[MaxSockets];
            for (var i = 0; i < MaxSockets; i++)
            {
                _sockets[i].inUse = false;

                _sockets[i].connections = new Connection[MaxConnections];
                for (var j = 0; j < MaxSockets; j++) _sockets[i].connections[j].inUse = false;

                _sockets[i].channels = new Channel[MaxChannels];
                for (var j = 0; j < MaxSockets; j++) _sockets[i].channels[j].inUse = false;
            }

            _buffer = new byte[BufferSize];

            var config = new GlobalConfig();
            NetworkTransport.Init(config);
        }

        private void Update()
        {
            NetworkEventType networkEvent;
            do
            {
                int socketId;
                int connectionId;
                int channelId;
                int dataSize;
                byte error;

                networkEvent = NetworkTransport.Receive(
                    out socketId,
                    out connectionId,
                    out channelId,
                    _buffer,
                    BufferSize,
                    out dataSize,
                    out error
                );

                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        _sockets[socketId].c.onConnectEvent(connectionId);
                        break;

                    case NetworkEventType.DataEvent:
                        _sockets[socketId].c.onDataEvent(connectionId, _buffer, dataSize);
                        break;

                    case NetworkEventType.BroadcastEvent:
                        _sockets[socketId].c.onBroadcastEvent(connectionId);
                        break;

                    case NetworkEventType.DisconnectEvent:
                        _sockets[socketId].c.onDisconnectEvent(connectionId);
                        break;

                    case NetworkEventType.Nothing:
                        break;
                }
            } while (networkEvent != NetworkEventType.Nothing);
        }

        private void OnDestroy()
        {
            NetworkTransport.Shutdown();
        }

        // ======================================================================================== sockets part

        public int OpenSocket(SocketConfiguration sc)
        {
            var connectionConfig = new ConnectionConfig();
            var channelIndex = 0;
            var channels = new Channel[sc.channels.Length];
            foreach (var qos in sc.channels)
            {
                var channel = new Channel
                {
                    id = connectionConfig.AddChannel(qos),
                    type = qos
                };
                channels[channelIndex] = channel;
                channelIndex++;
            }

            var hostTopology = new HostTopology(connectionConfig, MaxConnections);
            var freshSocketId = NetworkTransport.AddHost(
                hostTopology,
                sc.port);
            _sockets[freshSocketId].inUse = true;
            _sockets[freshSocketId].c = sc;
            _sockets[freshSocketId].channels = channels;
            _sockets[freshSocketId].eventsAvaliable = false;
            return freshSocketId;
        }

        public void CloseSocket(int socketId)
        {
            NetworkTransport.RemoveHost(socketId);
            SocketNotUsing(socketId);
        }

        private void SocketNotUsing(int socketId)
        {
            _sockets[socketId].inUse = false;
            for (var i = 0; i < _sockets[socketId].activeConnections; i++) ConnectionNotUsing(socketId, i);
            for (var i = 0; i < _sockets[socketId].activeChannels; i++) ChannelNotUsing(socketId, i);
        }

        // ======================================================================================== connections part

        public void OpenConnection(int socketId, ConnectionConfiguration cc)
        {
            byte error;
            var connectionId = NetworkTransport.Connect(
                socketId,
                cc.ip,
                cc.port,
                cc.exceptionConnectionId,
                out error);
            _sockets[socketId].connections[connectionId].c = cc;
            _sockets[socketId].connections[connectionId].ready = false;
            if ((NetworkError) error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError) error));
        }

        public void Send(int socketId, int connectionId, byte[] buffer, int size)
        {
            byte error;
            NetworkTransport.Send(socketId, connectionId, 0, buffer, size, out error);
            if ((NetworkError) error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError) error));
        }

        public void CloseConnection(int socketId, int connectionId)
        {
            byte error;
            NetworkTransport.Disconnect(socketId, connectionId, out error);
            if ((NetworkError) error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError) error));
            ConnectionNotUsing(socketId, connectionId);
        }

        private void ConnectionNotUsing(int socketId, int connectionId)
        {
            _sockets[socketId].connections[connectionId].inUse = false;
        }

        // ======================================================================================== channels part

        private void ChannelNotUsing(int socketId, int channelId)
        {
            _sockets[socketId].channels[channelId].inUse = false;
        }
    }
}