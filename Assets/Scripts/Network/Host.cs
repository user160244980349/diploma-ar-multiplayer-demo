using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Host : ISocketSubscriber
	{
		Socket socket;

		public Host() {

            SocketConfiguration sc = new SocketConfiguration {
                bufferSize = 1024,
                channels = new QosType[] { QosType.Reliable }
            };

            socket = new Socket(sc);
            socket.Subscribe(this);
            socket.Open();

        }

        public void Boot () {

        }

        public void OnBroadcastEvent (Connection connection) {

        }

        public void OnConnectEvent (Connection connection) {

            Debug.Log(string.Format("Client {0} connected to socket {1}", connection.Id, socket.Id));
        }

        public void OnDataEvent (Connection connection, byte[] data, int dataSize) {

            Debug.Log(string.Format("Recieved {2} from client {0} connected to socket {1}", connection.Id, socket.Id, System.Text.Encoding.UTF8.GetString(data, 0, dataSize)));
        }

        public void OnDisconnectEvent (Connection connection) {

            Debug.Log(string.Format("Client {0} disconnected from socket {1}", connection.Id, socket.Id));
        }

        public void Send () {

        }

        public void Shutdown () {
            socket.Close();
        }

    }

}