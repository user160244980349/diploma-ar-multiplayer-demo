using Network.Messages;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        private static Host _instance;

        private HostState _state;
        private List<int> _clients;
        private int _socketId;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
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
        #endregion

        public static Host GetInstance()
        {
            return _instance;
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
            _socketId = NetworkManager.GetInstance().OpenSocket(sc);
        }
        public void Send(ANetworkMessage message)
        {

        }
        public void Shutdown()
        {
            Debug.Log(string.Format("HOST::Shutdown"));
            _state = HostState.Down;
            NetworkManager.GetInstance().CloseSocket(_socketId);
        }

        #region Network delegates
        public void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socketId));
            _clients.Add(connection);
        }
        public void OnBroadcastEvent(int connection)
        {

        }
        public void OnDataEvent(int connection, ANetworkMessage message)
        {
            Debug.Log(string.Format("HOST::Received data from client {0} connected to socket {1}", connection, _socketId));
            switch (message.networkMessageType)
            {
                case NetworkMessageType.Beep:
                    Debug.Log(" > Boop from network layer");
                    for (var i = 0; i < _clients.Count; i++)
                    {
                        if (_clients[i] == connection) continue;
                        NetworkManager.GetInstance().Send(_socketId, _clients[i], 1, message);
                    }
                    break;

                case NetworkMessageType.Service:

                    break;

                case NetworkMessageType.Higher:
                    for (var i = 0; i < _clients.Count; i++)
                    {
                        if (_clients[i] == connection) continue;
                        NetworkManager.GetInstance().Send(_socketId, _clients[i], 1, message);
                    }
                    break;

                default:
                    break;
            }
        }
        public void OnDisconnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", connection, _socketId));
            _clients.Remove(connection);
        }
        #endregion
    }
}
