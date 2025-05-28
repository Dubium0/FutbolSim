using System;
using UnityEngine;

namespace FootballSim.Player
{

    public interface IFootballPlayerInputSource
    {
        public bool Enable { get; set; }
        public Vector3 MovementVector { get; }

        public Action OnPassActionPerformed { set; }
        public Action OnPassActionCanceled { set; }

        public Action OnThroughPassActionPerformed { set; }
        public Action OnThroughPassActionCanceled { set; }

        public Action OnLobPassActionPerformed { set; }
        public Action OnLobPassActionCanceled { set; }

        public Action OnShootActionPerformed { set; }
        public Action OnShootActionCanceled { set; }

        public Action OnSprintActionPerformed { set; }
        public Action OnSprintActionCanceled { set; }

        public void Init(int t_playerIndex = 0);
    }

}