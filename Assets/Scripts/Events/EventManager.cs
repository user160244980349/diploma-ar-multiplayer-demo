using System.Collections.Generic;
using Events.EventTypes;
using UnityEngine;

namespace Events {

    public class EventManager : MonoBehaviour {

        static EventManager instance = null;
        List<IGameEvent> events = null;

        void Awake () {

            if (instance == null) {
                instance = this;
            } else {
                Destroy(this);
            }
            EventsBootstrapper.LoadEvents(out events);
        }

        public static EventManager GetInstance () {
            return instance;
        }

        public T GetEvent<T> () {
            return (T)(events.Find(e => e is T));
        }

    }

}
