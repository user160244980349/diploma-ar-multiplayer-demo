using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.Networking.Types;

namespace Network
{
    public class Connection : MonoBehaviour
    {
        public bool ReadyForSend { get; set; }
        public int Id { get; private set; }
        public string Ip { get; private set; }
        public int Port { get; private set; }

        private int _socketId;
        private bool _opened;
        private int _queueLength;

        public bool ImmediateStart(int socketId, int id)
        {
            if (_opened) return false;
            _opened = true;

            Id = id;
            _socketId = socketId;

            NetworkTransport.GetConnectionInfo(_socketId, Id, out string ip, out int port, out NetworkID network, out NodeID end, out byte error);
            ParseError("Failed to get connection info", error);

            Ip = ip;
            Port = port;

            NetworkTransport.NotifyWhenConnectionReadyForSend(_socketId, Id, 1, out error);
            ParseError("Failed to request notify", error);

            return true;
        }
        public void Disconnect()
        {
            NetworkTransport.Disconnect(_socketId, Id, out byte error);
            ParseError("Failed to disconnect", error);
        }
        public bool QueueMessage(int channelId, byte[] packet)
        {
            var queued = NetworkTransport.QueueMessageForSending(_socketId, Id, channelId, packet, packet.Length, out byte error);
            ParseError("Failed to queue outgoing message", error);
            if (queued)
            {
                _queueLength++;
                return true;
            }
            return false;
        }

        private void Start()
        {
            name = string.Format("Connection<{0}>", Id);
        }
        private void Update()
        {
            byte error;
            if (ReadyForSend && _queueLength != 0)
            {
                ReadyForSend = NetworkTransport.SendQueuedMessages(_socketId, Id, out error);
                ParseError("Failed to send queued messages", error);
                if (!ReadyForSend)
                {
                    NetworkTransport.NotifyWhenConnectionReadyForSend(_socketId, Id, _queueLength, out error);
                    ParseError("Failed to request notify", error);
                }
                else
                    _queueLength = 0;
            }
        }
        private void OnDestroy()
        {
            Debug.LogFormat("SOCKET<{0}>::CONNECTION<{1}>::Destroyed", _socketId, Id);
        }
        private void ParseError(string message, byte rawError)
        {
            var error = (NetworkError)rawError;
            if (error != NetworkError.Ok)
            {
                Debug.LogErrorFormat("SOCKET<{0}>::CONNECTION<{1}>::{2}: {3}", _socketId, Id, error, message);
            }
        }
    }
}