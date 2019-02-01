using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Network
{
    public class Connection : MonoBehaviour
    {
        public int Id { get; private set; }
        public int SocketId { get; private set; }
        public int Port { get; private set; }
        public string Ip { get; private set; }
        public ConnectionState State { get; private set; }
        public ConnectionSettings Settings { get; set; }

        private int _queueLength;
        private Timer _send;

        #region MonoBehaviour
        private void Start()
        {
            Id = Settings.id;
            SocketId = Settings.socketId;
            Ip = Settings.ip;
            Port = Settings.port;

            _send = gameObject.AddComponent<Timer>();
            _send.Duration = Settings.sendRate;

            gameObject.name = string.Format("Connection{0}", Id);
        }
        private void Update()
        {
            ManageConnection();
        }
        #endregion

        public bool Connect()
        {
            if (State != ConnectionState.ReadyToConnect) return false;
            State = ConnectionState.Connecting;
            return true;
        }
        public bool Confirm()
        {
            if (State != ConnectionState.WaitingConfirm) return false;
            State = ConnectionState.Connected;
            return true;
        }
        public bool Up()
        {
            if (State != ConnectionState.Connected) return false;
            State = ConnectionState.Up;
            _send.Discard();
            _send.Running = true;
            return true;
        }
        public void Disconnect(bool incoming)
        {
            if (incoming)
            {
                State = ConnectionState.Disconnected;
                return;
            }
            State = ConnectionState.Disconnecting;
        }
        public bool QueueMessage(int channelId, byte[] packet)
        {
            if (State != ConnectionState.Up) return false;
            _queueLength++;
            NetworkTransport.QueueMessageForSending(SocketId, Id, channelId, packet, packet.Length, out byte error);
            ParseError(error);
            return true;
        }

        private void ManageConnection()
        {
            switch (State)
            {
                case ConnectionState.Connecting:
                {
                    if (Id == 0)
                    {
                        State = ConnectionState.WaitingConfirm;

                        Id = NetworkTransport.Connect(SocketId, Ip, Port, 0, out byte error);
                        ParseError(error);

                        gameObject.name = string.Format("Connection{0}", Id);
                        break;
                    }
                    else
                    {
                        State = ConnectionState.Connected;

                        NetworkTransport.GetConnectionInfo(SocketId, Id, out string ip, out int port, out NetworkID network, out NodeID end, out byte error);
                        ParseError(error);

                        Ip = ip;
                        Port = port;
                    }
                    break;
                }
                case ConnectionState.Connected:
                {
                    break;
                }
                case ConnectionState.Up:
                {
                    if (!_send.Elapsed) break;
                    _send.Discard();
                    if (_queueLength < 0) break;
                    _queueLength = 0;
                    NetworkTransport.SendQueuedMessages(SocketId, Id, out byte error);
                    ParseError(error);
                    break;
                }
                case ConnectionState.Disconnecting:
                {
                    State = ConnectionState.Disconnected;
                    NetworkTransport.Disconnect(SocketId, Id, out byte error);
                    ParseError(error);
                    break;
                }
                case ConnectionState.Disconnected:
                {
                    break;
                }
            }
        }
        private void ParseError(byte rawError)
        {
            var error = (NetworkError)rawError;
            if (error != NetworkError.Ok)
            {
                Debug.LogErrorFormat("NetworkError {0}", error);
            }
        }
    }
}