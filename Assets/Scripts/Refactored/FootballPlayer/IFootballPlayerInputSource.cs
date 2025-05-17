using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FootballSim.Player
{


    public interface IFootballPlayerInputSource
    {
        public bool Enable { get; set; }
        public Vector3 MovementVector { get; }

        public bool IsLowActionAPerformed { get; }
        public bool IsLowActionACanceled { get; }

        public bool IsLowActionBPerformed { get; }
        public bool IsLowActionBCanceled { get; }

        public bool IsHighActionAPerformed { get; }
        public bool IsHighActionACanceled { get; }

        public bool IsHighActionBPerformed { get; }
        public bool IsHighActionBCanceled { get; }

        public bool IsSprintActionPerformed { get; }
        public bool IsSprintActionCanceled { get; }
    }

}