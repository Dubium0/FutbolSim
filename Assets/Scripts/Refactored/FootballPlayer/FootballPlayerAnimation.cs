using System;
using UnityEngine;

namespace FootballSim.Player
{

    public class FootballPlayerAnimation : MonoBehaviour
    {

        public event Action OnBallTouchEvent;

        public void OnBallTouch()
        {
            if (OnBallTouchEvent != null)
            {
                OnBallTouchEvent.Invoke();
                OnBallTouchEvent = null;// needs to add again
                
            }

        }

    }



}