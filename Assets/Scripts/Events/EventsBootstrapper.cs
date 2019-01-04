using System.Collections.Generic;
using Events.EventTypes;

namespace Events
{
    public static class EventsBootstrapper
    {
        public static void LoadEvents(out List<IGameEvent> events)
        {
            events = new List<IGameEvent>
            {
                new ButtonClicked()
            };
        }
    }
}