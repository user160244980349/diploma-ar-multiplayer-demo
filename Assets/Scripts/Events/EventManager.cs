using System.Collections.Generic;
using UnityEngine;

namespace Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Singleton { get; private set; }
        public delegate void Listener(object info);

        private Dictionary<GameEventType, Listener> _listeners;
        private Queue<GameEventType> _events;
        private Queue<object> _infos;

        #region MonoBehaviour
        private void Awake()
        {
            if (Singleton == null)
                Singleton = this;
            else if (Singleton == this) Destroy(gameObject);

            DontDestroyOnLoad(gameObject);
            gameObject.name = "EventManager";

            Init();
        }
        private void LateUpdate()
        {
            while (_events.Count > 0)
            {
                var queuedEvent = _events.Dequeue();
                var queuedInfo = _infos.Dequeue();
                if (_listeners.ContainsKey(queuedEvent))
                {
                    _listeners.TryGetValue(queuedEvent, out Listener listener);
                    listener(queuedInfo);
                }
            }
        }
        #endregion

        public void Publish(GameEventType type, object info)
        {
            _events.Enqueue(type);
            _infos.Enqueue(info);
        }
        public void ImmediatePublish(GameEventType type, object info)
        {
            if (_listeners.TryGetValue(type, out Listener listener))
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
            if (_listeners.TryGetValue(type, out Listener listener))
                listener -= newListener;
        }

        private void Init()
        {
            _listeners = new Dictionary<GameEventType, Listener>();
            _events = new Queue<GameEventType>();
            _infos = new Queue<object>();
        }
    }
}