using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;


namespace FootballSim.Player
{

    public class FootballPlayerInputSource_Offline : MonoBehaviour, IFootballPlayerInputSource
    {

        public Vector3 MovementVector { get { return GetMovementVector(); } }

        public bool IsLowActionAPerformed { get { if (IsWorkingConditionMet()) return m_LowActionA.WasPerformedThisFrame(); else { return false; } } }

        public bool IsLowActionACanceled { get { if (IsWorkingConditionMet()) return m_LowActionA.WasReleasedThisFrame(); else { return false; } } }

        public bool IsLowActionBPerformed { get { if (IsWorkingConditionMet()) return m_LowActionB.WasPerformedThisFrame(); else { return false; } } }

        public bool IsLowActionBCanceled { get { if (IsWorkingConditionMet()) return m_LowActionB.WasReleasedThisFrame(); else { return false; } } }

        public bool IsHighActionAPerformed { get { if (IsWorkingConditionMet()) return m_HighActionA.WasPerformedThisFrame(); else { return false; } } }

        public bool IsHighActionACanceled { get { if (IsWorkingConditionMet()) return m_HighActionA.WasReleasedThisFrame(); else { return false; } } }

        public bool IsHighActionBPerformed { get { if (IsWorkingConditionMet()) return m_HighActionB.WasPerformedThisFrame(); else { return false; } } }

        public bool IsHighActionBCanceled { get { if (IsWorkingConditionMet()) return m_HighActionB.WasReleasedThisFrame(); else { return false; } } }

        public bool IsSprintActionPerformed { get { if (IsWorkingConditionMet()) return m_SprintAction.WasPerformedThisFrame(); else { return false; } } }

        public bool IsSprintActionCanceled { get { if (IsWorkingConditionMet()) return m_SprintAction.WasReleasedThisFrame(); else { return false; } } }

        private InputAction m_MoveAction;
        private InputAction m_SprintAction;
        private InputAction m_LowActionA;
        private InputAction m_LowActionB;
        private InputAction m_HighActionA;
        private InputAction m_HighActionB;
        private InputAction m_LookAction;

        private bool m_Enable = true;
        public bool EnableDebug = true;
        public bool Enable { get { return m_Enable; } set { m_Enable = value; } }


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