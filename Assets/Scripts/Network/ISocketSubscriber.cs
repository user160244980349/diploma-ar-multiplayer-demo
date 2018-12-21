
namespace Diploma.Network {

    public interface ISocketSubscriber {

        void Boot ();
        void Shutdown ();
        void Send ();
        void OnConnectEvent (int connection);
        void OnDataEvent (int connection, byte[] data, int dataSize);
        void OnBroadcastEvent (int connection);
        void OnDisconnectEvent (int connection);

    }

}