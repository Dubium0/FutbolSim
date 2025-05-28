using UnityEngine;

namespace FootballSim.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "FootballSim/PlayerData")]
    public class FootballPlayerData : ScriptableObject
    {
        public float MaxWalkSpeed;
        public float MaxRunSpeed;
        public float WalkingAcceleration;
        public float RunningAcceleration;

        public float MaxRunningTime;

        public float RunningCooldown;

        public float RotationTime;

        public float MinimumPassPower;

        public float MaximumPassPower;

        public float MinimumLobPassPower;

        public float MaximumLobPassPower;

        public float MinimumThroughPassPower;

        public float MaximumThrougPassPower;

        public float MaximumShootPower;

        public float MinimumShootPower;

        public float PassAngle;
        public float ShootAngle;
        public float LobPassAngle;

        public float MinimumShootDistance;

        public float AggressiveDefenseDistance;
        public float PassiveDefenseDistance;

        public float MaxJumpPower = 10f; //Only used for goal keeper.


    }   

}