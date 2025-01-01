

using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UIElements;

namespace Player.Controller.States
{
    public class DribblingFutbollerState : IPlayerState
    {
        private IFootballAgent controller_;
        private float currentAcceleration_;

        public DribblingFutbollerState(IFootballAgent controller)
        {
            controller_ = controller;
        }

      
        public void Move()
        {
            var inputVector = controller_.MovementVector;

            controller_.Rigidbody.AddForce(inputVector * currentAcceleration_, ForceMode.Acceleration);

            if (inputVector.magnitude > 0)
            {
                controller_.Transform.forward = MathExtra.MoveTowards(controller_.Transform.forward, inputVector, 1 / controller_.AgentInfo.RotationTime);
            }
           
        }

        private Vector3 shootdir_;
        public void OnHighActionAEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.Transform.position).normalized;
            shootdir_.y = .5f;
        }

        public void OnHighActionAExit()
        {
            controller_.ChangeToGhostLayer(0.5f);
            Football.Instance.HitBall(shootdir_, 10);
            controller_.SetState(new FreeFutbollerState(controller_));
        }

        public void OnHighActionBEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.Transform.position).normalized;
            shootdir_.y = .5f;
        }

        public void OnHighActionBExit()
        {
            controller_.ChangeToGhostLayer(0.5f );
            Football.Instance.HitBall(shootdir_, 15);
            controller_.SetState(new FreeFutbollerState(controller_));
        }

       
        public void OnLowActionAEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.Transform.position).normalized;


        }

        public void OnLowActionAExit()
        {
            controller_.ChangeToGhostLayer(0.5f);
            Football.Instance.HitBall(shootdir_, 10);
            controller_.SetState(new FreeFutbollerState(controller_));


        }

        public void OnLowActionBEnter()
        {
            shootdir_ = (controller_.WorldMousePosition - controller_.Transform.position).normalized;
        }

        public void OnLowActionBExit()
        {
            controller_.ChangeToGhostLayer(0.5f);
            Football.Instance.HitBall(shootdir_, 10);
            controller_.SetState(new FreeFutbollerState(controller_));
        }

        public void OnSprintEnter()
        {
            controller_.Rigidbody.maxLinearVelocity = controller_.AgentInfo.MaxRunSpeed;
            currentAcceleration_ = controller_.AgentInfo.RunningAcceleration;
        }
        public void OnSprintExit()
        {

            controller_.Rigidbody.maxLinearVelocity = controller_.AgentInfo.MaxWalkSpeed;
            currentAcceleration_ = controller_.AgentInfo.WalkingAcceleration;

        }

        public void OnEnter()
        {
            controller_.SprintAction.performed += context => { OnSprintEnter(); };
            controller_.SprintAction.canceled += context => { OnSprintExit(); };

            controller_.Rigidbody.maxLinearVelocity = controller_.AgentInfo.MaxWalkSpeed;
            currentAcceleration_ = controller_.AgentInfo.WalkingAcceleration;
        }

        public void OnExit()
        {
           Debug.Log("Bye bye dribbling state");
        }
        private void AdjutBallPosition()
        {
            var targetPosition = controller_.FocusPointTransform.position;
            targetPosition.y = Football.Instance.RigidBody.position.y;
            Football.Instance.RigidBody.MovePosition(targetPosition);
        }
        public void OnFixedUpdate()
        {
            AdjutBallPosition();
            HandleTransition();
        }

        public void HandleTransition()
        {
            if (Football.Instance.IsPlayerStruggling(controller_))
            {
                controller_.SetState(new StrugglingFutbollerState(controller_));
            }
            else if (Football.Instance.CurrentOwnerPlayer != controller_)
            {
                controller_.SetState(new FreeFutbollerState(controller_));
            }
               
        }
    }


}