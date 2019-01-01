using System.Collections.Generic;
using Events.EventTypes;

namespace Events {

    public class EventsBootstrapper {

        public static void LoadEvents (out List<IGameEvent> events) {

            events = new List<IGameEvent> {
                new ButtonClicked(),
            };

        }

    }

}

