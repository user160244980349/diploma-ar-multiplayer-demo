using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class NetworkManager : MonoBehaviour {

        public static NetworkManager Instance { get { return instance; } }
        static NetworkManager instance = null;
        
        const int bufferSize = 1024;
        byte[] buffer;

        const int maxSockets = 16;
        Socket[] sockets;

        const int maxConnections = 16;
        const int maxChannels = 16;

        void Awake () {

            if (instance == null) {
                instance = this; 
            } else {
                Destroy(this);
            }

            sockets = new Socket[maxSockets];
            for (int i = 0; i < maxSockets; i++)
            {
                sockets[i].inUse = false;
                sockets[i].eventsAvaliable = false;

                sockets[i].connections = new Connection[maxConnections];
                for (int j = 0; j < maxSockets; j++)
                {
                    sockets[i].connections[j].inUse = false;
                }

                sockets[i].channels = new Channel[maxChannels];
                for (int j = 0; j < maxSockets; j++)
                {
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

            for (int socketIndex = 0; socketIndex < maxSockets;  socketIndex++)
            {
                if (!sockets[socketIndex].eventsAvaliable && sockets[socketIndex].inUse) return;
                sockets[socketIndex].eventsAvaliable = false;

                int connectionId;
                int channelId;
                int dataSize;
                byte error;

                NetworkEventType networkEvent;
                do
                {
                    networkEvent = NetworkTransport.ReceiveFromHost(
                        sockets[socketIndex].id,
                        out connectionId,
                        out channelId,
                        buffer,
                        bufferSize,
                        out dataSize,
                        out error
                    );

                    if ((NetworkError)error != NetworkError.Ok)
                        Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

                    switch (networkEvent)
                    {
                        case NetworkEventType.ConnectEvent:
                            //foreach (ISocketSubscriber subscriber in subscribers)
                            //{
                            //    subscriber.OnConnectEvent(connectionId);
                            //}
                            break;

                        case NetworkEventType.DataEvent:
                            //foreach (ISocketSubscriber subscriber in subscribers)
                            //{
                            //    subscriber.OnDataEvent(connectionId, buffer, dataSize);
                            //}
                            break;

                        case NetworkEventType.BroadcastEvent:
                            //foreach (ISocketSubscriber subscriber in subscribers)
                            //{
                            //    subscriber.OnBroadcastEvent(connectionId);
                            //}
                            break;

                        case NetworkEventType.DisconnectEvent:
                            //foreach (ISocketSubscriber subscriber in subscribers)
                            //{
                            //    subscriber.OnDisconnectEvent(connectionId);
                            //}
                            break;

                    }
                } while (networkEvent != NetworkEventType.Nothing);
            }
            
        }

        void OnDestroy () {
            NetworkTransport.Shutdown();
        }

        public int OpenSocket(SocketConfiguration sc) {

            int freeSocketIndex = 0;
            for (int i = 0; i < maxSockets; i++)
            {
                if (!sockets[i].inUse)
                {
                    freeSocketIndex = i;
                }
            }

            ConnectionConfig connectionConfig = new ConnectionConfig();

            int channelIndex = 0;
            foreach (QosType qos in sc.channels)
            {
                Channel channel = new Channel
                {
                    inUse = true,
                    id = connectionConfig.AddChannel(qos),
                    type = qos,
                };
                sockets[freeSocketIndex].channels[channelIndex] = channel;
                channelIndex++;
            }

            HostTopology hostTopology = new HostTopology(connectionConfig, maxConnections);

            sockets[freeSocketIndex].id = NetworkTransport.AddHost(
                hostTopology,
                sc.port);

            return sockets[freeSocketIndex].id;
        }

        public void CloseSocket(int socketId)
        {
            if (sockets[socketId].inUse && socketId < maxSockets)
            {
                sockets[socketId].inUse = false;
                
                for (int j = 0; j < maxSockets; j++)
                {
                    sockets[socketId].connections[j].inUse = false;
                }
                
                for (int j = 0; j < maxSockets; j++)
                {
                    sockets[socketId].channels[j].inUse = false;
                }

                NetworkTransport.RemoveHost(socketId);
            }
        }

        public void OnConnectionReady(int socketId, int connectionId)
        {
            sockets[socketId].connections[connectionId].ready = true;
        }

        public int OpenConnection(int socketId,  ConnectionConfiguration cc) {

            int freeConnectionIndex = 0;
            for (int i = 0;  sockets[socketId].connections[i].inUse != false; i++) {
                freeConnectionIndex = i;
            }

            byte error;
            Connection connection = new Connection
            {
                id = NetworkTransport.Connect(
                    socketId,
                    cc.ip,
                    cc.port,
                    cc.exceptionConnectionId,
                    out error),
                c = cc,
                inUse = true,
                ready = false                
            };

            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
            
            return 0;
        }

        public void CloseConnection(int socketId, int connectionId)
        {
            if (sockets[socketId].inUse && 
                socketId < maxSockets && 
                sockets[socketId].connections[connectionId].inUse &&
                connectionId < maxConnections)
            {
                sockets[socketId].connections[connectionId].inUse = false;

                byte error;
                NetworkTransport.Disconnect(socketId, connectionId, out error);

                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
            }

        }

        public void Send(int socketId, int connectionId, byte[] buffer, int size)
        {
            byte error;

            if (!sockets[socketId].inUse && socketId < maxSockets)
                return;

            if (!sockets[socketId].connections[connectionId].inUse && socketId < maxConnections)
                return;

            if (sockets[socketId].connections[connectionId].ready)
            {
                if (!NetworkTransport.Send(socketId, connectionId, 0, buffer, size, out error))
                {

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

        public void OnNetworkEvent (int socketId) {
            for (int i = 0; i < maxSockets; i++)
            {
                if (sockets[i].id == socketId)
                {
                    sockets[i].eventsAvaliable = true;
                }
            }
        }

	}

}