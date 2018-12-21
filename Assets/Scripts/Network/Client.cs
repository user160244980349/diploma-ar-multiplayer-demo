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
            socket.Subscribe(this);
            socket.Open();

            ConnectionConfiguration cc = new ConnectionConfiguration {
                socket = socket
            };
            connection = new Connection(cc);
            connection.Open();

        }

        public void Boot () {

        }

        public void OnBroadcastEvent (Connection connection) {

        }

        public void OnConnectEvent (Connection connection) {

            Debug.Log(string.Format("Connected to host"));
        }

        public void OnDataEvent (Connection connection, byte[] data, int dataSize) {

            Debug.Log(string.Format("Recieved data from host {0} connected to socket {1}", connection.Id, socket.Id));
        }

        public void OnDisconnectEvent (Connection connection) {

            Debug.Log(string.Format("Disconnected from host"));
        }

        public void Send () {
            connection.Send(System.Text.Encoding.UTF8.GetBytes("xyu"), 1024);
        }

        public void Shutdown () {
            socket.Close();
        }

    }

}