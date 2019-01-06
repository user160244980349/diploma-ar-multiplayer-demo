using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        private static EventManager _instance;

        private List<object> _events;

        #region MonoBehaviour
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance == this)
            {
                Destroy(gameObject);
            }
            DontDestroyOnLoad(gameObject);

            EventsBootstrapper.LoadEvents(out _events);
        }
        private void Start()
        {

        }
        private void Update()
        {

        }
        #endregion

        public static EventManager GetInstance()
        {
            return _instance;
        }
        public T GetEvent<T>()
        {
            return (T)_events.Find(e => e is T);
        }
    }
}
