using System.Collections.Generic;
using System.Threading;
using Events;
using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class Host : MonoBehaviour
    {
        public HostState State { get; private set; }
        public int BroadcastKey { get; set; }

        private GameObject _socketPrefab;

        private Socket _socket;
        private List<int> _clients;

        private Timer _discovery;
        private const float _discoveryDuration = 0.1f;
        private const float _switchDelay = 20f;

        #region MonoBehaviour
        private void Start()
        {
            _discovery = gameObject.AddComponent<Timer>();
            _discovery.Duration = _discoveryDuration;

            _socketPrefab = Resources.Load("Networking/Socket") as GameObject;

            _clients = new List<int>();

            var socketObject = Instantiate(_socketPrefab, gameObject.transform);
            _socket = socketObject.GetComponent<Socket>();
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
            ManageSocket();
            ManageHost();
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

        private void ManageSocket()
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
        }
        private void ManageHost()
        {
            switch (State)
            {
                case HostState.StartingUp:
                {
                    if (_socket.State != SocketState.Up) break;

                    if (BroadcastKey == 0)
                    {
                        State = HostState.Up;
                        BroadcastKey = KeyGenerator.Generate();
                    }
                    else
                    {
                        State = HostState.FallingBack;
                        Debug.LogFormat("HOST::Broadcasting to port {1} with key {0}", BroadcastKey, 8001);
                        _socket.StartBroadcast(BroadcastKey, 8001, new FallbackHostReady());
                        _discovery.Remains = 5;
                        _discovery.Running = true;
                    }
                    break;
                }
                case HostState.FallingBack:
                {
                    //for (var i = 0; i < 60; i++)
                    //{
                    //    Thread.Sleep(5);
                    //    Debug.LogFormat("HOST::Broadcasting to port {1} with key {0}", BroadcastKey, 8001 + i);
                    //    _socket.StartBroadcast(BroadcastKey, 8001 + i, new FallbackHostReady());
                    //    Thread.Sleep(200);
                    //    _socket.StopBroadcast();
                    //}
                    //Debug.Log("HOST::Finished broadcasting");
                    //State = HostState.Up;
                    if (!_discovery.Elapsed) break;
                    State = HostState.Up;
                    _socket.StopBroadcast();
                    Debug.Log("HOST::Finished broadcasting");
                    break;
                }
                case HostState.Up:
                {
                    ParseMessanges();
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
        private void ParseMessanges()
        {
            while (_socket.PollMessage(out MessageWrapper wrapper))
            {
                switch (wrapper.message.networkMessageType)
                {
                    case NetworkMessageType.Higher:
                    {
                        Debug.Log(string.Format("HOST::Received higher message from {0}:{1}", wrapper.ip, wrapper.port));
                        EventManager.Singleton.Publish(GameEventType.NetworkMessageReceived, wrapper.message);
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
                        if (_socket.DisconnectError == NetworkError.Timeout)
                        {
                            Debug.Log(string.Format("HOST::Client {0}:{1} disconnected with timeout", wrapper.ip, wrapper.port));
                        }
                        else
                        {
                            Debug.Log(string.Format("HOST::Client {0}:{1} disconnected", wrapper.ip, wrapper.port));
                        }
                        var disconnectedIndex = _clients.FindIndex(match => match == wrapper.connection);
                        _clients.Remove(wrapper.connection);
                        for (var i = disconnectedIndex; i < _clients.Count; i++)
                        {
                            Send(new QueueShuffle(i * _switchDelay), _clients[i]);
                        }
                        break;
                    }
                }
            }
        }
        private void Send(object message, int connectionId)
        {
            Debug.Log("HOST::Sending data");
            _socket.Send(connectionId, 1, message as ANetworkMessage);
        }
        private void Send(object message)
        {
            Debug.Log("HOST::Sending data");
            for (var i = 0; i < _clients.Count; i++) _socket.Send(_clients[i], 1, message as ANetworkMessage);
        }
    }
}