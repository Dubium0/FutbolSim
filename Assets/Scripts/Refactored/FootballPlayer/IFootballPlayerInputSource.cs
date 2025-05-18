using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FootballSim.Player
{


    public interface IFootballPlayerInputSource
    {
        public bool Enable { get; set; }
        public Vector3 MovementVector { get; }

        public Action OnLowActionAPerformed { set; }
        public Action OnLowActionACanceled { set; }

        public Action OnLowActionBPerformed { set; }
        public Action OnLowActionBCanceled { set; }

        public Action OnHighActionAPerformed { set; }
        public Action OnHighActionACanceled { set; }

        public Action OnHighActionBPerformed { set; }
        public Action OnHighActionBCanceled { set; }

        public Action OnSprintActionPerformed { set; }
        public Action OnSprintActionCanceled { set; }

        public void Init(int t_playerIndex = 0);
    }

}