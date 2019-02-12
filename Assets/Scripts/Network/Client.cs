using Events;
using Network.Messages;
using Network.Messages.Wrappers;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Client : MonoBehaviour
    {
        public int BroadcastKey;

        private bool _closing;
        private bool _switching;
        private GameObject _socketPrefab;
        private Socket _socket;
        private int _host;
        private Coroutine _switch;
        private float _switchDuration;

        public void Close()
        {
            _closing = true;
            _socket.Close();
        }

        private IEnumerator Switch()
        {
            _switching = true;
            yield return new WaitForSeconds(_switchDuration);
            EventManager.Singleton.Publish(GameEventType.Switch, BroadcastKey);
        }
        private void Start()
        {
            name = "NetworkClient";
            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<Socket>();
            var started = _socket.ImmediateStart(new SocketSettings
            {
                channels = new QosType[3] { QosType.Reliable,
                                            QosType.Reliable,
                                            QosType.Unreliable },
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

            while (_socket.PollMessage(out ReceiveWrapper wrapper))
            {
                switch (wrapper.message.lowType)
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
                        _switchDuration = fallbackInfo.switchDelay;
                        _socket.ReceiveBroadcast(BroadcastKey);
                        Debug.LogFormat("CLIENT::Got broadcast key {0}, fallback delay {1}", BroadcastKey, _switchDuration);
                        break;
                    }
                    case NetworkMessageType.FallbackHostReady:
                    {
                        if (!_switching) break;
                        _switching = false;
                        _socket.Connect(wrapper.ip, wrapper.port);
                        StopCoroutine(_switch);
                        Debug.LogFormat("CLIENT::Connecting to fallback {0}:{1} with key {2}", wrapper.ip, wrapper.port, BroadcastKey);
                        break;
                    }
                    case NetworkMessageType.QueueShuffle:
                    {
                        var fallbackInfo = wrapper.message as QueueShuffle;
                        _switchDuration = fallbackInfo.switchDelay;
                        Debug.LogFormat("CLIENT::Queue shuffled, fallback delay {0}", _switchDuration);
                        break;
                    }
                    case NetworkMessageType.FoundLobby:
                    {
                        EventManager.Singleton.Publish(GameEventType.FoundLobby, wrapper);
                        break;
                    }
                    case NetworkMessageType.Higher:
                    {
                        EventManager.Singleton.Publish(GameEventType.ReceiveNetworkMessage, wrapper);
                        break;
                    }
                    case NetworkMessageType.Disconnect:
                    {
                        if (_socket.DisconnectError == NetworkError.Timeout && BroadcastKey != 0)
                        {
                            _switch = StartCoroutine(Switch());
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
        private void Send(object wrapper)
        {
            _socket.Send(_host, (SendWrapper)wrapper);
        }
        private void ConnectToHost(object info)
        {
            var message = (ReceiveWrapper)info;
            _socket.Connect(message.ip, message.port);
        }
        private void DisconnectFromHost(object info)
        {
            _socket.Disconnect(_host);
        }
    }
}