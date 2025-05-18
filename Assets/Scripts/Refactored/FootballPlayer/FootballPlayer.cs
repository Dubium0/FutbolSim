
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace FootballSim.Player
{
    [RequireComponent(typeof(Rigidbody),typeof(FootballPlayerInputSource_Offline),typeof(FootballPlayerInputSource_Online) )]
    public class FootballPlayer : NetworkBehaviour
    {

        private IFootballPlayerInputSource m_InputSource;
        public IFootballPlayerInputSource InputSource { get { return m_InputSource; } }

        public Rigidbody Rigidbody { get { return GetComponent<Rigidbody>(); } }
        public Transform Transform { get { return transform; } }

        public FootballPlayerData Data { get { return m_Data; } }

        [SerializeField]
        private FootballPlayerData m_Data;


        private IPlayerState m_CurrentState;

        private bool m_IsHumanControlled;

        private bool m_IsInitialized = false;

        // always sycn values
        private NetworkVariable<Vector3> m_SyncedPosition = new NetworkVariable<Vector3>(
    Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<Quaternion> m_SyncedRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);



        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public void Init(bool t_IsHummanControlled = false, bool t_IsOnlinePlayer = false, int t_PlayerIndex = 0)
        {
            if (t_IsOnlinePlayer)
            {
                m_InputSource = GetComponent<FootballPlayerInputSource_Online>();
                m_InputSource.Init(t_PlayerIndex);
            }
            else
            {
                m_InputSource = GetComponent<FootballPlayerInputSource_Offline>();
                m_InputSource.Init(t_PlayerIndex);
            }

            m_IsHumanControlled = t_IsHummanControlled;
            if (m_InputSource != null)
            {

                m_InputSource.OnLowActionAPerformed = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnLowActionAEnter(); };
                m_InputSource.OnLowActionACanceled = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnLowActionAExit(); };
                m_InputSource.OnLowActionBPerformed = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnLowActionBEnter(); };
                m_InputSource.OnLowActionBCanceled = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnLowActionBExit(); };
                m_InputSource.OnHighActionAPerformed = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnHighActionAEnter(); };
                m_InputSource.OnHighActionACanceled = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnHighActionAExit(); };
                m_InputSource.OnHighActionBPerformed = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnHighActionBEnter(); };
                m_InputSource.OnHighActionBCanceled = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnHighActionBExit(); };
                m_InputSource.OnSprintActionPerformed = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnSprintEnter(); };
                m_InputSource.OnSprintActionCanceled = () => { if (IsServer && m_IsHumanControlled) m_CurrentState.OnSprintExit(); };
            }
            else
            {
                Debug.LogError("Input Source cannot be null!");
            }

            m_CurrentState = new FreeState(this);
            m_CurrentState.OnEnter();
            m_IsInitialized = true;

        }

        private float m_TickIntervalMs = 66.6f;
        private float m_ElapsedTime = 0.0f;
        private void Update()
        {
            if (!m_IsInitialized) return;
            if (IsServer)
            {
                if (m_IsHumanControlled)
                {
                    m_CurrentState.OnUpdate();
                }
                m_ElapsedTime += Time.deltaTime * 1000;
                if (m_ElapsedTime >= m_TickIntervalMs)
                {
                    m_SyncedPosition.Value = transform.position;
                    m_SyncedRotation.Value = transform.rotation;
                    m_ElapsedTime = 0.0f;
                }
            }
            if (IsClient)
            {
                transform.position = m_SyncedPosition.Value;
                transform.rotation = m_SyncedRotation.Value;
            }

        }

        private void FixedUpdate()
        {
            if (!m_IsInitialized) return;
            if (IsServer & m_IsHumanControlled)
            {
                m_CurrentState.OnFixedUpdate();
            }
        }

    

    }   

}