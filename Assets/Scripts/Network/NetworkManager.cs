using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }

        private const int MaxSockets = 16;
        private const int MaxConnections = 16;
        private const int NetworkMaxPacketSize = 1024;

        private Socket[] _sockets;
        private byte[] _packet;
        private byte _error;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkManager";
            _sockets = new Socket[MaxSockets];

            _packet = new byte[NetworkMaxPacketSize];

            var config = new GlobalConfig {
                NetworkEventAvailable = NetworkEventAvailable,
                ConnectionReadyForSend = ConnectionReadyForSend,
            };
            NetworkTransport.Init(config);
        }
        private void Update()
        {
            NetworkEventType networkEvent;
            for (int socketId = 0; socketId < MaxSockets; socketId++)
            {
                if (_sockets[socketId] == null) continue;
                if (!_sockets[socketId].eventsReady) continue;
                _sockets[socketId].eventsReady = false;
                do
                {
                    networkEvent = NetworkTransport.ReceiveFromHost(
                        socketId,
                        out int connectionId,
                        out int channelId,
                        _packet,
                        NetworkMaxPacketSize,
                        out int dataSize,
                        out _error
                    );
                    ShowErrorIfThrown();

                    _sockets[socketId].HandleMessage(networkEvent, connectionId, _packet);
                } while (networkEvent != NetworkEventType.Nothing);
            }
        }
        private void OnDestroy()
        {
            NetworkTransport.Shutdown();
        }
        #endregion
        
        private void NetworkEventAvailable(int socketId)
        {
            _sockets[socketId].eventsReady = true;
        }
        private void ConnectionReadyForSend(int socketId, int connectionId)
        {
            _sockets[socketId].ConnectionReady(connectionId);
        }
        public int OpenSocket(SocketConfiguration sc)
        {
            var newSocket = new Socket(sc);
            _sockets[newSocket.id] = newSocket;

            return newSocket.id;
        }
        public void CloseSocket(int socketId)
        {
            _sockets[socketId] = null;
        }
        public void Send(int socketId, int connectionId, int channelId, ANetworkMessage message)
        {
            _sockets[socketId].Send(connectionId, channelId, message);
        }
        public void OpenConnection(int socketId, ConnectionConfiguration cc)
        {
            _sockets[socketId].OpenConnection(cc);
        }
        public void CloseConnection(int socketId, int connectionId)
        {
            _sockets[socketId].CloseConnection(connectionId);
        }
        private void ShowErrorIfThrown()
        {
            if ((NetworkError)_error != NetworkError.Ok)
                Debug.LogErrorFormat("NetworkError {0}", (NetworkError)_error);
        }
    }
}