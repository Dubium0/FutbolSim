

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

namespace Player.Controller.States
{
    public class DribblingFutbollerState : IPlayerState
    {
        private PlayerController controller_;
        private float currentAcceleration_;

        public DribblingFutbollerState(PlayerController controller)
        {
            controller_ = controller;
        }

      
        public void Move()
        {
            var inputVector = controller_.MovementVector;

            controller_.Rigidbody.AddForce(inputVector * currentAcceleration_, ForceMode.Acceleration);

            if (inputVector.magnitude > 0)
            {
                controller_.transform.forward = MathExtra.MoveTowards(controller_.transform.forward, inputVector, 1 / controller_.PlayerData.RotationTime);
            }

        }
        private void AdjutBallPosition()
        {
            var targetPosition = controller_.GetFocusPoint();
            targetPosition.y = SoccerBall.Instance.RigidBody.position.y;
            SoccerBall.Instance.RigidBody.MovePosition(targetPosition);
        }
        private Vector3 shootdir_;
        public void OnHighActionAEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.transform.position).normalized;
            shootdir_.y = .5f;
        }

        public void OnHighActionAExit()
        {
            controller_.ChangeToGhostLayer();
            SoccerBall.Instance.HitBall(shootdir_, 10);
            controller_.SetState(new FreeFutbollerState(controller_));
        }

        public void OnHighActionBEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.transform.position).normalized;
            shootdir_.y = .5f;
        }

        public void OnHighActionBExit()
        {
            controller_.ChangeToGhostLayer();
            SoccerBall.Instance.HitBall(shootdir_, 15);
            controller_.SetState(new FreeFutbollerState(controller_));
        }

       
        public void OnLowActionAEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.transform.position).normalized;


        }

        public void OnLowActionAExit()
        {
            controller_.ChangeToGhostLayer();
            SoccerBall.Instance.HitBall(shootdir_, 10);
            controller_.SetState(new FreeFutbollerState(controller_));


        }

        public void OnLowActionBEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.transform.position).normalized;
        }

        public void OnLowActionBExit()
        {
            controller_.ChangeToGhostLayer();
            SoccerBall.Instance.HitBall(shootdir_, 15);
            controller_.SetState(new FreeFutbollerState(controller_));
        }

        public void OnSprintEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnSprintExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnEnter()
        {
            controller_.Rigidbody.maxLinearVelocity = controller_.PlayerData.MaxWalkSpeed;
            currentAcceleration_ = controller_.PlayerData.WalkingAcceleration;
        }

        public void OnExit()
        {
            controller_.Debugger.Log("Bye bye dribbling state");
        }

        public void OnFixedUpdate()
        {
            AdjutBallPosition();
        }

        public void HandleTransition()
        {
            if (SoccerBall.Instance.IsPlayerStruggling(controller_))
            {
                controller_.SetState(new StrugglingFutbollerState(controller_));
            }
            else if (SoccerBall.Instance.CurrentOwnerPlayer != controller_)
            {
                controller_.SetState(new FreeFutbollerState(controller_));
            }
               
        }
    }


}