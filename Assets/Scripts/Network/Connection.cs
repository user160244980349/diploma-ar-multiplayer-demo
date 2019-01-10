using UnityEngine.Networking;
using UnityEngine;
using Network.Messages;

namespace Network
{
    public class Connection
    {
        public int id;
        public int parentId;
        public string ip;
        public int port;
        public int exceptionConnectionId;
        public int notificationLevel;

        private byte _error;

        public Connection(int socketId, ConnectionConfiguration cc)
        {
            id = NetworkTransport.Connect(id, cc.ip, cc.port, cc.exceptionConnectionId, out _error);
            parentId = socketId;
            ShowErrorIfThrown();
            Debug.LogFormat(" >> Connection opened {0}", id);
        }
        public Connection(int socketId, int connectionId)
        {
            id = connectionId;
            parentId = socketId;
            Debug.LogFormat(" >> Connection accepted {0}", id);
        }
        ~Connection()
        {
            Debug.LogFormat(" >> Connection cloased {0}", id);
            NetworkTransport.Disconnect(parentId, id, out _error);
            ShowErrorIfThrown();
        }
        public void SendQueuedMessages()
        {
            NetworkTransport.SendQueuedMessages(parentId, id, out _error);
        }
        public void QueueMessage(int channelId, ANetworkMessage message)
        {
            message.timeStamp = NetworkTransport.GetNetworkTimestamp();
            var binaryMessage = Formatter.Serialize(message);

            NetworkTransport.QueueMessageForSending(parentId, id, channelId, binaryMessage, binaryMessage.Length, out _error);
            ShowErrorIfThrown();

            var queueSize = NetworkTransport.GetOutgoingMessageQueueSize(parentId, out _error);
            ShowErrorIfThrown();

            NetworkTransport.NotifyWhenConnectionReadyForSend(parentId, id, queueSize, out _error);
            ShowErrorIfThrown();
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}