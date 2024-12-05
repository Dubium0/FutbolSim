
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
  
    [CreateAssetMenu(fileName = "GameEvent", menuName = "YunYun Tools/Game Event")]
    public class GameEvent : ScriptableObject
    {
        private List<GameEventListener> listeners =
            new List<GameEventListener>();

        public void Raise()
        {
            for (int i = listeners.Count - 1; i >= 0; i--)
            {
                listeners[i].OnEventRaised();
                //Debug.Log("Event raised");
            }
        }

        public void RegisterListener(GameEventListener listener)
        {
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
        public void UnregisterListener(GameEventListener listener)
        {
            if (listeners.Contains(listener))
            {
                listeners.Remove(listener);
            }
        }
    }
}
