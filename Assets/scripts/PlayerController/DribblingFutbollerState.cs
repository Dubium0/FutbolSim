

using UnityEngine;

namespace Player.Controller
{



    public class DribblingFutbollerState : IPlayerActions
    {
        private PlayerController controller_;
        private float currentAcceleration_;


        public DribblingFutbollerState(PlayerController controller)
        {
            controller_ = controller;

            controller_.SprintAction.performed += context => { OnSprintEnter(); };
            controller_.SprintAction.canceled += context => { OnSprintExit(); };

            controller_.Rigidbody.maxLinearVelocity = controller.PlayerData.MaxWalkSpeed;
            currentAcceleration_ = controller_.PlayerData.WalkingAcceleration;
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

        public void OnHighActionAEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionAExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionBEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionBExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionAEnter()
        {
            var shootDirection = (controller_.WorldMousePosition - controller_.transform.position).normalized;
        }

        public void OnLowActionAExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionBEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionBExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnSprintEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnSprintExit()
        {
            throw new System.NotImplementedException();
        }

        public void HandleCollision(Collision collision)
        {


        }
    }


}