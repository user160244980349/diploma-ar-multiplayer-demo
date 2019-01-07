using Multiplayer.Messages;
using Network.Messages;
using UnityEngine.UI;

namespace Events.EventTypes
{
    public class MultiplayerMessageReady
    {
        public delegate void Callback(ANetworkMessage b);
        private Callback _c;

        public void Subscribe(Callback c)
        {
            _c += c;
        }
        public void Unsubscribe(Callback c)
        {
            _c -= c;
        }
        public void Publish(AMultiplayerMessage b)
        {
            _c((ANetworkMessage)b);
        }
    }
}