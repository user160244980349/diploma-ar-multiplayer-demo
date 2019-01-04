using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        private int _connectionId;
        private int _socketId;
        public bool Booted { get; private set; }
        public bool Connected { get; private set; }

        private void Start()
        {
        }

        public void Boot()
        {
            Booted = true;
            var sc = new SocketConfiguration
            {
                channels = new QosType[2] {QosType.Reliable, QosType.Reliable},
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent
            };
            _socketId = NetworkManager.Instance.OpenSocket(sc);
        }

        public void Connect(ConnectionConfiguration cc)
        {
            Connected = true;
            NetworkManager.Instance.OpenConnection(_socketId, cc);
        }

        public void Disconnect()
        {
            Connected = false;
            NetworkManager.Instance.CloseConnection(_socketId, _connectionId);
        }

        private void OnBroadcastEvent(int connection)
        {
        }

        private void OnConnectEvent(int connection)
        {
            ApplicationManager.Instance.LoadScene("Playground");
            _connectionId = connection;
            Debug.Log("CLIENT::Connected to host");
        }

        private void OnDataEvent(int connection, byte[] data, int dataSize)
        {
            Debug.Log(string.Format("CLIENT::Received data from host {0} connected to socket {1}", connection,
                _socketId));
        }

        private void OnDisconnectEvent(int connection)
        {
            Connected = false;
            ApplicationManager.Instance.LoadScene("MainMenu");
            Debug.Log("CLIENT::Disconnected from host");
        }

        public void Send()
        {
            NetworkManager.Instance.Send(_socketId, _connectionId, Encoding.UTF8.GetBytes("xyu"), 1024);
        }

        public void Shutdown()
        {
            Booted = false;
            NetworkManager.Instance.CloseSocket(_socketId);
        }
    }
}