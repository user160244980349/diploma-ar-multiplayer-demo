namespace Events
{
    public class GameEvent
    {
        public GameEventType Type { get; protected set; }
        public object Info { get; protected set; }

        public GameEvent(GameEventType type, object info)
        {
            Type = type;
            Info = info;
        }
    }
}