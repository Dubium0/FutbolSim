
using UnityEngine;

namespace FootballSim.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "FootballSim/PlayerData")]
    public class FootballPlayerData : ScriptableObject
    {
        public float MaxWalkSpeed;
        public float WalkingAcceleration;
        public float RotationTime;
        
    }   

}