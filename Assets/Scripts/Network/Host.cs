using System.Collections.Generic;
using Events;
using Network.Messages;
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
        private Timer _discovery;
        private const float _discoveryDuration = 5f;
        private const float _switchDelay = 5f;

        public void Close()
        {
            _closing = true;
            _socket.Close();
        }

        private void Start()
        {
            name = "NetworkHost";
            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;
            _clients = new List<int>();
            _discovery = gameObject.AddComponent<Timer>();
            _discovery.Duration = _discoveryDuration;

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<Socket>();
            _socket.ImmediateStart(new SocketSettings
            {
                channels = new QosType[2] { QosType.Reliable, QosType.Unreliable },
                port = 8000,
                maxConnections = 16,
                packetSize = 1024,
            });

            Debug.Log("HOST::Boot on port 8000");
            if (BroadcastKey != 0)
            {
                _discovery.Discard();
                _discovery.Running = true;
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
            EventManager.Singleton.Subscribe(GameEventType.StartLobbyBroadcast, OnStartLobbyBroadcast);
            EventManager.Singleton.Subscribe(GameEventType.StopLobbyBroadcast, OnStopLobbyBroadcast);
        }
        private void Update()
        {
            if (_closing && _socket == null)
            {
                Destroy(gameObject);
            }

            if (_discovery.Elapsed)
            {
                _discovery.Discard();
                _discovery.Running = false;
                _socket.StopBroadcast();
                Debug.Log("HOST::Finished broadcasting");
            }

            while (_socket.PollMessage(out MessageWrapper wrapper))
            {
                switch (wrapper.message.networkMessageType)
                {
                    case NetworkMessageType.Higher:
                    {
                        EventManager.Singleton.Publish(GameEventType.ReceiveNetworkMessage, wrapper.message);
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