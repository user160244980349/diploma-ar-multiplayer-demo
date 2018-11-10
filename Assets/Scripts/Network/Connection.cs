using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Connection {

        public int Id { get { return id; } }

        int id;
		bool opened = false;
		Socket socket;
		ConnectionConfiguration config;

		public Connection(ConnectionConfiguration cc) {
            byte error;

			Debug.Log(cc);

			config = cc;
			id = NetworkTransport.Connect(socket.Id, config.ip, config.port, config.exceptionConnectionId, out error);
            socket.RegisterConnection(this);
            Debug.Log(string.Format("Opened connection: {0}", id));
        }

        ~Connection () {
            byte error;
            NetworkTransport.Disconnect(socket.Id, id, out error);
        }

        public void Send (byte[] buffer, int size) {
            byte error;
            if (opened) {
                Channel channel;
                socket.Channels.TryGetValue(0, out channel);
                if (!NetworkTransport.Send(socket.Id, id, channel.id, buffer, size, out error)) {
                    opened = false;
                    NetworkTransport.NotifyWhenConnectionReadyForSend(socket.Id, id, config.notificationLevel, out error);
                }
            }
        }

        public void OnReady () {
            opened = true;
        }

    }
}
