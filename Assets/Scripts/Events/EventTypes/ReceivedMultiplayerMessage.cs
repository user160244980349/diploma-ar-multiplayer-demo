using Multiplayer.Messages;
using Network.Messages;

namespace Events.EventTypes
{
    public class ReceivedMultiplayerMessage
    {
        public delegate void Callback(AMultiplayerMessage b);

        private Callback _c;
        public void Subscribe(Callback c)
        {
            _c += c;
        }
        public void Unsubscribe(Callback c)
        {
            _c -= c;
        }
        public void Publish(ANetworkMessage b)
        {
            _c((AMultiplayerMessage) b);
        }
    }
}