using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Client {

        int socketId;
        int connectionId;

        public Client () {

        }

        public void Boot () {

            SocketConfiguration sc = new SocketConfiguration {
                channels = new QosType[2] { QosType.Reliable, QosType.Reliable },
                port = 8001,

                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent,
            };
            socketId = NetworkManager.Instance.OpenSocket(sc);

        }

        public void Connect() {

            ConnectionConfiguration cc = new ConnectionConfiguration {
                ip = "127.0.0.1",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0,
            };
            NetworkManager.Instance.OpenConnection(socketId, cc);

        }

        public void Disconnect () {
            NetworkManager.Instance.CloseConnection(socketId, connectionId);
        }

        public void OnBroadcastEvent (int connection) {

        }

        public void OnConnectEvent (int connection) {
            connectionId = connection;
            Debug.Log(string.Format("CLIENT::Connected to host"));
        }

        public void OnDataEvent (int connection, byte[] data, int dataSize) {
            Debug.Log(string.Format("CLIENT::Recieved data from host {0} connected to socket {1}", connection, socketId));
        }

        public void OnDisconnectEvent (int connection) {
            Debug.Log(string.Format("CLIENT::Disconnected from host"));
        }

        public void Send () {
            NetworkManager.Instance.Send(socketId, connectionId, System.Text.Encoding.UTF8.GetBytes("xyu"), 1024);
        }

        public void Shutdown () {
            NetworkManager.Instance.CloseSocket(socketId);
        }

    }

}