using System.Collections.Generic;
using UnityEngine;

namespace Player.Controller
{
    public class SoccerBall : MonoBehaviour
    {

        private float playerCheckRadius_ = 2;

        [SerializeField] private LayerMask playerCheckLayer_;

        private PlayerController currentOwnerPlayer_ = null;    

        private void FixedUpdate()
        {
            CheckPlayerCollision();
        }

        private void CheckPlayerCollision()
        {
            var colliders = Physics.OverlapSphere(transform.position, playerCheckRadius_, playerCheckLayer_);

            PlayerController playerToAssign = null;

            foreach (var collider in colliders) { 

                if(collider.TryGetComponent<PlayerController>(out var playerController))
                {
                    if (playerToAssign != null) {
                        if (playerToAssign.GetAcqusitionScore() < playerController.GetAcqusitionScore()) {
                            playerToAssign = playerController;
                        
                        }
                    }
                    else
                    {
                        playerToAssign = playerController;
                    }
                }
            
            }

            currentOwnerPlayer_ = playerToAssign;

        }



    }
}
