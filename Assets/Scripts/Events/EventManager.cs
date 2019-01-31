using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Singleton { get; private set; }
        public delegate void Listener(object info);

        private Dictionary<GameEventType, Listener> _listeners;
        private Queue<EventWrapper> _events;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "EventManager";

            _listeners = new Dictionary<GameEventType, Listener>();
            _events = new Queue<EventWrapper>();
        }
        private void Update()
        {
            while (_events.Count > 0)
            {
                var queuedEvent = _events.Dequeue();
                if (!_listeners.ContainsKey(queuedEvent.type)) continue;
                _listeners.TryGetValue(queuedEvent.type, out Listener listener);
                listener(queuedEvent.info);
            }
        }
        #endregion

        public void Publish(GameEventType type, object info)
        {
            _events.Enqueue(new EventWrapper {
                type = type,
                info = info,
            });
        }
        public void ImmediatePublish(GameEventType type, object info)
        {
            if (!_listeners.TryGetValue(type, out Listener listener)) return;
            listener(info);
        }
        public void RegisterListener(GameEventType type, Listener newListener)
        {
            if (_listeners.ContainsKey(type))
            {
                _listeners.TryGetValue(type, out Listener listener);
                listener += newListener;
            }
            else
            {
                _listeners.Add(type, newListener);
            }
        }
        public void UnregisterListener(GameEventType type, Listener newListener)
        {
            if (!_listeners.TryGetValue(type, out Listener listener)) return;
            listener -= newListener;
        }
    }
}