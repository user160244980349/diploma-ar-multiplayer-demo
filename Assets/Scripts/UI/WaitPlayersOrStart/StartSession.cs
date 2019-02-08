using Events;
using Multiplayer.Messages.Responses;
using UnityEngine;

namespace UI.WaitPlayersOrStart
{
    public class StartSession : MonoBehaviour
    {
        public void Click()
        {
            EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new SessionStarted());
        }
    }
}
