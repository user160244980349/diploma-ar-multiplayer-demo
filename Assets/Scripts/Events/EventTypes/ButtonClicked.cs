using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Events.EventTypes {

    public class ButtonClicked : IGameEvent {

        List<Action<Button>> callbacks;

        public ButtonClicked () {
            callbacks = new List<Action<Button>>();
        }

        ~ButtonClicked () {
            foreach (Action<Button> callback in callbacks) {
                callbacks.Remove(callback);
            }
        }

        public void Subscribe (Action<Button> callback) {
            callbacks.Add(callback);
        }

        public void Unsubscribe (Action<Button> callback) {
            callbacks.Remove(callback);
        }

        public void Publish (Button button) {
            foreach (Action<Button> callback in callbacks) {
                callback(button);
            }
        }

    }

}
