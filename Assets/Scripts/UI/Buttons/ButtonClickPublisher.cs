using Events;
using Events.EventTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    public class ButtonClickPublisher : MonoBehaviour
    {
        private Button _button;
        private ButtonClicked _click;

        private void Start()
        {
            _button = GetComponent<Button>();

            _button.onClick.AddListener(Click);
            _click = EventManager.Instance.GetEvent<ButtonClicked>();
        }

        private void Click()
        {
            _click.Publish(_button);
        }
    }
}