using Events;
using UnityEngine;

public class Leave : MonoBehaviour
{
    public void Click()
    {
        EventManager.Singleton.Publish(GameEventType.LogOutLobby, null);
    }
}
