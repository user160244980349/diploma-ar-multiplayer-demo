using System.Collections.Generic;
using Events;
using Events.EventTypes;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        private List<int> _clients;
        private MultiplayerMessageReady _mmr;
        private ReceivedMultiplayerMessage _rmm;
        private int _socketId;

        public static Host Singleton { get; private set; }
        public HostState State { get; private set; }

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
            _mmr.Subscribe(Send);
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
            _mmr.Unsubscribe(Send);
            ApplicationManager.Singleton.LoadScene("MainMenu");
        }

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            State = HostState.Down;
        }
        private void Start()
        {
            _clients = new List<int>();
            _mmr = EventManager.Singleton.GetEvent<MultiplayerMessageReady>();
            _rmm = EventManager.Singleton.GetEvent<ReceivedMultiplayerMessage>();
        }
        #endregion

        #region Network delegates
        public void OnConnectEvent(int connection)
        {
            Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", connection, _socketId));
            _clients.Add(connection);
        }
        public void OnBroadcastEvent(int connection)
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
                    _rmm.Publish(message);
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