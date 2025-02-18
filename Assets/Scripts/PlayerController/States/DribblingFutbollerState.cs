﻿

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
            var mousePos = controller_.WorldMousePosition;
            mousePos.y = controller_.Transform.position.y;
            shootdir_ = (mousePos - controller_.Transform.position);
            shootdir_.Normalize();  
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
            var mousePos = controller_.WorldMousePosition;
            mousePos.y = controller_.Transform.position.y;
            shootdir_ = (mousePos - controller_.Transform.position);
            shootdir_.Normalize();
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
       
            var mousePos = controller_.WorldMousePosition;
            mousePos.y = controller_.Transform.position.y;
            shootdir_ = (mousePos - controller_.Transform.position);
            shootdir_.Normalize();
        


        }

        public void OnLowActionAExit()
        {
            controller_.ChangeToGhostLayer(0.5f);
            Football.Instance.HitBall(shootdir_, 15);
            controller_.SetState(new FreeFutbollerState(controller_));
            // Debug.Log(shootdir_);

        }

        public void OnLowActionBEnter()
        {

            var mousePos = controller_.WorldMousePosition;
            mousePos.y = controller_.Transform.position.y;
            shootdir_ = (mousePos - controller_.Transform.position);
            shootdir_.Normalize();
        }

        public void OnLowActionBExit()
        {
          
            controller_.ChangeToGhostLayer(0.5f);
            Football.Instance.HitBall(shootdir_, 15);
            controller_.SetState(new FreeFutbollerState(controller_));
            // Debug.Log(shootdir_);
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
           // Debug.Log("Bye bye dribbling state");
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