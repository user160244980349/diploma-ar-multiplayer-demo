using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class NetworkManager : MonoBehaviour {

        public static NetworkManager Instance { get { return instance; } }
        static NetworkManager instance = null;

        int activeSockets;
        const int maxSockets = 16;
        const int maxConnections = 16;
        const int maxChannels = 16;
        const int bufferSize = 1024;

        byte[] buffer;
        Socket[] sockets;

        void Awake () {

            if (instance == null) {
                instance = this;
            } else {
                Destroy(this);
            }
            
            sockets = new Socket[maxSockets];
            for (int i = 0; i < maxSockets; i++) {
                sockets[i].inUse = false;

                sockets[i].connections = new Connection[maxConnections];
                for (int j = 0; j < maxSockets; j++) {
                    sockets[i].connections[j].inUse = false;
                }

                sockets[i].channels = new Channel[maxChannels];
                for (int j = 0; j < maxSockets; j++) {
                    sockets[i].channels[j].inUse = false;
                }
            }

            buffer = new byte[bufferSize];

            GlobalConfig config = new GlobalConfig {
                ConnectionReadyForSend = OnConnectionReady,
                NetworkEventAvailable = OnNetworkEvent
            };
            NetworkTransport.Init(config);
        }

        void Update () {

            int socketId;
            int connectionId;
            int channelId;
            int dataSize;
            byte error;
            NetworkEventType networkEvent;

            do {
                networkEvent = NetworkTransport.Receive(
                    out socketId,
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
                        sockets[socketId].c.onConnectEvent(connectionId);
                        break;

                    case NetworkEventType.DataEvent:
                        sockets[socketId].c.onDataEvent(connectionId, buffer, dataSize);
                        break;

                    case NetworkEventType.BroadcastEvent:
                        sockets[socketId].c.onBroadcastEvent(connectionId);
                        break;

                    case NetworkEventType.DisconnectEvent:
                        sockets[socketId].c.onDisconnectEvent(connectionId);
                        break;

                }
            } while (networkEvent != NetworkEventType.Nothing);

        }

        void OnDestroy () {
            NetworkTransport.Shutdown();
        }

        // ======================================================================================== sockets part

        public int OpenSocket (SocketConfiguration sc) {

            ConnectionConfig connectionConfig = new ConnectionConfig();

            int channelIndex = 0;
            Channel[] channels = new Channel[sc.channels.Length];
            foreach (QosType qos in sc.channels) {
                Channel channel = new Channel {
                    id = connectionConfig.AddChannel(qos),
                    type = qos,
                };
                channels[channelIndex] = channel;
                channelIndex++;
            }

            HostTopology hostTopology = new HostTopology(connectionConfig, maxConnections);

            int freshSocketId = NetworkTransport.AddHost(
                hostTopology,
                sc.port);

            sockets[freshSocketId].inUse = true;
            sockets[freshSocketId].c = sc;
            sockets[freshSocketId].channels = channels;
            sockets[freshSocketId].eventsAvaliable = false;

            return freshSocketId;
        }

        public void OnNetworkEvent (int socketId) {
            sockets[socketId].eventsAvaliable = true;
        }

        public void CloseSocket (int socketId) {
            SocketNotUsing(socketId);
            NetworkTransport.RemoveHost(socketId);
        }

        public void SocketNotUsing (int socketId) {
            sockets[socketId].inUse = false;
            for (int i = 0; i < sockets[socketId].activeConnections; i++) {
                ConnectionNotUsing(socketId, i);
            }
            for (int i = 0; i < sockets[socketId].activeChannels; i++) {
                ChannelNotUsing(socketId, i);
            }
        }

        // ======================================================================================== connections part

        public void OnConnectionReady (int socketId, int connectionId) {
            sockets[socketId].connections[connectionId].ready = true;
        }

        public void OpenConnection (int socketId, ConnectionConfiguration cc) {
            byte error;

            int connectionId = NetworkTransport.Connect(
                socketId,
                cc.ip,
                cc.port,
                cc.exceptionConnectionId,
                out error);

            sockets[socketId].connections[connectionId].c = cc;
            sockets[socketId].connections[connectionId].ready = false;

            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
        }

        public void Send (int socketId, int connectionId, byte[] buffer, int size) {
            byte error;
            
            if (sockets[socketId].connections[connectionId].ready || true) {
                if (!NetworkTransport.Send(socketId, connectionId, 0, buffer, size, out error)) {

                    if ((NetworkError)error != NetworkError.Ok)
                        Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

                    sockets[socketId].connections[connectionId].ready = false;
                    NetworkTransport.NotifyWhenConnectionReadyForSend(
                        socketId,
                        connectionId,
                        sockets[socketId].connections[connectionId].c.notificationLevel,
                        out error);

                    if ((NetworkError)error != NetworkError.Ok)
                        Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
                }

                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
            }
        }

        public void CloseConnection (int socketId, int connectionId) {

            ConnectionNotUsing(socketId, connectionId);

            byte error;
            NetworkTransport.Disconnect(socketId, connectionId, out error);

            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
        }

        public void ConnectionNotUsing (int socketId, int connectionId) {
            sockets[socketId].connections[connectionId].inUse = false;
        }

        // ======================================================================================== channels part

        public void ChannelNotUsing (int socketId, int channelId) {
            sockets[socketId].channels[channelId].inUse = false;
        }

    }

}