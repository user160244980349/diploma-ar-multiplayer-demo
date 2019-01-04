using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Events.EventTypes
{
    public class ButtonClicked : IGameEvent
    {
        private readonly List<Action<Button>> callbacks;

        public ButtonClicked()
        {
            callbacks = new List<Action<Button>>();
        }

        ~ButtonClicked()
        {
            foreach (var callback in callbacks) callbacks.Remove(callback);
        }

        public void Subscribe(Action<Button> callback)
        {
            callbacks.Add(callback);
        }

        public void Unsubscribe(Action<Button> callback)
        {
            callbacks.Remove(callback);
        }

        public void Publish(Button button)
        {
            foreach (var callback in callbacks) callback(button);
        }
    }
}