using Events;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    public class ButtonClickPublisher : MonoBehaviour
    {
        private Button _button;

        private void Start()
        {
            _button = GetComponent<Button>();

            _button.onClick.AddListener(Click);
        }

        private void Click()
        {
            EventManager.Singleton.Publish(GameEventType.ButtonClicked, _button);
        }
    }
}