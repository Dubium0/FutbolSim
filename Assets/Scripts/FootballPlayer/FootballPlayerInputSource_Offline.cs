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

        public Action OnPassActionPerformed     { set { m_OnPassActionPerformed = value;} }
        public Action OnPassActionCanceled      { set { m_OnPassActionCanceled = value;} }
        public Action OnThroughPassActionPerformed     { set { m_OnThroughPassActionPerformed = value;} }
        public Action OnThroughPassActionCanceled      { set { m_OnThroughPassActionCanceled = value;} }
        public Action OnLobPassActionPerformed    { set { m_OnLobPassActionPerformed = value;} }
        public Action OnLobPassActionCanceled     { set { m_OnLobPassActionCanceled = value;} }
        public Action OnShootActionPerformed    { set { m_OnShootActionPerformed = value;} }
        public Action OnShootActionCanceled     { set { m_OnShootActionCanceled = value;} }
        public Action OnSprintActionPerformed   { set { m_OnSprintActionPerformed = value;} }
        public Action OnSprintActionCanceled    { set { m_OnSprintActionCanceled = value;} }

        private Action m_OnPassActionPerformed;
        private Action m_OnPassActionCanceled;
        private Action m_OnThroughPassActionPerformed;
        private Action m_OnThroughPassActionCanceled;
        private Action m_OnLobPassActionPerformed;
        private Action m_OnLobPassActionCanceled;
        private Action m_OnShootActionPerformed;
        private Action m_OnShootActionCanceled;
        private Action m_OnSprintActionPerformed;
        private Action m_OnSprintActionCanceled;





        private InputAction m_MoveAction;
        private InputAction m_SprintAction;
        private InputAction m_PassAction;
        private InputAction m_ThroughPassAction;
        private InputAction m_LobPassAction;
        private InputAction m_ShootAction;

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
            m_PassAction    = InputSystem.actions?.FindAction($"{playerPrefix}Pass");
            m_ThroughPassAction    = InputSystem.actions?.FindAction($"{playerPrefix}ThroughPass");
            m_LobPassAction   = InputSystem.actions?.FindAction($"{playerPrefix}LobPass");
            m_ShootAction   = InputSystem.actions?.FindAction($"{playerPrefix}Shoot");
            m_SprintAction  = InputSystem.actions?.FindAction($"{playerPrefix}Sprint");
        }

        private void BindActions()
        {

            m_PassAction.performed      += context => { if(IsWorkingConditionMet()) m_OnPassActionPerformed();  };
            m_PassAction.canceled       += context => { if(IsWorkingConditionMet()) m_OnPassActionCanceled();   };
            m_ThroughPassAction.performed      += context => { if(IsWorkingConditionMet()) m_OnThroughPassActionPerformed();  };
            m_ThroughPassAction.canceled       += context => { if(IsWorkingConditionMet()) m_OnThroughPassActionCanceled();   };
            m_LobPassAction.performed     += context => { if(IsWorkingConditionMet()) m_OnLobPassActionPerformed(); };
            m_LobPassAction.canceled      += context => { if(IsWorkingConditionMet()) m_OnLobPassActionCanceled();  };
            m_ShootAction.performed     += context => { if(IsWorkingConditionMet()) m_OnShootActionPerformed(); };
            m_ShootAction.canceled      += context => { if(IsWorkingConditionMet()) m_OnShootActionCanceled();  };
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