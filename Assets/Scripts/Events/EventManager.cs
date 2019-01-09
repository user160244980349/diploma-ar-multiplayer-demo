using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        private List<object> _events;
        public static EventManager Singleton { get; private set; }

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);

            EventsBootstrapper.LoadEvents(out _events);
        }
        #endregion

        public T GetEvent<T>()
        {
            return (T) _events.Find(e => e is T);
        }
    }
}