using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class NetworkManager : MonoBehaviour {

        static NetworkManager instance = null;

        GlobalConfig config;
        Dictionary<int, Socket> sockets;

        public static NetworkManager GetInstance() {
            return instance;
        }

        void Awake () {
   
            if (instance == null) {
                instance = this; 
            } else {
                Destroy(this);
            }

            sockets = new Dictionary<int, Socket>();
            config = new GlobalConfig
            {
                ConnectionReadyForSend = OnConnectionReady,
                NetworkEventAvailable = OnNetworkEvent
            };
            NetworkTransport.Init(config);
        }

        void Update () {
            foreach (Socket socket in sockets.Values) {
                socket.OnNetworkEvents();
            }
        }

        void OnDestroy () {
            NetworkTransport.Shutdown();
        }

        public void OnConnectionReady (int socketId, int connectionId) {
            Socket socket;
            if (sockets.TryGetValue(socketId, out socket)) {
                socket.OnConnectionReady(connectionId);
            }
        }

        public void OnNetworkEvent (int socketId) {
            Socket socket;
            if (sockets.TryGetValue(socketId, out socket)) {
                socket.OnEventsAvaliable();
            }
        }

        public void RegisterSocket (Socket socket) { 
            sockets.Add(socket.Id, socket);
        }

        public void UnregisterSocket (Socket socket) {
            sockets.Remove(socket.Id);
        }

	}

}