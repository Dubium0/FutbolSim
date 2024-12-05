
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Player.Controller.States
{

    public class FreeFutbollerState : IPlayerState
    {
        private PlayerController controller_;
        private float currentAcceleration_;


        public FreeFutbollerState(PlayerController controller)
        {
            controller_ = controller;

        }

        public void HandleTransition()
        {
            if (SoccerBall.Instance.IsPlayerStruggling(controller_))
            {
                controller_.SetState(new StrugglingFutbollerState(controller_));
            }else if (SoccerBall.Instance.CurrentOwnerPlayer == controller_)
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
                controller_.transform.forward = MathExtra.MoveTowards(controller_.transform.forward, inputVector, 1 / controller_.PlayerData.RotationTime);
            }

        }

        public void OnEnter()
        {
            controller_.SprintAction.performed += context => { OnSprintEnter(); };
            controller_.SprintAction.canceled += context => { OnSprintExit(); };

            controller_.Rigidbody.maxLinearVelocity = controller_.PlayerData.MaxWalkSpeed;
            currentAcceleration_ = controller_.PlayerData.WalkingAcceleration;
        }

        public void OnExit()
        {
            controller_.Debugger.Log("Bye Bye free state");
        }

        public void OnFixedUpdate()
        {
           
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
            controller_.Rigidbody.maxLinearVelocity = controller_.PlayerData.MaxRunSpeed;
            currentAcceleration_ = controller_.PlayerData.RunningAcceleration;
        }
        public void OnSprintExit()
        {

            controller_.Rigidbody.maxLinearVelocity = controller_.PlayerData.MaxWalkSpeed;
            currentAcceleration_ = controller_.PlayerData.WalkingAcceleration;

        }

       
       
    }
}
