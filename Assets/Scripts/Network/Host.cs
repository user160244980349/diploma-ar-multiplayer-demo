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
            NetworkManager.GetInstance().RegisterSocket(socket);

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