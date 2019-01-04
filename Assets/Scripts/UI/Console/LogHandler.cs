using UnityEngine;

namespace UI.Console
{
    public class LogHandler : MonoBehaviour
    {
        public Console console;
        public bool WithStackTrace;

        private void OnEnable()
        {
            Application.logMessageReceivedThreaded += SendLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceivedThreaded -= SendLog;
        }

        private void SendLog(string condition, string stackTrace, LogType type)
        {
            if (WithStackTrace && type != LogType.Log)
                condition = string.Format("Message: {0}\nStackTrace: {1}", condition, stackTrace);

            switch (type)
            {
                case LogType.Log:
                    console.SendMessage(condition, Color.white);
                    break;

                case LogType.Warning:
                    console.SendMessage(condition, Color.yellow);
                    break;

                case LogType.Error:
                    console.SendMessage(condition, Color.red);
                    break;

                case LogType.Exception:
                    console.SendMessage(condition, Color.red);
                    break;

                case LogType.Assert:
                    console.SendMessage(condition, Color.red);
                    break;
            }
        }
    }
}