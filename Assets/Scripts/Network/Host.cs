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

        private IEnumerator StopDiscovery()
        {
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
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
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
                StartCoroutine(StopDiscovery());
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

            while (_socket.PollMessage(out MessageWrapper wrapper))
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

                        Send(new FallbackInfo(BroadcastKey, (_clients.Count - 1) * _switchDelay), wrapper.connection);
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
                            Send(new QueueShuffle(i * _switchDelay), _clients[i]);
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

        private void Send(object message, int connectionId)
        {
            // Debug.Log("HOST::Sending data");
            _socket.Send(connectionId, 1, message as ANetworkMessage);
        }
        private void Send(object message)
        {
            // Debug.Log("HOST::Sending data");
            for (var i = 0; i < _clients.Count; i++) _socket.Send(_clients[i], 1, message as ANetworkMessage);
        }
        private void SendReply(object message)
        {
            var reply = message as ReplyWrapper;
            _socket.Send(reply.connection, 1, reply.message as ANetworkMessage);
        }
        private void SendExcept(object message)
        {
            var except = message as ExceptWrapper;
            for (var i = 0; i < _clients.Count; i++)
            {
                if (i == except.connection) continue;
                _socket.Send(_clients[i], 1, except.message as ANetworkMessage);
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