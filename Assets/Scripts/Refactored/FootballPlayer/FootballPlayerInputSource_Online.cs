using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

namespace FootballSim.Player
{

    public class FootballPlayerInputSource_Online : NetworkBehaviour, IFootballPlayerInputSource
    {
        public Vector3 MovementVector { get { if (IsWorkingConditionMet()) return GetMovementVector(); else return Vector3.zero; } }

        public Action OnLowActionAPerformed { set { m_OnLowActionAPerformed = value; } }
        public Action OnLowActionACanceled { set { m_OnLowActionACanceled = value; } }
        public Action OnLowActionBPerformed { set { m_OnLowActionBPerformed = value; } }
        public Action OnLowActionBCanceled { set { m_OnLowActionBCanceled = value; } }
        public Action OnHighActionAPerformed { set { m_OnHighActionAPerformed = value; } }
        public Action OnHighActionACanceled { set { m_OnHighActionACanceled = value; } }
        public Action OnHighActionBPerformed { set { m_OnHighActionBPerformed = value; } }
        public Action OnHighActionBCanceled { set { m_OnHighActionBCanceled = value; } }
        public Action OnSprintActionPerformed { set { m_OnSprintActionPerformed = value; } }
        public Action OnSprintActionCanceled { set { m_OnSprintActionCanceled = value; } }

        public bool Enable { get { return m_Enable; } set { m_Enable = value; } }
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

        private Vector2 m_ClientMovementInput = Vector2.zero;
        private void SetActions(int t_playerIndex = 0)
        {
            string playerPrefix = t_playerIndex >= 0 ? $"Player{t_playerIndex + 1}/" : "";

            if (EnableDebug) Debug.Log("[Input System] Setting up for " + playerPrefix);

            m_MoveAction = InputSystem.actions?.FindAction($"{playerPrefix}Move");
            m_LookAction = InputSystem.actions?.FindAction($"{playerPrefix}Look");
            m_LowActionA = InputSystem.actions?.FindAction($"{playerPrefix}LowActionA");
            m_LowActionB = InputSystem.actions?.FindAction($"{playerPrefix}LowActionB");
            m_HighActionA = InputSystem.actions?.FindAction($"{playerPrefix}HighActionA");
            m_HighActionB = InputSystem.actions?.FindAction($"{playerPrefix}HighActionB");
            m_SprintAction = InputSystem.actions?.FindAction($"{playerPrefix}Sprint");
        }


        private float m_TickIntervalMs= 66.6f; // 15 times per second means

