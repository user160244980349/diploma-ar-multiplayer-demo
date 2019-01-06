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

        #region MonoBehaviour
        private void Start()
        {
            _button = GetComponent<Button>();

            _button.onClick.AddListener(Click);
            _click = EventManager.GetInstance().GetEvent<ButtonClicked>();
        }
        #endregion

        private void Click()
        {
            _click.Publish(_button);
        }
    }
}
