using UnityEngine;

namespace Player.Controller
{
    [CreateAssetMenu(fileName = "Player Data",menuName ="Futbol Sim/Player/Player Data")]
    public class PlayerData : ScriptableObject
    {
        //same for all players
        public const int BallAcqusitionStaminaReductionRate = 5;


        // might change by player
        [SerializeField] private float maxWalkSpeed_ = 4.0f;
        public float MaxWalkSpeed { get => maxWalkSpeed_;  }
        
        [SerializeField] private float maxRunSpeed_ = 9.0f;
        public float MaxRunSpeed { get => maxRunSpeed_;  }

        [SerializeField] private float walkingAcceleration_ = 6.0f;
        public float WalkingAcceleration { get => walkingAcceleration_; }

        [SerializeField] private float runningAcceleration_ = 12.0f;
        public float RunningAcceleration { get => runningAcceleration_; }

        [SerializeField] private float rotationTime_ = 2;
        public float RotationTime { get => rotationTime_;  }

        [SerializeField] private float ballAcqusitionRadius_ = 1;
        public float BallAcqusitionRadius { get => ballAcqusitionRadius_;}
 
        [SerializeField] private LayerMask ballAcquisitonLayers_;
        public LayerMask BallAcquisitonLayers {  get =>  ballAcquisitonLayers_; }


        [SerializeField,Range(10,int.MaxValue)] private int maxBallAcqusitionStamina_;
        public int MaxBallAcqusitionStamina { get => maxBallAcqusitionStamina_; }


        [SerializeField] private int ballAcqusitionPoint_;
        public int BallAcqusitionPoint { get => ballAcqusitionPoint_; }
    }


}
