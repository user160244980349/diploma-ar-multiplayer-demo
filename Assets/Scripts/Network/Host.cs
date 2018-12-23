using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Host {

        int socketId;

        public Host () {

        }

        public void Boot () {

            SocketConfiguration sc = new SocketConfiguration {
                channels = new QosType[2] { QosType.Reliable, QosType.Reliable },
                port = 8000,
                
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent,
            };
            socketId = NetworkManager.Instance.OpenSocket(sc);

        }

        public void OnBroadcastEvent (int connection) {

        }

        public void OnConnectEvent (int connection) {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, socketId));
        }

        public void OnDataEvent (int connection, byte[] data, int dataSize) {
            Debug.Log(string.Format("HOST::Recieved data from client {0} connected to socket {1}", connection, socketId));
            NetworkManager.Instance.Send(socketId, connection, System.Text.Encoding.UTF8.GetBytes("xyu"), 1024);
        }

        public void OnDisconnectEvent (int connection) {
            Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", connection, socketId));
        }

        public void Send () {

        }

        public void Shutdown () {

        }

    }

}