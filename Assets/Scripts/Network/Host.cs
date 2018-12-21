using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Host : ISocketSubscriber
	{

		public Host() {
            
        }

        public void Boot () {

        }

        public void OnBroadcastEvent (int connection) {

        }

        public void OnConnectEvent (int connection) {

            //Debug.Log(string.Format("Client {0} connected to socket {1}", connection, socket.Id));
        }

        public void OnDataEvent (int connection, byte[] data, int dataSize) {

            //Debug.Log(string.Format("Recieved data from client {0} connected to socket {1}", connection, socket.Id));
        }

        public void OnDisconnectEvent (int connection) {

            //Debug.Log(string.Format("Client {0} disconnected from socket {1}", connection, socket.Id));
        }

        public void Send () {

        }

        public void Shutdown () {

        }

    }

}