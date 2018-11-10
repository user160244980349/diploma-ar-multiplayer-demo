
namespace Diploma.Network {

    public interface ISocketSubscriber {

        void Boot ();
        void Shutdown ();
        void Send ();
        void OnConnectEvent (Connection connection);
        void OnDataEvent (Connection connection, byte[] data, int dataSize);
        void OnBroadcastEvent (Connection connection);
        void OnDisconnectEvent (Connection connection);

    }

}