using Events;
using UnityEngine;

public class ReturnToMainMenu : MonoBehaviour
{
    public void Click()
    {
        EventManager.Singleton.Publish(GameEventType.ExitToMainMenu, null);
    }
}
