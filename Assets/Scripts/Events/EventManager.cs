using System.Collections.Generic;
using Events.EventTypes;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        private List<IGameEvent> _events;

        public static EventManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(this);

            EventsBootstrapper.LoadEvents(out _events);
        }

        public T GetEvent<T>()
        {
            return (T) _events.Find(e => e is T);
        }
    }
}