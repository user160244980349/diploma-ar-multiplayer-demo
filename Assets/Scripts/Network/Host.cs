using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        public static Host Instance { get; private set; }

        private HostState _state;
        private List<int> _clients;
        private int _socketId;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            _state = HostState.Down;
        }

        private void Start()
        {
            _clients = new List<int>();
        }

        private void Update()
        {
            
        }

        public HostState GetState()
        {
            return _state;
        }

        public void Boot()
        {
            Debug.Log(string.Format("HOST::Boot"));
            _state = HostState.Up;
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
            Debug.Log(string.Format("HOST::Received data from client {0} connected to socket {1}", connection, _socketId));
            for (var i = 0; i < _clients.Count; i++)
            {
                if (_clients[i] == connection) continue;
                NetworkManager.Instance.Send(_socketId, _clients[i], data, dataSize);
            }
        }

        public void OnDisconnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", connection, _socketId));
            _clients.Remove(connection);

            if (_state == HostState.Shuttingdown && _clients.Count == 0)
            {
                _state = HostState.Down;
                NetworkManager.Instance.CloseSocket(_socketId);
            }
        }

        public void Send()
        {

        }

        public void Shutdown()
        {
            Debug.Log(string.Format("HOST::Shutdown"));
            _state = HostState.Shuttingdown;
            for (var i = 0; i < _clients.Count; i++)
            {
                NetworkManager.Instance.CloseConnection(_socketId, _clients[i]);
            }
        }
    }
}