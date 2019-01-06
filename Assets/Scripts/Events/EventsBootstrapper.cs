using System.Collections.Generic;
using Events.EventTypes;

namespace Events
{
    public static class EventsBootstrapper
    {
        public static void LoadEvents(out List<object> events)
        {
            events = new List<object>
            {
                new ButtonClicked()
            };
        }
    }
}