using UnityEngine;
using UnityEngine.Networking;

namespace Network {

    public class Client {

        private int _socketId;
        private int _connectionId;

        public Client () {

        }

        public void Boot () {

            var sc = new SocketConfiguration {
                channels = new QosType[2] { QosType.Reliable, QosType.Reliable },
//                port = 8001,

                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent,
            };
            _socketId = Network.NetworkManager.Instance.OpenSocket(sc);

        }

        public void Connect() {

            var cc = new ConnectionConfiguration {
                ip = "127.0.0.1",
                port = 8000,
                notificationLevel = 1,
                exceptionConnectionId = 0,
            };
            Network.NetworkManager.Instance.OpenConnection(_socketId, cc);

        }

        public void Disconnect () {
            Network.NetworkManager.Instance.CloseConnection(_socketId, _connectionId);
        }

        private void OnBroadcastEvent (int connection) {

        }

        private void OnConnectEvent (int connection) {
            _connectionId = connection;
            Debug.Log(string.Format("CLIENT::Connected to host"));
        }

        private void OnDataEvent (int connection, byte[] data, int dataSize) {
            Debug.Log(string.Format("CLIENT::Recieved data from host {0} connected to socket {1}", connection, _socketId));
        }

        private void OnDisconnectEvent (int connection) {
            Debug.Log(string.Format("CLIENT::Disconnected from host"));
        }

        public void Send () {
            Network.NetworkManager.Instance.Send(_socketId, _connectionId, System.Text.Encoding.UTF8.GetBytes("xyu"), 1024);
        }

        public void Shutdown () {
            Network.NetworkManager.Instance.CloseSocket(_socketId);
        }

    }

}