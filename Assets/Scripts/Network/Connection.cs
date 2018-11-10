using System.Net.Sockets;
using UnityEngine;
using UnityEngine.Networking;

namespace Diploma.Network {

    public class Connection {

        public int Id { get { return id; } }

        int id;
		bool opened = false;
		ConnectionConfiguration config;

		public Connection(ConnectionConfiguration cc) {
            byte error;
			config = cc;
			id = NetworkTransport.Connect(config.socket.Id, config.ip, config.port, config.exceptionConnectionId, out error);
			config.socket.RegisterConnection(this);
            Debug.Log(string.Format("Opened connection: {0}", id));
        }

        ~Connection () {
            byte error;
            NetworkTransport.Disconnect(config.socket.Id, id, out error);
        }

        public void Send (byte[] buffer, int size) {
            byte error;
            if (opened) {
                Channel channel;
				config.socket.Channels.TryGetValue(0, out channel);
                if (!NetworkTransport.Send(config.socket.Id, id, channel.id, buffer, size, out error)) {
                    opened = false;
                    NetworkTransport.NotifyWhenConnectionReadyForSend(config.socket.Id, id, config.notificationLevel, out error);
                }
            }
        }

        public void OnReady () {
            opened = true;
        }

    }

}
