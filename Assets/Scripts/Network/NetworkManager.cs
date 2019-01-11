using Network.Messages;
using UnityEngine;
using UnityEngine.Networking;

namespace Network
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Singleton { get; private set; }

        private const int MaxSockets = 16;

        private Socket[] _sockets;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "NetworkManager";
            _sockets = new Socket[MaxSockets];

            var config = new GlobalConfig {
                NetworkEventAvailable = NetworkEventAvailable,
                ConnectionReadyForSend = ConnectionReadyForSend,
            };
            NetworkTransport.Init(config);
        }
        private void OnDestroy()
        {
            NetworkTransport.Shutdown();
        }
        #endregion

        public void RegisterSocket(Socket socket)
        {
            _sockets[socket.Id] = socket;
        }
        public void UnregisterSocket(Socket socket)
        {
            _sockets[socket.Id] = null;
        }
        private void NetworkEventAvailable(int socketId)
        {
            _sockets[socketId].EventsReady();
        }
        private void ConnectionReadyForSend(int socketId, int connectionId)
        {
            _sockets[socketId].ConnectionReady(connectionId);
        }
    }
}