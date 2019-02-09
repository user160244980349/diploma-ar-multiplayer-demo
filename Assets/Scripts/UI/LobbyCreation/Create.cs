using Events;
using UnityEngine;
using UnityEngine.UI;

public class Create : MonoBehaviour
{
    public InputField input;

    public void Click()
    {
        EventManager.Singleton.Publish(GameEventType.CreateLobby, input.text);
    }
}
