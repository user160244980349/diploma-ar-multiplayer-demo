using Events;
using UnityEngine;

namespace UI.MainMenu
{
    public class Discovery : MonoBehaviour
    {
        public void Click()
        {
            EventManager.Singleton.Publish(GameEventType.BecomeClient, null);
        }
    }
}
