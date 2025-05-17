using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FootballSim.Player
{
    public struct PlayerInputPayload : INetworkSerializable
    {
        public Vector3 MovementVector;

        public bool IsLowActionAPerformed;

        public bool IsLowActionACanceled;

        public bool IsLowActionBPerformed;

        public bool IsLowActionBCanceled;

        public bool IsHighActionAPerformed;

        public bool IsHighActionACanceled;

        public bool IsHighActionBPerformed;

        public bool IsHighActionBCanceled;

        public bool IsSprintActionPerformed;

        public bool IsSprintActionCanceled;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref MovementVector);

            serializer.SerializeValue(ref IsLowActionAPerformed);
            serializer.SerializeValue(ref IsLowActionACanceled);

            serializer.SerializeValue(ref IsLowActionBPerformed);
            serializer.SerializeValue(ref IsLowActionBCanceled);

            serializer.SerializeValue(ref IsHighActionAPerformed);
            serializer.SerializeValue(ref IsHighActionACanceled);

            serializer.SerializeValue(ref IsHighActionBPerformed);
            serializer.SerializeValue(ref IsHighActionBCanceled);

            serializer.SerializeValue(ref IsSprintActionPerformed);
            serializer.SerializeValue(ref IsSprintActionCanceled);
        }
    }
    public class FootballPlayerInputSource_Online : NetworkBehaviour, IFootballPlayerInputSource
    {
        public Vector3 MovementVector {get { return m_ServerLastProcessedInput.MovementVector; }}

        public bool IsLowActionAPerformed {get { return m_ServerLastProcessedInput.IsLowActionAPerformed; }}

        public bool IsLowActionACanceled {get { return m_ServerLastProcessedInput.IsLowActionACanceled; }}

        public bool IsLowActionBPerformed{get { return m_ServerLastProcessedInput.IsLowActionBPerformed; }}

        public bool IsLowActionBCanceled {get { return m_ServerLastProcessedInput.IsLowActionBCanceled; }}

        public bool IsHighActionAPerformed {get { return m_ServerLastProcessedInput.IsHighActionAPerformed; }}

        public bool IsHighActionACanceled {get { return m_ServerLastProcessedInput.IsHighActionACanceled; }}

        public bool IsHighActionBPerformed {get { return m_ServerLastProcessedInput.IsHighActionBPerformed; }}

        public bool IsHighActionBCanceled {get { return m_ServerLastProcessedInput.IsHighActionBCanceled; }}

        public bool IsSprintActionPerformed {get { return m_ServerLastProcessedInput.IsSprintActionPerformed; }}

        public bool IsSprintActionCanceled {get { return m_ServerLastProcessedInput.IsSprintActionCanceled; }}

        public bool Enable { get { return m_Enable; } set { m_Enable = value; } }

        private InputAction m_MoveAction;
        private InputAction m_SprintAction;
        private InputAction m_LowActionA;
        private InputAction m_LowActionB;
        private InputAction m_HighActionA;
        private InputAction m_HighActionB;
        private InputAction m_LookAction;

        private bool m_Enable = true;
        public bool EnableDebug = true;

        private PlayerInputPayload m_ServerLastProcessedInput;

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

        private bool IsWorkingConditionMet()
        {
            return m_Enable;
        }

        public void Init(int t_playerIndex = 0)
        {
            SetActions(t_playerIndex);
        }
        private void Update()
        {
            if (IsClient && IsOwner)
            {
                m_ServerLastProcessedInput = new()
                {
                    MovementVector = GetMovementVector(),

                    IsLowActionAPerformed = m_LowActionA.WasPerformedThisFrame(),

                    IsLowActionACanceled = m_LowActionA.WasReleasedThisFrame(),

                    IsLowActionBPerformed = m_LowActionB.WasPerformedThisFrame(),

                    IsLowActionBCanceled = m_LowActionB.WasReleasedThisFrame(),

                    IsHighActionAPerformed = m_HighActionA.WasPerformedThisFrame(),

                    IsHighActionACanceled = m_HighActionA.WasReleasedThisFrame(),

                    IsHighActionBPerformed = m_HighActionB.WasPerformedThisFrame(),

                    IsHighActionBCanceled = m_HighActionB.WasReleasedThisFrame(),

                    IsSprintActionPerformed = m_SprintAction.WasPerformedThisFrame(),

                    IsSprintActionCanceled = m_SprintAction.WasReleasedThisFrame()

                };
                
            }
        }

        private void FixedUpdate()
        {
            if (IsClient && IsOwner)
            {
                SubmitInputServerRpc(m_ServerLastProcessedInput);
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

            var inputValue = m_MoveAction.ReadValue<Vector2>();
            var moveDirection = cameraRight * inputValue.x + cameraForward * inputValue.y;

            return moveDirection.normalized;
        }
        [ServerRpc]
        public void SubmitInputServerRpc(PlayerInputPayload inputData, ServerRpcParams rpcParams = default)
        {
            // Basic validation: ensure the sender is the owner
            if (rpcParams.Receive.SenderClientId != OwnerClientId) return;

            // Store this input. It will be processed in FixedUpdate on the server.
            m_ServerLastProcessedInput = inputData;
            // Optionally, if m_ServerLastProcessedInput is a NetworkVariable, its value is set here.
        }



    }

}