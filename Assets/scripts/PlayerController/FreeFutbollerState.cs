
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.Playables;
using Utility;
namespace Player.Controller
{

    public class FreeFutbollerState : IPlayerActions
    {
        private PlayerController controller_;
        private float currentAcceleration_;


        public FreeFutbollerState(PlayerController controller)
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

       
        public void HandleCollision(Collision collision)
        {

        }
    }
}
