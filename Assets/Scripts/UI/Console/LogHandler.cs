using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Diploma.UI {

    public class LogHandler : MonoBehaviour {

        public bool WithStackTrace = false;
        public Console console;

        void OnEnable () {
            Application.logMessageReceivedThreaded += SendLog;
        }

        void OnDisable () {
            Application.logMessageReceivedThreaded -= SendLog;
        }

        void SendLog (string condition, string stackTrace, UnityEngine.LogType type) {

            if (WithStackTrace && type != LogType.Log) {
                condition = string.Format("Message: {0}\nStackTrace: {1}", condition, stackTrace);
            }

            switch (type) {

                case UnityEngine.LogType.Log:
                    console.SendMessage(condition, Color.white);
                    break;

                case UnityEngine.LogType.Warning:
                    console.SendMessage(condition, Color.yellow);
                    break;

                case UnityEngine.LogType.Error:
                    console.SendMessage(condition, Color.red);
                    break;

                case UnityEngine.LogType.Exception:
                    console.SendMessage(condition, Color.red);
                    break;

                case UnityEngine.LogType.Assert:
                    console.SendMessage(condition, Color.red);
                    break;

            }

        }

    }

}
