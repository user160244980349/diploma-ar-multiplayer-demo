using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Connection {

        public int Id { get { return id; } }

        int id;
		bool opened;
        string ip;
        int port;
        Socket socket;
        int exceptionConnectionId;
        int notificationLevel;

        public Connection(ConnectionConfiguration cc) {

            opened                  = true;
            id                      = cc.id;
            ip                      = cc.ip;
            port                    = cc.port;
            socket                  = cc.socket;
            exceptionConnectionId   = cc.exceptionConnectionId;
            notificationLevel       = cc.notificationLevel;
        }

        public void Open () {
            byte error;
            id = NetworkTransport.Connect(socket.Id, ip, port, exceptionConnectionId, out error);

            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

            // Debug.Log(string.Format("Opened connection: {0}", id));
        }

        public void Close () {
            byte error;
            socket.UnregisterConnection(this);
            NetworkTransport.Disconnect(socket.Id, id, out error);

            if ((NetworkError)error != NetworkError.Ok)
                Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

            // Debug.Log(string.Format("Closed connection: {0}", id));
        }

        public void Send (byte[] buffer, int size) {
            byte error;
            if (opened) {
                Channel channel;
				socket.Channels.TryGetValue(0, out channel);
                if (!NetworkTransport.Send(socket.Id, id, channel.id, buffer, size, out error)) {
                    
                    if ((NetworkError)error != NetworkError.Ok)
                        Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));

                    opened = false;
                    NetworkTransport.NotifyWhenConnectionReadyForSend(socket.Id, id, notificationLevel, out error);
                    
                    if ((NetworkError)error != NetworkError.Ok)
                        Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
                }
                
                if ((NetworkError)error != NetworkError.Ok)
                    Debug.LogError(string.Format("NetworkError {0}", (NetworkError)error));
            }
        }

        public void OnReady () {
            opened = true;
        }

    }

}
