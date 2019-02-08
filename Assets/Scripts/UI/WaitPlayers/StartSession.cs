using Events;
using Multiplayer.Messages.Responses;
using UnityEngine;

namespace UI.WaitPlayers
{
    public class StartSession : MonoBehaviour
    {
        public void Click()
        {
            EventManager.Singleton.Publish(GameEventType.SendMultiplayerMessage, new SessionStarted());
        }
    }
}
