using Network.Messages;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        private static NetworkManager _instance;

        private const int MaxSockets = 16;
        private const int MaxConnections = 16;
        private const int MaxChannels = 16;
        private const int BufferSize = 1024;

        private byte[] _buffer;
        private byte error;
        private Socket[] _sockets;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
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

        public static NetworkManager GetInstance()
        {
            return _instance;
        }

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
                    out error
                );

                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

                switch (networkEvent)
                {
                    case NetworkEventType.ConnectEvent:
                        ConnectionUsing(socketId, connectionId);
                        _sockets[socketId].onConnectEvent(connectionId);
                        break;

                    case NetworkEventType.DataEvent:
                        _sockets[socketId].onDataEvent(connectionId, Formatter.Deserialize(_buffer));
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

        #region Sockets
        public int OpenSocket(SocketConfiguration sc)
        {
            var connectionConfig = new ConnectionConfig();
            var hostTopology = new HostTopology(connectionConfig, MaxConnections);
            var freshSocketId = NetworkTransport.AddHost(
                hostTopology,
                sc.port);
            for (var i = 0; i < sc.channels.Length; i++)
            {
                connectionConfig.AddChannel(sc.channels[i]);
            }
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
        #endregion

        #region Connections
        public void Send(int socketId, int connectionId, int channelId, ANetworkMessage message)
        {
            if (!_sockets[socketId].inUse) return;
            if (!_sockets[socketId].connections[connectionId].inUse) return;
            var binaryMessage = Formatter.Serialize(message);
            NetworkTransport.Send(socketId, connectionId, channelId, binaryMessage, binaryMessage.Length, out error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
        }
        public void OpenConnection(int socketId, ConnectionConfiguration cc)
        {
            var connectionId = NetworkTransport.Connect(
                socketId,
                cc.ip,
                cc.port,
                cc.exceptionConnectionId,
                out error);
            ConnectionUsing(socketId, connectionId);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
        }
        public void CloseConnection(int socketId, int connectionId)
        {
            if (!_sockets[socketId].inUse) return;
            if (!_sockets[socketId].connections[connectionId].inUse) return;
            NetworkTransport.Disconnect(socketId, connectionId, out error);
            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
        }

        private void ConnectionUsing(int socketId, int connectionId)
        {
            _sockets[socketId].connections[connectionId].inUse = true;
        }
        private void ConnectionNotUsing(int socketId, int connectionId)
        {
            _sockets[socketId].connections[connectionId].inUse = false;
        }
        #endregion
    }
}
