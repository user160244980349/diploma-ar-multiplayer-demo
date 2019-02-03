using Events;
using Network.Messages;
using Tools;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        public int BroadcastKey { get; set; }

        private bool _closing;

        private GameObject _socketPrefab;
        private Socket _socket;
        private int _host;
        private Timer _switch;

        public void Close()
        {
            _closing = true;
            _socket.Close();
        }

        private void Start()
        {
            name = "NetworkClient";
            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;
            _switch = gameObject.AddComponent<Timer>();

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<Socket>();
            var started = _socket.ImmediateStart(new SocketSettings
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8001,
                maxConnections = 1,
                packetSize = 1024,
            });
            if (!started)
            {
                Destroy(gameObject);
                return;
            }

            _socket.ReceiveBroadcast(1);
            Debug.Log("CLIENT::Boot on port 8001");
            
            EventManager.Singleton.Subscribe(GameEventType.SendNetworkMessage, Send);
            EventManager.Singleton.Subscribe(GameEventType.ConnectToHost, ConnectToHost);
            EventManager.Singleton.Subscribe(GameEventType.DisconnectFromHost, DisconnectFromHost);
            EventManager.Singleton.Publish(GameEventType.ClientStarted, null);
        }
        private void Update()
        {
            if (_closing && _socket == null)
            {
                Destroy(gameObject);
            }

            if (_switch.Elapsed)
            {
                _switch.Discard();
                _switch.Running = false;
                EventManager.Singleton.Publish(GameEventType.Switch, BroadcastKey);
            }

            while (_socket.PollMessage(out MessageWrapper wrapper))
            {
                switch (wrapper.message.networkMessageType)
                {
                    case NetworkMessageType.Connect:
                    {
                        _host = wrapper.connection;
                        if (BroadcastKey != 0)
                        {
                            EventManager.Singleton.Publish(GameEventType.ConnectedToHostInFallback, null);
                            break;
                        }
                        EventManager.Singleton.Publish(GameEventType.ConnectedToHost, null);
                        Debug.LogFormat("CLIENT::Connected to {0}:{1}", wrapper.ip, wrapper.port);
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
                    case NetworkMessageType.FallbackHostReady:
                    {
                        if (!_switch.Running) break;
                        _socket.Connect(wrapper.ip, wrapper.port);
                        _switch.Discard();
                        _switch.Running = false;
                        Debug.LogFormat("CLIENT::Connecting to fallback {0}:{1} with key {2}", wrapper.ip, wrapper.port, BroadcastKey);
                        break;
                    }
                    case NetworkMessageType.QueueShuffle:
                    {
                        var fallbackInfo = wrapper.message as QueueShuffle;
                        _switch.Duration = fallbackInfo.switchDelay;
                        Debug.LogFormat("CLIENT::Queue shuffled, fallback delay {0}", _switch.Duration);
                        break;
                    }
                    case NetworkMessageType.FoundLobby:
                    {
                        EventManager.Singleton.Publish(GameEventType.FoundLobby, wrapper);
                        break;
                    }
                    case NetworkMessageType.Higher:
                    {
                        EventManager.Singleton.Publish(GameEventType.ReceiveNetworkMessage, wrapper.message);
                        break;
                    }
                    case NetworkMessageType.Disconnect:
                    {
                        if (_socket.DisconnectError == NetworkError.Timeout && BroadcastKey != 0)
                        {
                            _switch.Discard();
                            _switch.Running = true;
                            EventManager.Singleton.Publish(GameEventType.DisconnectedFromHostInFallback, null);
                            break;
                        }
                        EventManager.Singleton.Publish(GameEventType.DisconnectedFromHost, null);
                        Debug.LogFormat("CLIENT::Disconnected from {0}:{1}", wrapper.ip, wrapper.port);
                        break;
                    }
                }
            }
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Unsubscribe(GameEventType.SendNetworkMessage, Send);
            EventManager.Singleton.Unsubscribe(GameEventType.ConnectToHost, ConnectToHost);
            EventManager.Singleton.Unsubscribe(GameEventType.DisconnectFromHost, DisconnectFromHost);
            EventManager.Singleton.Publish(GameEventType.ClientDestroyed, null);
            Debug.Log("CLIENT::Destroyed");
        }
        private void Send(object message)
        {
            _socket.Send(_host, 0, message as ANetworkMessage);
        }
        private void ConnectToHost(object info)
        {
            var message = info as MessageWrapper;
            _socket.Connect(message.ip, message.port);
        }
        private void DisconnectFromHost(object info)
        {
            _socket.Disconnect(_host);
        }
    }
}