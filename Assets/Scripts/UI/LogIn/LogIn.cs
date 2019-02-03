using Events;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LogIn
{
    public class LogIn : MonoBehaviour
    {
        public InputField input;

        public void Click()
        {
            EventManager.Singleton.Publish(GameEventType.LogIntoLobby, input.text);
        } 
    }
}
