
using UnityEngine;

namespace Utility
{
 
    public enum Severity
    {
        INFO = 2,
        WARNING = 1,
        ERROR = 0
    }
    public class DebugUtils
    {
        private string name_ = "NoName";
      
        private bool enable_ = true;

        private Severity severity_ = Severity.ERROR;

        public void SetEnable(bool value)
        {
            enable_ = value;
        }
        public void SetSeverity(Severity severity)
        {

            severity_ = severity;
        }
        public void Log(System.Object message, Severity severity = Severity.INFO)
        {
            if (enable_ == false) { return; }
            if (severity <= severity_) {

                switch (severity) {
                
                    case Severity.INFO:
                        Debug.Log(message);
                        break;
                    case Severity.WARNING:
                        Debug.LogWarning(message); 
                        break;
                    case Severity.ERROR:
                        Debug.LogError(message);
                    break;
                }
            }
        }
     

    }


}