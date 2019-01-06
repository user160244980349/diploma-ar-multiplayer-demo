using System;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Events.EventTypes
{
    public class ButtonClicked
    {
        public delegate void Callback(Button b);
        private Callback _c;

        public void Subscribe(Callback c)
        {
            _c += c;
        }
        public void Unsubscribe(Callback c)
        {
            _c -= c;
        }
        public void Publish(Button b)
        {
            _c(b);
        }
    }
}