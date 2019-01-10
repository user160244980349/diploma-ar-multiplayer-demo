using System.Collections.Generic;
using Events;
using Events.EventTypes;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkHost : MonoBehaviour
    {
        public static NetworkHost Singleton { get; private set; }
        public HostState State { get; private set; }

        private SendNetworkMessage _snm;
        private ReceiveNetworkMessage _rnm;
        private int _socketId;
        private List<int> _clients;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkHost";
            State = HostState.Down;
        }
        private void Start()
        {
            _clients = new List<int>();
            _snm = EventManager.Singleton.GetEvent<SendNetworkMessage>();
            _rnm = EventManager.Singleton.GetEvent<ReceiveNetworkMessage>();
        }
        #endregion

        public void Boot()
        {
            Debug.Log("HOST::Boot");
            State = HostState.Up;

            var sc = new SocketConfiguration
            {
                channels = new QosType[2] {QosType.Reliable, QosType.Unreliable},
                port = 8000,
                onConnectEvent = OnConnectEvent,
                onDataEvent = OnDataEvent,
                onBroadcastEvent = OnBroadcastEvent,
                onDisconnectEvent = OnDisconnectEvent
            };

            _socketId = NetworkManager.Singleton.OpenSocket(sc);
            _snm.Subscribe(Send);
            ApplicationManager.Singleton.LoadScene("Playground");
        }
        public void Send(ANetworkMessage message)
        {
//            Debug.Log("CLIENT::Sending data");
            for (var i = 0; i < _clients.Count; i++) NetworkManager.Singleton.Send(_socketId, _clients[i], 1, message);
        }
        public void Shutdown()
        {
            Debug.Log("HOST::Shutdown");
            State = HostState.Down;
            NetworkManager.Singleton.CloseSocket(_socketId);
            _snm.Unsubscribe(Send);
            ApplicationManager.Singleton.LoadScene("MainMenu");
        }
        private void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socketId));
            _clients.Add(connection);
        }
        private void OnBroadcastEvent(int connection)
        {
        }
        private void OnDataEvent(int connection, ANetworkMessage message)
        {
            //            Debug.Log(string.Format("HOST::Received data from client {0} connected to socket {1}", connection,
            //                _socketId));

            switch (message.networkMessageType)
            {
                case NetworkMessageType.Beep:
                    Debug.Log(" > Boop from network layer");
                    break;

                case NetworkMessageType.Service:

                    break;

                case NetworkMessageType.Higher:
                    _rnm.Publish(message);
                    break;
            }
        }
        private void OnDisconnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", connection, _socketId));
            _clients.Remove(connection);
        }
    }
}