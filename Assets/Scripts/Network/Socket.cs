using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Socket {

        public int Id { get { return id; } }
        public Dictionary<int, Channel> Channels { get { return channels; } }

        bool eventsAvaliable = false;

        int id;
        int port;
        int maxConnections;
        int bufferSize;
        byte[] buffer;
        HostTopology hostTopology;

        List<ISocketSubscriber> subscribers;
        Dictionary<int, Channel> channels;
        Dictionary<int, Connection> connections;

        public Socket (SocketConfiguration sc) {

            port            = sc.port;
            maxConnections  = sc.maxConnections;
            bufferSize      = sc.bufferSize;

            buffer      = new byte[bufferSize];
            subscribers = new List<ISocketSubscriber>();
            channels    = new Dictionary<int, Channel>();
            connections = new Dictionary<int, Connection>();

            ConnectionConfig connectionConfig = new ConnectionConfig();
            foreach (QosType qos in sc.channels) {
                Channel channel = new Channel {
                    id = connectionConfig.AddChannel(qos)
                };
                connectionConfig.AddChannel(qos);
                channels.Add(channel.id, channel);
            }

            hostTopology = new HostTopology(connectionConfig, maxConnections);
        }

        public void Subscribe (ISocketSubscriber subscriber) {
            subscribers.Add(subscriber);
        }

        public void Unsubscribe (ISocketSubscriber subscriber) {
            subscribers.Remove(subscriber);
        }

        public void Open() {
            id = NetworkTransport.AddHost(hostTopology, port);
            NetworkManager.Instance.RegisterSocket(this);
            // Debug.Log(string.Format("Opened socket: {0}", id));
        }

        public void Close () {

            foreach (Connection connection in connections.Values) {
                connection.Close();
            }

            NetworkManager.Instance.UnregisterSocket(this);
            NetworkTransport.RemoveHost(id);
            // Debug.Log(string.Format("Closed socket: {0}", id));
        }

        public void RegisterIncomingConnection (out Connection connection, int connectionId) {
            ConnectionConfiguration cc = new ConnectionConfiguration {
                id = connectionId,
                socket = this,
            };
            connection = new Connection(cc);
            connections.Add(connection.Id, connection);
        }

        public void RegisterConnection (Connection connection) {
            connections.Add(connection.Id, connection);
        }

        public void UnregisterConnection (Connection connection) {
            connections.Remove(connection.Id);
        }

        public void OnConnectionReady (int connectionId) {
            Connection connection;
            connections.TryGetValue(connectionId, out connection);
            connection.OnReady();
        }

        public void OnEventsAvaliable () {
            eventsAvaliable = true;
        }

        public void OnNetworkEvents () {

            if (!eventsAvaliable) return;

            eventsAvaliable = false;

            int connectionId;
            int channelId;
            int dataSize;
            byte error;

            NetworkEventType networkEvent;
            Connection connection;

            do {

                networkEvent = NetworkTransport.ReceiveFromHost(
                    id,
                    out connectionId,
                    out channelId,
                    buffer,
                    bufferSize,
                    out dataSize,
                    out error
                );

                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

                switch (networkEvent) {

                    case NetworkEventType.ConnectEvent:
                        if (!connections.ContainsKey(connectionId)) {
                            RegisterIncomingConnection(out connection, connectionId);
                            foreach (ISocketSubscriber subscriber in subscribers) {
                                subscriber.OnConnectEvent(connection);
                            }
                        }
                        break;

                    case NetworkEventType.DataEvent:
                        if (connections.ContainsKey(connectionId)) {
                            connections.TryGetValue(connectionId, out connection);
                            foreach (ISocketSubscriber subscriber in subscribers) {
                                subscriber.OnDataEvent(connection, buffer, dataSize);
                            }
                        }
                        break;

                    case NetworkEventType.BroadcastEvent:
                        if (connections.ContainsKey(connectionId)) {
                            connections.TryGetValue(connectionId, out connection);
                            foreach (ISocketSubscriber subscriber in subscribers) {
                                subscriber.OnBroadcastEvent(connection);
                            }
                        }
                        break;

                    case NetworkEventType.DisconnectEvent:
                        if (connections.ContainsKey(connectionId)) {
                            connections.TryGetValue(connectionId, out connection);
                            UnregisterConnection(connection);
                            connections.Remove(connection.Id);
                            foreach (ISocketSubscriber subscriber in subscribers) {
                                subscriber.OnDisconnectEvent(connection);
                            }
                        }
                        break;

                }

            } while (networkEvent != NetworkEventType.Nothing);

        }

    }

}
