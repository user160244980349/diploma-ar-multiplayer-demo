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
        public bool ReadyForSend { get; set; }
        public string Ip { get; private set; }
        public ConnectionState State { get; private set; }
        public ConnectionSettings Settings { get; set; }

        private int _queueLength;

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
            ReadyForSend = true;
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
            var queued = NetworkTransport.QueueMessageForSending(SocketId, Id, channelId, packet, packet.Length, out byte error);
            ParseError(error);
            if (!queued)
                State = ConnectionState.WrongConnection;
            else
                _queueLength++;
            return queued;
        }

        private void Start()
        {
            Id = Settings.id;
            SocketId = Settings.socketId;
            Ip = Settings.ip;
            Port = Settings.port;

            gameObject.name = string.Format("Connection{0}", Id);
            State = ConnectionState.ReadyToConnect;
        }
        private void Update()
        {
            ManageConnection();
        }

        private void ManageConnection()
        {
            switch (State)
            {
                case ConnectionState.Connecting:
                {
                    byte error;
                    if (Id == 0)
                    {
                        State = ConnectionState.WaitingConfirm;
                        Id = NetworkTransport.Connect(SocketId, Ip, Port, 0, out error);
                        ParseError(error);
                        gameObject.name = string.Format("Connection{0}", Id);
                        break;
                    }
                    State = ConnectionState.Connected;
                    NetworkTransport.GetConnectionInfo(SocketId, Id, out string ip, out int port, out NetworkID network, out NodeID end, out error);
                    ParseError(error);
                    Ip = ip;
                    Port = port;
                    break;
                }
                case ConnectionState.Up:
                {
                    if (!ReadyForSend) break;
                    if (_queueLength == 0) break;
                    ReadyForSend = NetworkTransport.SendQueuedMessages(SocketId, Id, out byte error);
                    ParseError(error);
                    if (ReadyForSend)
                    {
                        _queueLength = 0;
                        break;
                    }
                    var notify = NetworkTransport.NotifyWhenConnectionReadyForSend(SocketId, Id, _queueLength, out error);
                    ParseError(error);
                    if (!notify) {
                        State = ConnectionState.WrongConnection;
                    }
                    break;
                }
                case ConnectionState.Disconnecting:
                {
                    State = ConnectionState.Disconnected;
                    NetworkTransport.Disconnect(SocketId, Id, out byte error);
                    ParseError(error);
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