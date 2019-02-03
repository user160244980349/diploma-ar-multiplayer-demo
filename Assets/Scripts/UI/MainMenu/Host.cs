using Events;
using UnityEngine;

namespace UI.MainMenu
{
    public class Host : MonoBehaviour
    {
        public void Click()
        {
            EventManager.Singleton.Publish(GameEventType.BecomeHost, null);
        }
    }
}
