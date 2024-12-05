using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    
    public class GameEventListener : MonoBehaviour
    {
        public GameEvent Event;

        public UnityEvent Response;

        private void OnEnable()
        {
            //Debug.Log(gameObject.name + " Registered for event " + Event.name);
            Event.RegisterListener(this);
        }

        private void OnDisable()
        {
            //Debug.Log(gameObject.name + " Unregistered for event " + Event.name);
            Event.UnregisterListener(this);
        }

        public void OnEventRaised()
        {
           // Debug.Log(gameObject.name + " Invoked for event " + Event.name);
            Response.Invoke();
        }
    }
}
