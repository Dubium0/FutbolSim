using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Player.Controller.States
{
    public class StrugglingFutbollerState : IPlayerState
    {
        private IFootballAgent controller_;
        private float currentAcceleration_;

        public StrugglingFutbollerState(IFootballAgent controller)
        {
            controller_ = controller;
           
        }

        public void HandleTransition()
        {
            if(!Football.Instance.IsPlayerStruggling(controller_))
            {
                if(Football.Instance.CurrentOwnerPlayer == controller_)
                {
                    controller_.SetState(new DribblingFutbollerState(controller_));
                }
                else
                {
                    controller_.SetState(new FreeFutbollerState(controller_));
                }
            }
        }

        public void Move()
        {
            var inputVector = controller_.MovementVector;

            controller_.Rigidbody.AddForce(inputVector * currentAcceleration_, ForceMode.Acceleration);
            Debug.Log(inputVector);
            if (inputVector.magnitude > 0)
            {
                controller_.Transform.forward = MathExtra.MoveTowards(controller_.Transform.forward, inputVector, 1 / controller_.AgentInfo.RotationTime);
            }
            Debug.Log("I'm struggling :<");
        }

        public void OnEnter()
        {
            currentAcceleration_ = controller_.AgentInfo.StrugglingAcceleration;
            controller_.Rigidbody.maxLinearVelocity = controller_.AgentInfo.MaxStrugglingSpeed;
        }

        public void OnExit()
        {
            Debug.Log("Bye Bye struggling state");
        }

        public void OnFixedUpdate()
        {
            HandleTransition();
        }

        public void OnHighActionAEnter()
        {
           //do nothing.
        }

        public void OnHighActionAExit()
        {
            // do nothing
        }

        public void OnHighActionBEnter()
        {
            // do nothing
        }

        public void OnHighActionBExit()
        {
            // do nothing
        }

        public void OnLowActionAEnter()
        {
            // do nothing
        }

        public void OnLowActionAExit()
        {
            // do nothing
        }

        public void OnLowActionBEnter()
        {
            // do nothing
        }

        public void OnLowActionBExit()
        {
            // do nothing
        }

        public void OnSprintEnter()
        {
            // NO ACTION
        }

        public void OnSprintExit()
        {
            // do nothing
        }
    }
}
