using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        private List<object> _events;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance == this)
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

        public T GetEvent<T>()
        {
            return (T)_events.Find(e => e is T);
        }
    }
}