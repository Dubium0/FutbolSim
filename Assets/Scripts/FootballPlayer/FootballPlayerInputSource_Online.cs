using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FootballSim.Player
{

    public class FootballPlayerInputSource_Online : NetworkBehaviour, IFootballPlayerInputSource
    {
        public Vector3 MovementVector { get { if (IsWorkingConditionMet()) return GetMovementVector(); else return Vector3.zero; } }

        public Action OnPassActionPerformed { set { m_OnPassActionPerformed = value; } }
        public Action OnPassActionCanceled { set { m_OnPassActionCanceled = value; } }
        public Action OnThroughPassActionPerformed { set { m_OnThroughPassActionPerformed = value; } }
        public Action OnThroughPassActionCanceled { set { m_OnThroughPassActionCanceled = value; } }
        public Action OnLobPassActionPerformed { set { m_OnLobPassActionPerformed = value; } }
        public Action OnLobPassActionCanceled { set { m_OnLobPassActionCanceled = value; } }
        public Action OnShootActionPerformed { set { m_OnShootActionPerformed = value; } }
        public Action OnShootActionCanceled { set { m_OnShootActionCanceled = value; } }
        public Action OnSprintActionPerformed { set { m_OnSprintActionPerformed = value; } }
        public Action OnSprintActionCanceled { set { m_OnSprintActionCanceled = value; } }

        public bool Enable { get { return m_Enable; } set { m_Enable = value; } }
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
        private InputAction m_LookAction;

        private bool m_Enable = false;
        public bool EnableDebug = true;

        private Vector2 m_ClientMovementInput = Vector2.zero;
        private void SetActions(int t_playerIndex = 0)
        {
            string playerPrefix;
            if (t_playerIndex  == 1)
            {
                playerPrefix = "Keyboard Only/";
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

            m_MoveAction = InputSystem.actions?.FindAction($"{playerPrefix}Move");
            m_LookAction = InputSystem.actions?.FindAction($"{playerPrefix}Look");
            m_PassAction = InputSystem.actions?.FindAction($"{playerPrefix}Pass");
            m_ThroughPassAction = InputSystem.actions?.FindAction($"{playerPrefix}ThroughPass");
            m_LobPassAction = InputSystem.actions?.FindAction($"{playerPrefix}LobPass");
            m_ShootAction = InputSystem.actions?.FindAction($"{playerPrefix}Shoot");
            m_SprintAction = InputSystem.actions?.FindAction($"{playerPrefix}Sprint");
        }


        private float m_TickIntervalMs= 66.6f; // 15 times per second means

        private float m_ElapsedTime = 0.0f;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            
        }
        private void Update()
        {
            if (!m_Enable) return;
            if (IsClient && IsOwner && !IsHost)
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
            if (IsClient && IsOwner && !IsHost)
            {
                m_PassAction.performed += context => { if (IsWorkingConditionMet()) RequestPassActionPerformedFromServerRpc(); };
                m_PassAction.canceled += context => { if (IsWorkingConditionMet()) RequestPassActionCanceledFromServerRpc(); };
                m_ThroughPassAction.performed += context => { if (IsWorkingConditionMet()) RequestThroughPassActionPerformedFromServerRpc(); };
                m_ThroughPassAction.canceled += context => { if (IsWorkingConditionMet()) RequestThroughPassActionCanceledFromServerRpc(); };
                m_LobPassAction.performed += context => { if (IsWorkingConditionMet()) RequestLobPassActionPerformedFromServerRpc(); };
                m_LobPassAction.canceled += context => { if (IsWorkingConditionMet()) RequestLobPassActionCanceledFromServerRpc(); };
                m_ShootAction.performed += context => { if (IsWorkingConditionMet()) RequestShootActionBPerformedFromServerRpc(); };
                m_ShootAction.canceled += context => { if (IsWorkingConditionMet()) RequestShootActionBCanceledFromServerRpc(); };
                m_SprintAction.performed += context => { if (IsWorkingConditionMet()) RequestActionSprintPerformedFromServerRpc(); };
                m_SprintAction.canceled += context => { if (IsWorkingConditionMet()) RequestActionSprintCanceledFromServerRpc(); };
            }
            if (IsHost && IsOwner)
            {
                m_PassAction.performed += context => { if (IsWorkingConditionMet()) m_OnPassActionPerformed(); };
                m_PassAction.canceled += context => { if (IsWorkingConditionMet()) m_OnPassActionCanceled(); };
                m_ThroughPassAction.performed += context => { if (IsWorkingConditionMet()) m_OnThroughPassActionPerformed(); };
                m_ThroughPassAction.canceled += context => { if (IsWorkingConditionMet()) m_OnThroughPassActionCanceled(); };
                m_LobPassAction.performed += context => { if (IsWorkingConditionMet()) m_OnLobPassActionPerformed(); };
                m_LobPassAction.canceled += context => { if (IsWorkingConditionMet()) m_OnLobPassActionCanceled(); };
                m_ShootAction.performed += context => { if (IsWorkingConditionMet()) m_OnShootActionPerformed(); };
                m_ShootAction.canceled += context => { if (IsWorkingConditionMet()) m_OnShootActionCanceled(); };
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

            if (IsHost)
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
        private void RequestPassActionPerformedFromServerRpc() { if ( IsWorkingConditionMet()) m_OnPassActionPerformed(); if (EnableDebug) Debug.Log("Sending LowA performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestPassActionCanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnPassActionCanceled(); if (EnableDebug) Debug.Log("Sending LowA canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestThroughPassActionPerformedFromServerRpc() { if (IsWorkingConditionMet()) m_OnThroughPassActionPerformed(); if (EnableDebug) Debug.Log("Sending LowB performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestThroughPassActionCanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnThroughPassActionCanceled(); if (EnableDebug) Debug.Log("Sending LowB canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestLobPassActionPerformedFromServerRpc() { if (IsWorkingConditionMet()) m_OnLobPassActionPerformed(); if (EnableDebug) Debug.Log("Sending HighA performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestLobPassActionCanceledFromServerRpc() { if (IsWorkingConditionMet()) m_OnLobPassActionCanceled(); if (EnableDebug) Debug.Log("Sending HighA canceled to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestShootActionBPerformedFromServerRpc() { if (IsWorkingConditionMet()) m_OnShootActionPerformed(); if (EnableDebug) Debug.Log("Sending HighB performed to Server"); }

        [Rpc(SendTo.Server)]
        private void RequestShootActionBCanceledFromServerRpc() { if ( IsWorkingConditionMet()) m_OnShootActionCanceled(); if (EnableDebug) Debug.Log("Sending HighB canceled to Server"); }

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