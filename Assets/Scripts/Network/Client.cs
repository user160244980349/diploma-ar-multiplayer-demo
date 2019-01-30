using Events;
using Network.Messages;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        public ClientState State { get; private set; }
        public int BroadcastKey { get; set; }

        private GameObject _socketPrefab;

        private Socket _socket;
        private int _host;
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
            _socket = socketObject.GetComponent<Socket>();
            _socket.Settings = new SocketSettings
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8001/* + DateTime.Now.Second*/,
                maxConnections = 1,
                packetSize = 1024,
            };
            Debug.LogFormat("CLIENT::Boot on port {0}", 8001 + DateTime.Now.Second);

            gameObject.name = "NetworkClient";
            EventManager.Singleton.RegisterListener(GameEventType.NetworkMessageSend, Send);
        }
        private void Update()
        {
            ManageSocket();
            ManageClient();
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

        private void ManageSocket()
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
        }
        private void ManageClient()
        {
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
                    ParseMessages();
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
                            Debug.LogFormat("CLIENT::Connecting to fallback {0}:{1} with key {2}", wrapper.ip, wrapper.port, BroadcastKey);
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
        private void ParseMessages()
        {
            while (_socket.PollMessage(out MessageWrapper wrapper))
            {
                switch (wrapper.message.networkMessageType)
                {
                    case NetworkMessageType.Connect:
                    {
                        _host = wrapper.connection;
                        if (_socket.DisconnectError == NetworkError.Timeout)
                        {
                            Debug.LogFormat("CLIENT::Connection recovered to {0}:{1}", wrapper.ip, wrapper.port);
                        }
                        else
                        {
                            EventManager.Singleton.Publish(GameEventType.Connected, null);
                            Debug.LogFormat("CLIENT::Connected to {0}:{1}", wrapper.ip, wrapper.port);
                        }
                        break;
                    }
                    case NetworkMessageType.FallbackInfo:
                    {
                        var fallbackInfo = wrapper.message as FallbackInfo;
                        BroadcastKey = fallbackInfo.netKey;
                        _switch.Duration = fallbackInfo.switchDelay;
                        _socket.ReceiveBroadcast(BroadcastKey);
                        Debug.LogFormat("CLIENT::Got broadcast key {0}, fallback delay {1}", BroadcastKey, _switch.Duration);
                        break;
                    }
                    case NetworkMessageType.QueueShuffle:
                    {
                        var fallbackInfo = wrapper.message as QueueShuffle;
                        _switch.Duration = fallbackInfo.switchDelay;
                        Debug.LogFormat("CLIENT::Queue shuffled, fallback delay {0}", _switch.Duration);
                        break;
                    }
                    case NetworkMessageType.Higher:
                    {
                        EventManager.Singleton.Publish(GameEventType.NetworkMessageReceived, wrapper.message);
                        Debug.LogFormat("CLIENT::Received higher message from {0}:{1} with ping {2}", wrapper.ip, wrapper.port, wrapper.ping);
                        break;
                    }
                    case NetworkMessageType.Disconnect:
                    {
                        if (_socket.DisconnectError == NetworkError.Timeout)
                        {
                            State = ClientState.WaitingReconnect;
                            _switch.Discard();
                            _switch.Running = true;
                            Debug.LogFormat("CLIENT::Disconnected from {0}:{1} with timeout", wrapper.ip, wrapper.port);
                        }
                        else
                        {
                            Shutdown();
                            EventManager.Singleton.Publish(GameEventType.Disconnected, null);
                            Debug.LogFormat("CLIENT::Disconnected from {0}:{1}", wrapper.ip, wrapper.port);
                        }
                        break;
                    }
                }
            }
        }
        private void Send(object message)
        {
            Debug.Log("CLIENT::Sending data");
            _socket.Send(_host, 0, message as ANetworkMessage);
        }
    }
}