using System.Collections.Generic;
using System.Threading;
using Events;
using Network.Messages;
using Network.Socket;
using UnityEngine;
using UnityEngine.Networking;

namespace Network.Host
{
    public class NetworkHost : MonoBehaviour
    {
        public HostState State { get; private set; }
        public int BroadcastKey { get; set; }

        private GameObject _socketPrefab;

        private NetworkSocket _socket;
        private List<int> _connections;

        private Timer _discovery;
        private const float _discoveryDuration = 0.1f;
        private const float _switchDelay = 10f;

        #region MonoBehaviour
        private void Start()
        {
            _discovery = gameObject.AddComponent<Timer>();
            _discovery.Duration = _discoveryDuration;

            if (BroadcastKey == 0)
                BroadcastKey = KeyGenerator.Generate();
            else
            {
                _discovery.Remains = 5;
                _discovery.Running = true;
                State = HostState.FallingBack;
            }

            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;

            _connections = new List<int>();

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<NetworkSocket>();
            _socket.Settings = new SocketSettings
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8000,
                maxConnections = 16,
                packetSize = 1024,
            };
            Debug.Log("HOST::Boot on port 8000");

            gameObject.name = "NetworkHost";
            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageSend, Send);
        }
        private void Update()
        {
            switch (_socket.State)
            {
                case SocketState.ReadyToOpen:
                {
                    _socket.Open();
                    break;
                }

                case SocketState.Opened:
                {
                    _socket.Up();
                    break;
                }

                case SocketState.Down:
                {
                    Destroy(_socket.gameObject);
                    break;
                }
            }

            switch (State)
            {
                case HostState.StartingUp:
                {
                    if (_socket.State != SocketState.Up) break;
                    State = HostState.Up;
                    break;
                }

                case HostState.Up:
                {
                    while (_socket.PollMessage(out MessageWrapper wrapper))
                    {
                        switch (wrapper.message.networkMessageType)
                        {
                            case NetworkMessageType.Higher:
                            {
                                Debug.Log(string.Format("HOST::Received data from {0}", wrapper.connection));
                                EventManager.Singleton.Publish(GameEventType.NetworkMessageReceived, wrapper.message);
                                break;
                            }

                            case NetworkMessageType.Connect:
                            {
                                Debug.Log(string.Format("HOST::Client {0} connected to socket {1}", wrapper.connection, _socket.Id));
                                _connections.Add(wrapper.connection);

                                Send(new FallbackInfo(BroadcastKey, (wrapper.connection - 1) * _switchDelay), wrapper.connection);
                                break;
                            }

                            case NetworkMessageType.Disconnect:
                            {
                                Debug.Log(string.Format("HOST::Client {0} disconnected from socket {1}", wrapper.connection, _socket.Id));
                                _connections.Remove(wrapper.connection);
                                break;
                            }
                        }
                    }
                    break;
                }

                case HostState.FallingBack:
                {
                    for (var i = 0; i < 60; i++)
                    {
                        Thread.Sleep(10);
                        Debug.LogFormat("HOST::Broadcasting to {1} port with key {0}", BroadcastKey, 8001 + i);
                        _socket.StartBroadcast(BroadcastKey, 8001 + i, new FallbackHostReady());
                        Thread.Sleep(100);
                        _socket.StopBroadcast();
                    }
                    // if (!_discovery.Elapsed) return;
                    // _socket.StopBroadcast();
                    Debug.Log("HOST::Finished broadcasting to 8001 port");
                    State = HostState.Up;
                    break;
                }

                case HostState.ShuttingDown:
                {
                    if (_socket != null) return;
                    State = HostState.Down;
                    break;
                }

                case HostState.Down:
                {
                    Debug.Log("HOST::Shutdown");
                    break;
                }
            }
        }
        private void OnDestroy()
        {
            EventManager.Singleton.UnregisterListener(GameEventType.NetworkMessageSend, Send);
        }
        #endregion

        public void Shutdown()
        {
            State = HostState.ShuttingDown;
            _socket.Close();
        }

        private void Send(object message, int connectionId)
        {
            Debug.Log("HOST::Sending data");
            _socket.Send(connectionId, 1, message as ANetworkMessage);
        }
        private void Send(object message)
        {
            Debug.Log("HOST::Sending data");
            for (var i = 0; i < _connections.Count; i++) _socket.Send(_connections[i], 1, message as ANetworkMessage);
        }
    }
}