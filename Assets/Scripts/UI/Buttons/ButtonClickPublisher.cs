using Events;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons
{
    public class ButtonClickPublisher : MonoBehaviour
    {
        private Button _button;

        #region MonoBehaviour
        private void Start()
        {
            _button = GetComponent<Button>();

            _button.onClick.AddListener(Click);
        }
        #endregion

        private void Click()
        {
            EventManager.Singleton.Publish(GameEventType.ButtonClicked, _button);
        }
    }
}