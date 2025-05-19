using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;
using UnityEngine.Video;


namespace FootballSim.Player
{

    public class FootballPlayerInputSource_Offline : MonoBehaviour, IFootballPlayerInputSource
    {

        public Vector3 MovementVector {get{ if (IsWorkingConditionMet()) return GetMovementVector(); else return Vector3.zero; }}

        public Action OnLowActionAPerformed     { set { m_OnLowActionAPerformed = value;} }
        public Action OnLowActionACanceled      { set { m_OnLowActionACanceled = value;} }
        public Action OnLowActionBPerformed     { set { m_OnLowActionBPerformed = value;} }
        public Action OnLowActionBCanceled      { set { m_OnLowActionBCanceled = value;} }
        public Action OnHighActionAPerformed    { set { m_OnHighActionAPerformed = value;} }
        public Action OnHighActionACanceled     { set { m_OnHighActionACanceled = value;} }
        public Action OnHighActionBPerformed    { set { m_OnHighActionBPerformed = value;} }
        public Action OnHighActionBCanceled     { set { m_OnHighActionBCanceled = value;} }
        public Action OnSprintActionPerformed   { set { m_OnSprintActionPerformed = value;} }
        public Action OnSprintActionCanceled    { set { m_OnSprintActionCanceled = value;} }

        private Action m_OnLowActionAPerformed;
        private Action m_OnLowActionACanceled;
        private Action m_OnLowActionBPerformed;
        private Action m_OnLowActionBCanceled;
        private Action m_OnHighActionAPerformed;
        private Action m_OnHighActionACanceled;
        private Action m_OnHighActionBPerformed;
        private Action m_OnHighActionBCanceled;
        private Action m_OnSprintActionPerformed;
        private Action m_OnSprintActionCanceled;





        private InputAction m_MoveAction;
        private InputAction m_SprintAction;
        private InputAction m_LowActionA;
        private InputAction m_LowActionB;
        private InputAction m_HighActionA;
        private InputAction m_HighActionB;
        private InputAction m_LookAction;

        private bool m_Enable = false;
        public bool EnableDebug = true;
        public bool Enable { get { return m_Enable; } set { m_Enable = value; } }

       

        private void SetActions(int t_playerIndex = 0)
        {
            string playerPrefix;
            if (t_playerIndex  == 1)
            {
                playerPrefix ="Keyboard Only/" ;
            }
            else if (t_playerIndex == 2)
            {
                playerPrefix = "Controller Only/";
            }
            else
            {
                playerPrefix = "Player/";
            }

            if (EnableDebug) Debug.Log("[Input System] Setting up for " + playerPrefix);

            m_MoveAction    = InputSystem.actions?.FindAction($"{playerPrefix}Move");
            m_LookAction    = InputSystem.actions?.FindAction($"{playerPrefix}Look");
            m_LowActionA    = InputSystem.actions?.FindAction($"{playerPrefix}LowActionA");
            m_LowActionB    = InputSystem.actions?.FindAction($"{playerPrefix}LowActionB");
            m_HighActionA   = InputSystem.actions?.FindAction($"{playerPrefix}HighActionA");
            m_HighActionB   = InputSystem.actions?.FindAction($"{playerPrefix}HighActionB");
            m_SprintAction  = InputSystem.actions?.FindAction($"{playerPrefix}Sprint");
        }

        private void BindActions()
        {

            m_LowActionA.performed      += context => { if(IsWorkingConditionMet()) m_OnLowActionAPerformed();  };
            m_LowActionA.canceled       += context => { if(IsWorkingConditionMet()) m_OnLowActionACanceled();   };
            m_LowActionB.performed      += context => { if(IsWorkingConditionMet()) m_OnLowActionBPerformed();  };
            m_LowActionB.canceled       += context => { if(IsWorkingConditionMet()) m_OnLowActionBCanceled();   };
            m_HighActionA.performed     += context => { if(IsWorkingConditionMet()) m_OnHighActionAPerformed(); };
            m_HighActionA.canceled      += context => { if(IsWorkingConditionMet()) m_OnHighActionACanceled();  };
            m_HighActionB.performed     += context => { if(IsWorkingConditionMet()) m_OnHighActionBPerformed(); };
            m_HighActionB.canceled      += context => { if(IsWorkingConditionMet()) m_OnHighActionBCanceled();  };
            m_SprintAction.performed    += context => { if(IsWorkingConditionMet()) m_OnSprintActionPerformed();};
            m_SprintAction.canceled     += context => { if(IsWorkingConditionMet()) m_OnSprintActionCanceled(); };
            
        }
        private bool IsWorkingConditionMet()
        {
            return m_Enable;
        }

        public void Init(int t_playerIndex = 0)
        {
            SetActions(t_playerIndex);
            BindActions();
            m_Enable = true;
        }

        private Vector3 GetMovementVector() {

            var cameraRight = Camera.main.transform.right;
            var cameraForward = Camera.main.transform.forward;
            cameraRight.y = 0;
            cameraForward.y = 0;
            cameraRight.Normalize();
            cameraForward.Normalize();

            var inputValue = m_MoveAction.ReadValue<Vector2>();
            var moveDirection = cameraRight * inputValue.x + cameraForward * inputValue.y;

            return moveDirection.normalized;
        }
        


    }

}