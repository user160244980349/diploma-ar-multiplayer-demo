using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Singleton { get; private set; }

        private List<object> _events;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "EventManager";
            EventsBootstrapper.LoadEvents(out _events);
        }
        #endregion

        public T GetEvent<T>()
        {
            return (T) _events.Find(e => e is T);
        }
    }
}