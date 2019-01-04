using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        private List<int> _clients;
        private int _socketId;
        public bool Booted { get; private set; }
        public bool ShuttingDown { get; private set; }

        private void Start()
        {
            _clients = new List<int>();
        }

        public void Boot()
        {
            Booted = true;
            var sc = new SocketConfiguration
            {
                channels = new QosType[2] {QosType.Reliable, QosType.Reliable},
                port = 8000,
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent
            };
            _socketId = NetworkManager.Instance.OpenSocket(sc);
        }

        public void OnBroadcastEvent(int connection)
        {
        }

        public void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socketId));
            _clients.Add(connection);
        }

        public void OnDataEvent(int connection, byte[] data, int dataSize)
        {
            Debug.Log(string.Format("HOST::Received data from client {0} connected to socket {1}", connection,
                _socketId));
            foreach (var client in _clients)
            {
                if (client == connection) continue;
                NetworkManager.Instance.Send(_socketId, connection, Encoding.UTF8.GetBytes("xyu"), 1024);
            }
        }

        public void OnDisconnectEvent(int connection)
        {
            if (ShuttingDown && _clients.Count == 0)
            {
                ShuttingDown = false;
                NetworkManager.Instance.CloseSocket(_socketId);
            }

            Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", connection, _socketId));
            _clients.Remove(connection);
        }

        public void Send()
        {
        }

        public void Shutdown()
        {
            Booted = false;
            ShuttingDown = true;
            foreach (var client in _clients) NetworkManager.Instance.CloseConnection(_socketId, client);
        }
    }
}