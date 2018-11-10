using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Client : ISocketSubscriber {

        Socket socket;
        Connection connection;

        public Client () {

            SocketConfiguration sc = new SocketConfiguration {
                bufferSize = 1024,
                channels = new QosType[] { QosType.Reliable },
                port = 8001
            };

            socket = new Socket(sc);
            NetworkManager.GetInstance().RegisterSocket(socket);

            ConnectionConfiguration cc = new ConnectionConfiguration {
                socket = socket
            };

            connection = new Connection(cc);

        }

        public void Boot () {

        }

        public void OnBroadcastEvent (Connection connection) {

        }

        public void OnConnectEvent (Connection connection) {

        }

        public void OnDataEvent (Connection connection, byte[] data, int dataSize) {

        }

        public void OnDisconnectEvent (Connection connection) {

        }

        public void Send () {

        }

        public void Shutdown () {

        }

    }

}