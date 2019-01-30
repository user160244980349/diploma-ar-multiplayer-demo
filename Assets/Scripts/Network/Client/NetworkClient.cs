using Events;
using Network.Messages;
using Network.Socket;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Network.Client
{
    public class NetworkClient : MonoBehaviour
    {
        public ClientState State { get; private set; }
        public int BroadcastKey { get; set; }

        private GameObject _socketPrefab;

        private NetworkSocket _socket;
        private int _connection;
        private NetworkError _disconnectError;

        private Timer _switch;
        private const float _switchDelay = 10f;

        #region MonoBehaviour
        private void Start()
        {
            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;

            _switch = gameObject.AddComponent<Timer>();
            _switch.Duration = _switchDelay;

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<NetworkSocket>();
            _socket.Settings = new SocketSettings
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8001 + DateTime.Now.Second,
                maxConnections = 1,
                packetSize = 1024,
            };
            Debug.LogFormat("CLIENT::Boot on port {0}", 8001 + DateTime.Now.Second);

            gameObject.name = "NetworkClient";
            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageSend, Send);
        }
        private void Update()
        {
            if (_socket != null)
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

                    case SocketState.Closed:
                    {
                        Destroy(_socket.gameObject);
                        break;
                    }
                }
            }

            switch (State)
            {
                case ClientState.StartingUp:
                {
                    if (_socket.State != SocketState.Up) break;
                    State = ClientState.Up;
                    _socket.OpenConnection("192.168.1.2", 8000);
                    break;
                }

                case ClientState.Up:
                {
                    while (_socket.PollMessage(out MessageWrapper wrapper))
                    {
                        switch (wrapper.message.networkMessageType)
                        {
                            case NetworkMessageType.Connect:
                            {
                                _connection = wrapper.connection;
                                Debug.Log("CLIENT::Connected to host");
                                EventManager.Singleton.Publish(GameEventType.Connected, null);
                                break;
                            }

                            case NetworkMessageType.FallbackInfo:
                            {
                                var fallbackInfo = wrapper.message as FallbackInfo;
                                BroadcastKey = fallbackInfo.netKey;
                                _switch.Duration = fallbackInfo.switchDelay;
                                _socket.ReceiveBroadcast(BroadcastKey);
                                Debug.LogFormat("CLIENT::Got broadcast key {0}", BroadcastKey);
                                break;
                            }

                            case NetworkMessageType.Higher:
                            {
                                EventManager.Singleton.Publish(GameEventType.NetworkMessageReceived, wrapper.message);
                                break;
                            }

                            case NetworkMessageType.Disconnect:
                            {
                                _switch.Discard();
                                _switch.Running = true;
                                State = ClientState.WaitingReconnect;
                                Debug.Log("CLIENT::Disconnected from host");
                                EventManager.Singleton.Publish(GameEventType.Disconnected, null);
                                break;
                            }
                        }
                    }
                    break;
                }

                case ClientState.WaitingReconnect:
                {
                    while (_socket.PollMessage(out MessageWrapper wrapper))
                    {
                        if (wrapper.message.networkMessageType == NetworkMessageType.FallbackHostReady)
                        {
                            State = ClientState.Up;
                            _socket.OpenConnection(wrapper.ip, wrapper.port);
                            Debug.LogFormat("CLIENT::Connecting to fallback host", BroadcastKey);
                            break;
                        }
                    }
                    if (!_switch.Elapsed) return;
                    State = ClientState.FallingBack;
                    Debug.Log("CLIENT::Falling back");
                    _socket.Close();
                    break;
                }

                case ClientState.FallingBack:
                {
                    if (_socket != null) return;
                    Debug.Log("CLIENT::Waiting switch");
                    State = ClientState.WaitingSwitch;
                    break;
                }

                case ClientState.WaitingSwitch:
                {
                    break;
                }

                case ClientState.ShuttingDown:
                {
                    if (_socket != null) return;
                    State = ClientState.Down;
                    break;
                }

                case ClientState.Down:
                {
                    Debug.Log("CLIENT::Shutdown");
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
            State = ClientState.ShuttingDown;
            _socket.Close();
        }

        private void Send(object message)
        {
            Debug.Log("CLIENT::Sending data");
            _socket.Send(_connection, 0, message as ANetworkMessage);
        }
    }
}