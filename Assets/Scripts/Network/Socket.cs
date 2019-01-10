using Network.Delegates;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Socket
    {
        public int id;
        public bool eventsReady;
        public HostTopology topology;
        public ConnectionConfig config;
        public int maxConnections;
        public Connection[] connections;
        public OnConnectEvent onConnectEvent;
        public OnDataEvent onDataEvent;
        public OnBroadcastEvent onBroadcastEvent;
        public OnDisconnectEvent onDisconnectEvent;

        private byte _error;

        public Socket(SocketConfiguration sc) {
            var connectionConfig = new ConnectionConfig();
            connectionConfig.Channels.Clear();

            for (var i = 0; i < sc.channels.Length; i++) connectionConfig.AddChannel(sc.channels[i]);

            var hostTopology = new HostTopology(connectionConfig, sc.maxConnections);
            var id = NetworkTransport.AddHost(hostTopology, sc.port);

            topology = hostTopology;
            config = connectionConfig;
            maxConnections = sc.maxConnections;
            connections = new Connection[maxConnections];

            onBroadcastEvent = sc.onBroadcastEvent;
            onConnectEvent = sc.onConnectEvent;
            onDataEvent = sc.onDataEvent;
            onDisconnectEvent = sc.onDisconnectEvent;

            Debug.LogFormat(" >> Socket opended {0}", id);
        }
        ~Socket()
        {
            for (var i = 0; i < maxConnections; i++)
            {
                if (connections[i] != null)
                    CloseConnection(i);
            }
            NetworkTransport.RemoveHost(id);
        }
        public void ConnectionReady(int connectionId)
        {
            connections[connectionId].SendQueuedMessages();
        }
        public void OpenConnection(ConnectionConfiguration cc)
        {
            var newConnection = new Connection(id, cc);
            connections[newConnection.id] = newConnection;
        }
        public void OpenConnection(int connectionId)
        {
            if (connections[connectionId] == null)
            {
                var newConnection = new Connection(id, connectionId);
                connections[newConnection.id] = newConnection;
            }
            else
            {
                connections[connectionId].id = connectionId;
            }
        }
        public void HandleMessage(NetworkEventType networkEvent, int connectionId, byte[] packet)
        {
            switch (networkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    OpenConnection(connectionId);
                    onConnectEvent(connectionId);
                    break;

                case NetworkEventType.DataEvent:
                    var message = Formatter.Deserialize(packet);
                    message.ping = NetworkTransport.GetRemoteDelayTimeMS(id, connectionId, message.timeStamp, out _error);
                    ShowErrorIfThrown();
                    onDataEvent(connectionId, message);
                    break;

                case NetworkEventType.BroadcastEvent:
                    onBroadcastEvent(connectionId);
                    break;

                case NetworkEventType.DisconnectEvent:
                    CloseConnection(connectionId);
                    onDisconnectEvent(connectionId);
                    break;

                case NetworkEventType.Nothing:
                    break;
            }
        }
        public void CloseConnection(int connectionId)
        {
            connections[connectionId] = null;
        }
        public void Send(int connectionId, int channelId, ANetworkMessage message)
        {
            connections[connectionId].QueueMessage(channelId, message);
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}