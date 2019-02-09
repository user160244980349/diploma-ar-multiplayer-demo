using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Singleton { get; private set; }
        public delegate void Listener(object info);

        private Dictionary<GameEventType, List<Listener>> _map;
        private Queue<EventWrapper> _events;

        public void Publish(GameEventType type, object info)
        {
            _events.Enqueue(new EventWrapper
            {
                type = type,
                info = info,
            });
        }
        public void ImmediatePublish(GameEventType type, object info)
        {
            if (!_map.TryGetValue(type, out List<Listener> listeners)) return;
            for (var i = 0; i < listeners.Count; i++)
            {
                listeners[i](info);
            }
        }
        public void Subscribe(GameEventType type, Listener newListener)
        {
            if (_map.ContainsKey(type))
            {
                _map.TryGetValue(type, out List<Listener> listeners);
                listeners.Add(newListener);
            }
            else
            {
                var listeners = new List<Listener>
                {
                    newListener
                };
                _map.Add(type, listeners);
            }
        }
        public void Unsubscribe(GameEventType type, Listener removingListener)
        {
            if (!_map.TryGetValue(type, out List<Listener> listeners)) return;
            listeners.Remove(removingListener);
        }

        private void Awake()
        {
            name = "EventManager";
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);
            DontDestroyOnLoad(gameObject);

            _map = new Dictionary<GameEventType, List<Listener>>();
            _events = new Queue<EventWrapper>();
        }
        private void Update()
        {
            while (_events.Count > 0)
            {
                var queuedEvent = _events.Dequeue();
                if (!_map.ContainsKey(queuedEvent.type)) continue;
                _map.TryGetValue(queuedEvent.type, out List<Listener> listeners);
                for (var i = 0; i < listeners.Count; i++)
                {
                    listeners[i](queuedEvent.info);
                }
            }
        }
    }
}