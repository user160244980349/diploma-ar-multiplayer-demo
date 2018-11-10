using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Diploma.Events;
using Diploma.Events.GameEvents;

namespace Diploma.UI {

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
