using Player.Controller.States;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem.iOS;
using Utility;

namespace Player.Controller
{
    [RequireComponent(typeof(Rigidbody))]
    public class SoccerBall : MonoBehaviour
    {
        public static SoccerBall Instance { get; private set; }
        [SerializeField]
        private float playerCheckRadius_ = 2;

        [SerializeField]
        private float maxVelocityForAcqusition_;

        [SerializeField] private LayerMask playerCheckLayer_;

        private PlayerController currentOwnerPlayer_ = null;    
        public PlayerController CurrentOwnerPlayer { get { return currentOwnerPlayer_; } }

        private HashSet<PlayerController> strugglingPlayers_ = new HashSet<PlayerController>();

        private Rigidbody rigidbody_;

        public Rigidbody RigidBody { get => rigidbody_; }

        private bool isStruggling_ = false;
        public bool IsStruggling { get => isStruggling_; }

        [SerializeField] private GameEvent onBallOwnerTickEvent_;


        private void Awake()
        {
            if (Instance != null && Instance != this) { 
            
                Destroy(this);

            }
            else
            {
                Instance = this;
            }

            rigidbody_ = GetComponent<Rigidbody>();
        }
        private void FixedUpdate()
        {
            CheckPlayerCollision();
        }



        private void CheckPlayerCollision()
        {

            
            var colliders = Physics.OverlapSphere(transform.position, playerCheckRadius_, playerCheckLayer_);

            PlayerController playerToAssign = currentOwnerPlayer_;

            
          
            foreach (var collider in colliders) { 

                if(collider.TryGetComponent<PlayerController>(out var otherPlayer))
                {
                    strugglingPlayers_.Add(otherPlayer);
                    if (playerToAssign != null && playerToAssign.GetTeamIndex() != otherPlayer.GetTeamIndex())
                    {
                        var currentPlayerScore = playerToAssign.TryToAcquireBall();
                        var otherPlayerScore = otherPlayer.TryToAcquireBall();
                        playerToAssign = currentPlayerScore > otherPlayerScore ? playerToAssign : otherPlayer;
                        isStruggling_ = true;
                       
                    }
                    else
                    {
                        playerToAssign = otherPlayer;
                    }
                }
            }


            if (!isStruggling_)
            {
                strugglingPlayers_.Clear();
            }
            
            var prevOwner = currentOwnerPlayer_;
            currentOwnerPlayer_ = playerToAssign;
            onBallOwnerTickEvent_.Raise();

        }

    
    
        public void HitBall(Vector3 direction, float shootPower)
        {
            rigidbody_.AddForce(direction * shootPower,ForceMode.VelocityChange);
            currentOwnerPlayer_ = null; 
        }
        public bool IsPlayerStruggling(PlayerController player) { 
        
           return strugglingPlayers_.Contains(player);
        }
        
      

    }
}