        private float m_ElapsedTime = 0.0f;
        private void Update()
        {
            if (!m_Enable) return;
            if (IsClient & IsOwner)
            {
                m_ElapsedTime += Time.deltaTime * 1000;
                if (m_ElapsedTime >= m_TickIntervalMs)
                {
                    UpdateMovementInputRpc(m_MoveAction.ReadValue<Vector2>());
                    m_ElapsedTime = 0;
                }

            }
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
    
        private void BindActions()
        {
            if (IsClient & IsOwner)
            {
                m_LowActionA.performed += context => { if (IsWorkingConditionMet()) RequestActionLowAPerformedFromServerRpc(); };
                m_LowActionA.canceled += context => { if (IsWorkingConditionMet()) RequestActionLowACanceledFromServerRpc(); };
                m_LowActionB.performed += context => { if (IsWorkingConditionMet()) RequestActionLowBPerformedFromServerRpc(); };
                m_LowActionB.canceled += context => { if (IsWorkingConditionMet()) RequestActionLowBCanceledFromServerRpc(); };
                m_HighActionA.performed += context => { if (IsWorkingConditionMet()) RequestActionHighAPerformedFromServerRpc(); };
                m_HighActionA.canceled += context => { if (IsWorkingConditionMet()) RequestActionHighACanceledFromServerRpc(); };
                m_HighActionB.performed += context => { if (IsWorkingConditionMet()) RequestActionHighBPerformedFromServerRpc(); };
                m_HighActionB.canceled += context => { if (IsWorkingConditionMet()) RequestActionHighBCanceledFromServerRpc(); };
                m_SprintAction.performed += context => { if (IsWorkingConditionMet()) RequestActionSprintPerformedFromServerRpc(); };
                m_SprintAction.canceled += context => { if (IsWorkingConditionMet()) RequestActionSprintCanceledFromServerRpc(); };
            }
            if (IsServer & IsOwner)
            {
                m_LowActionA.performed += context => { if (IsWorkingConditionMet()) m_OnLowActionAPerformed(); };
                m_LowActionA.canceled += context => { if (IsWorkingConditionMet()) m_OnLowActionACanceled(); };
                m_LowActionB.performed += context => { if (IsWorkingConditionMet()) m_OnLowActionBPerformed(); };
                m_LowActionB.canceled += context => { if (IsWorkingConditionMet()) m_OnLowActionBCanceled(); };
                m_HighActionA.performed += context => { if (IsWorkingConditionMet()) m_OnHighActionAPerformed(); };
                m_HighActionA.canceled += context => { if (IsWorkingConditionMet()) m_OnHighActionACanceled(); };
                m_HighActionB.performed += context => { if (IsWorkingConditionMet()) m_OnHighActionBPerformed(); };
                m_HighActionB.canceled += context => { if (IsWorkingConditionMet()) m_OnHighActionBCanceled(); };
                m_SprintAction.performed += context => { if (IsWorkingConditionMet()) m_OnSprintActionPerformed(); };
                m_SprintAction.canceled += context => { if (IsWorkingConditionMet()) m_OnSprintActionCanceled(); };
            }
        }

        private Vector3 GetMovementVector()
        {

            var cameraRight = Camera.main.transform.right;
            var cameraForward = Camera.main.transform.forward;
            cameraRight.y = 0;
            cameraForward.y = 0;
            cameraRight.Normalize();
            cameraForward.Normalize();

            var inputValue = Vector2.zero;

            if (IsServer)
            {
                if (IsOwner)
                {
                    inputValue = m_MoveAction.ReadValue<Vector2>();
                }
                else
                {
                    inputValue = m_ClientMovementInput;
                }

            }

            var moveDirection = cameraRight * inputValue.x + cameraForward * inputValue.y;

            return moveDirection.normalized;
        }

        [Rpc(SendTo.Server)]
        private void RequestActionLowAPerformedFromServerRpc() { if ( IsWorkingConditionMet()) m_OnLowActionAPerformed(); if (EnableDebug) Debug.Log("Sending LowA performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionLowACanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnLowActionACanceled(); if (EnableDebug) Debug.Log("Sending LowA canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionLowBPerformedFromServerRpc() { if (IsWorkingConditionMet()) m_OnLowActionBPerformed(); if (EnableDebug) Debug.Log("Sending LowB performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionLowBCanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnLowActionBCanceled(); if (EnableDebug) Debug.Log("Sending LowB canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionHighAPerformedFromServerRpc() { if (IsWorkingConditionMet()) m_OnHighActionAPerformed(); if (EnableDebug) Debug.Log("Sending HighA performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionHighACanceledFromServerRpc() { if (IsWorkingConditionMet()) m_OnHighActionACanceled(); if (EnableDebug) Debug.Log("Sending HighA canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionHighBPerformedFromServerRpc() { if (IsWorkingConditionMet()) m_OnHighActionBPerformed(); if (EnableDebug) Debug.Log("Sending HighB performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionHighBCanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnHighActionBCanceled(); if (EnableDebug) Debug.Log("Sending HighB canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionSprintPerformedFromServerRpc() { if ( IsWorkingConditionMet()) m_OnSprintActionPerformed(); if (EnableDebug) Debug.Log("Sending sprint performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestActionSprintCanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnSprintActionCanceled(); if (EnableDebug) Debug.Log("Sending sprint cancaled to Server"); }

        [Rpc(SendTo.Server)]
        private void UpdateMovementInputRpc(Vector2 t_MovementInput)
        {
            
            m_ClientMovementInput = t_MovementInput;
            
        }

       
    }

}