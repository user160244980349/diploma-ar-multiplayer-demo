using System.Collections.Generic;

namespace Diploma.Events.GameEvents {

    public class EventsBootstrapper {

        public static void LoadEvents (out List<IGameEvent> events) {

            events = new List<IGameEvent> {
                new ButtonClicked(),
            };

        }

    }

}

