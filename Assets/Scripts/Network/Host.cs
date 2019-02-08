using System.Collections;
using System.Collections.Generic;
using Events;
using Network.Messages;
using Network.Messages.Wrappers;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        public int BroadcastKey { get; set; }

        private bool _closing;
        private GameObject _socketPrefab;
        private Socket _socket;
        private List<int> _clients;
        private const float _discoveryDuration = 5f;
        private const float _switchDelay = 5f;

        public void Close()
        {
            _closing = true;
            _socket.Close();
        }

        private IEnumerator StartDiscovery()
        {
            _socket.StopBroadcast();
            Debug.Log("HOST::Finished broadcasting");
            yield return new WaitForSeconds(_discoveryDuration);
            _socket.StopBroadcast();
            Debug.Log("HOST::Finished broadcasting");
        }
        private void Start()
        {
            name = "NetworkHost";
            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;
            _clients = new List<int>();

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<Socket>();
            var started = _socket.ImmediateStart(new SocketSettings
            {
                channels = new QosType[3] { QosType.Reliable,
                                            QosType.Reliable,
                                            QosType.Unreliable },
                port = 8000,
                maxConnections = 16,
                packetSize = 1024,
            });
            if (!started)
            {
                Destroy(gameObject);
                return;
            }

            Debug.Log("HOST::Boot on port 8000");
            if (BroadcastKey != 0)
            {
                _socket.StartBroadcast(BroadcastKey, 8001, new FallbackHostReady());
                EventManager.Singleton.Publish(GameEventType.HostStartedInFallback, null);
                Debug.Log("HOST::Broadcasting to 8001 port");
            }
            else
            {
                BroadcastKey = KeyGenerator.Generate();
                EventManager.Singleton.Publish(GameEventType.HostStarted, null);
            }

            EventManager.Singleton.Subscribe(GameEventType.SendNetworkMessage, Send);
            EventManager.Singleton.Subscribe(GameEventType.SendNetworkReplyMessage, SendReply);
            EventManager.Singleton.Subscribe(GameEventType.SendNetworkExceptMessage, SendExcept);
            EventManager.Singleton.Subscribe(GameEventType.StartLobbyBroadcast, OnStartLobbyBroadcast);
            EventManager.Singleton.Subscribe(GameEventType.StopLobbyBroadcast, OnStopLobbyBroadcast);
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
                    case NetworkMessageType.Higher:
                    {
                        EventManager.Singleton.Publish(GameEventType.ReceiveNetworkMessage, wrapper);
                        break;
                    }
                    case NetworkMessageType.Connect:
                    {
                        Debug.Log(string.Format("HOST::Client {0}:{1} connected", wrapper.ip, wrapper.port));
                        _clients.Add(wrapper.connection);
                        var send = new SendWrapper
                        {
                            message = new FallbackInfo(BroadcastKey, (_clients.Count - 1) * _switchDelay),
                            channel = 0,
                        };
                        Send(wrapper.connection, send);
                        break;
                    }
                    case NetworkMessageType.Disconnect:
                    {
                        Debug.Log(string.Format("HOST::Client {0}:{1} disconnected", wrapper.ip, wrapper.port));
                        var disconnectedIndex = _clients.FindIndex(match => match == wrapper.connection);
                        _clients.Remove(wrapper.connection);
                        if (_closing) break;
                        for (var i = disconnectedIndex; i < _clients.Count; i++)
                        {
                            var send = new SendWrapper
                            {
                                message = new QueueShuffle(i * _switchDelay),
                                channel = 0,
                            };
                            Send(_clients[i], send);
                        }
                        break;
                    }
                }
            }
        }
        private void OnDestroy()
        {
            EventManager.Singleton.Unsubscribe(GameEventType.SendNetworkMessage, Send);
            EventManager.Singleton.Unsubscribe(GameEventType.SendNetworkReplyMessage, SendReply);
            EventManager.Singleton.Unsubscribe(GameEventType.SendNetworkExceptMessage, SendExcept);
            EventManager.Singleton.Unsubscribe(GameEventType.StartLobbyBroadcast, OnStartLobbyBroadcast);
            EventManager.Singleton.Unsubscribe(GameEventType.StopLobbyBroadcast, OnStopLobbyBroadcast);
            EventManager.Singleton.Publish(GameEventType.HostDestroyed, null);
            Debug.Log("HOST::Destroyed");
        }

        private void Send(int connectionId, object wrapper)
        {
            _socket.Send(connectionId, (SendWrapper)wrapper);
        }
        private void Send(object wrapper)
        {
            for (var i = 0; i < _clients.Count; i++)
                _socket.Send(_clients[i], (SendWrapper)wrapper);
        }
        private void SendReply(object wrapper)
        {
            var reply = (ReplyWrapper)wrapper;
            var send = new SendWrapper
            {
                message = reply.message,
                channel = reply.channel,
            };
            _socket.Send(reply.connection, send);
        }
        private void SendExcept(object wrapper)
        {
            var except = (ExceptWrapper)wrapper;
            var send = new SendWrapper
            {
                message = except.message,
                channel = except.channel,
            };
            for (var i = 0; i < _clients.Count; i++)
            {
                if (_clients[i] == except.connection) continue;
                _socket.Send(_clients[i], send);
            }
        }
        private void OnStartLobbyBroadcast(object info)
        {
            Debug.Log("HOST::Started lobby broadcast");
            _socket.StartBroadcast(1, 8001, new FoundLobby(info as string));
        }
        private void OnStopLobbyBroadcast(object info)
        {
            Debug.Log("HOST::Stoped lobby broadcast");
            _socket.StopBroadcast();
        }
    }
}