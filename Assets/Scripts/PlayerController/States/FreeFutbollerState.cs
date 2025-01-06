
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Player.Controller.States
{

    public class FreeFutbollerState : IPlayerState
    {
        private IFootballAgent controller_;
        private float currentAcceleration_;


        public FreeFutbollerState(IFootballAgent controller)
        {
            controller_ = controller;

        }

        public void HandleTransition()
        {
            if (Football.Instance.IsPlayerStruggling(controller_))
            {
                controller_.SetState(new StrugglingFutbollerState(controller_));
            }else if (Football.Instance.CurrentOwnerPlayer == controller_)
            {
                controller_.SetState(new DribblingFutbollerState(controller_));
            }
              
        }

        public void Move()
        {
            var inputVector = controller_.MovementVector;
                
            controller_.Rigidbody.AddForce(inputVector * currentAcceleration_, ForceMode.Acceleration);

            if (inputVector.magnitude > 0)
            {
                controller_.Transform.forward = MathExtra.MoveTowards(controller_.Transform.forward, inputVector, 1 / controller_.AgentInfo.RotationTime);
            }
            // Debug.Log("I'm free!");

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
            // Debug.Log("Bye Bye free state");
        }

        public void OnFixedUpdate()
        {
            HandleTransition();
        }

        public void OnHighActionAEnter()
        {
            Debug.Log("High A en");
        }

        public void OnHighActionAExit()
        {
            Debug.Log("High A ex");
        }

        public void OnHighActionBEnter()
        {
            Debug.Log("High B en");
        }

        public void OnHighActionBExit()
        {
            Debug.Log("High B ex");
        }

        public void OnLowActionAEnter()
        {
            Debug.Log("low A en");
        }

        public void OnLowActionAExit()
        {
            Debug.Log("low A ex");
        }

        public void OnLowActionBEnter()
        {
            Debug.Log("low B en");
        }

        public void OnLowActionBExit()
        {
            Debug.Log("low  B ex");
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

       
       
    }
}
