using Events;
using Events.EventTypes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Buttons {

    public class ButtonClickPublisher : MonoBehaviour {

        Button button;
        ButtonClicked click;

        void Start () {
            button = GetComponent<Button>();

            button.onClick.AddListener(Click);
            click = EventManager.GetInstance().GetEvent<ButtonClicked>();
        }

        void Click () {
            click.Publish(button);
        }

    }

}
