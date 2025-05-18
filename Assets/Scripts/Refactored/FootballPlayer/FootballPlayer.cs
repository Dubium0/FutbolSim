
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;


namespace FootballSim.Player
{
    public struct TransformSnapshot {

        public double Timestamp;
        public Vector3 Position;
        public Quaternion Rotation;
    }

    [RequireComponent(typeof(Rigidbody), typeof(FootballPlayerInputSource_Offline), typeof(FootballPlayerInputSource_Online))]
    public class FootballPlayer : NetworkBehaviour
    {

        private IFootballPlayerInputSource m_InputSource;
        public IFootballPlayerInputSource InputSource { get { return m_InputSource; } }

        public Rigidbody Rigidbody { get { return GetComponent<Rigidbody>(); } }
        public Transform Transform { get { return transform; } }

        public FootballPlayerData Data { get { return m_Data; } }

        [SerializeField]
        private FootballPlayerData m_Data;


        [SerializeField]
        private Animator m_Animator;

        public Animator Animator { get { return m_Animator; } }


        private IPlayerState m_CurrentState;

        private bool m_IsHumanControlled;

        private bool m_IsInitialized = false;

        // always sycn values
        private NetworkVariable<Vector3> m_SyncedPosition = new NetworkVariable<Vector3>(
    Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        private NetworkVariable<Quaternion> m_SyncedRotation = new NetworkVariable<Quaternion>(
        Quaternion.identity, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private List<TransformSnapshot> m_SnapshotBuffer = new();

        private Vector3 m_ClientPreviousInterpolatedPosition = Vector3.zero;
        private float m_ClientInterpolatedSpeedSqr = 0.0f;

        private double m_InterpolationDelaySeconds = 0.1;


        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            m_SyncedPosition.OnValueChanged += GetNextSnapshot;

        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            m_SyncedPosition.OnValueChanged -= GetNextSnapshot;

        }
    
        public void Init(bool t_IsHummanControlled = false, bool t_IsOnlinePlayer = false, int t_PlayerIndex = 0)
        {
            if (t_IsOnlinePlayer)
            {
                m_InputSource = GetComponent<FootballPlayerInputSource_Online>();
                m_InputSource.Init(t_PlayerIndex);
                if (IsHost)
                {
                    Debug.Log("Server will send client an RPC");
                    InitRpc(t_IsHummanControlled);            
                }
            }
            else
            {
                m_InputSource = GetComponent<FootballPlayerInputSource_Offline>();
                m_InputSource.Init(t_PlayerIndex);
            }

            m_IsHumanControlled = t_IsHummanControlled;
            if (m_InputSource != null)
            {
                
                m_InputSource.OnLowActionAPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnLowActionAEnter(); };
                m_InputSource.OnLowActionACanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnLowActionAExit(); };
                m_InputSource.OnLowActionBPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnLowActionBEnter(); };
                m_InputSource.OnLowActionBCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnLowActionBExit(); };
                m_InputSource.OnHighActionAPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnHighActionAEnter(); };
                m_InputSource.OnHighActionACanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnHighActionAExit(); };
                m_InputSource.OnHighActionBPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnHighActionBEnter(); };
                m_InputSource.OnHighActionBCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnHighActionBExit(); };
                m_InputSource.OnSprintActionPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnSprintEnter(); };
                m_InputSource.OnSprintActionCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnSprintExit(); };

            }
            else
            {
                Debug.LogError("Input Source cannot be null!");
            }

            m_CurrentState = new FreeState(this);
            m_CurrentState.OnEnter();
            m_IsInitialized = true;

        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitRpc(bool t_IsHummanControlled = false)
        {
            if(!IsHost)
            Init(t_IsHummanControlled, true);
        }

        private float m_TickIntervalMs = 66.6f;
        private float m_ElapsedTime = 0.0f;

        

        private void Update()
        {
            if (!m_IsInitialized) return;
            if (IsHost)
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
            if (IsClient && !IsHost)
            {
                InterpolateClientTransform();
            }

        }

        private void FixedUpdate()
        {
              if (!m_IsInitialized) return;
            if (IsHost && m_IsHumanControlled)
            {
                m_CurrentState.OnFixedUpdate();
                m_Animator.SetFloat("Velocity", Rigidbody.linearVelocity.sqrMagnitude);
            }
            if (IsClient && !IsHost)
            {
                m_Animator.SetFloat("Velocity", m_ClientInterpolatedSpeedSqr);
            }

        }

        private void GetNextSnapshot(Vector3 t_PreviousValue, Vector3 t_NewValue)
        {

            m_SnapshotBuffer.Add(new TransformSnapshot
            {
                Timestamp = NetworkManager.Singleton.LocalTime.Time,
                Position = t_NewValue,
                Rotation = m_SyncedRotation.Value
            });
           
            if (m_SnapshotBuffer.Count > 20) // Adjust buffer size as needed
            {
                m_SnapshotBuffer.RemoveAt(0);
            }
           

        }


        private void InterpolateClientTransform()
        {
            if (m_SnapshotBuffer.Count < 2)
            {
                // Not enough data to interpolate, snap to the latest known state if available
                if (m_SnapshotBuffer.Count == 1)
                {
                    transform.position = m_SnapshotBuffer[0].Position;
                    transform.rotation = m_SnapshotBuffer[0].Rotation;
                }
                // Reset previous position to avoid large velocity spikes when data arrives
                m_ClientPreviousInterpolatedPosition = transform.position;
                m_ClientInterpolatedSpeedSqr = 0f;
                return;
            }

            // Calculate the target time for rendering
            double renderTime = NetworkManager.Singleton.LocalTime.Time - m_InterpolationDelaySeconds;

            // Find the two states in the buffer that bracket the renderTime
            TransformSnapshot fromSnapshot = default;
            TransformSnapshot toSnapshot = default;
            bool foundSnapshots = false;

            for (int i = 0; i < m_SnapshotBuffer.Count - 1; i++)
            {
                if (m_SnapshotBuffer[i].Timestamp <= renderTime && m_SnapshotBuffer[i + 1].Timestamp >= renderTime)
                {
                    fromSnapshot = m_SnapshotBuffer[i];
                    toSnapshot = m_SnapshotBuffer[i + 1];
                    foundSnapshots = true;
                    break;
                }
            }

            if (!foundSnapshots)
            {
                // Render time is outside our buffered range.
                var latestBufferedState = m_SnapshotBuffer[m_SnapshotBuffer.Count - 1];
                if (renderTime > latestBufferedState.Timestamp)
                {
                    transform.position = latestBufferedState.Position;
                    transform.rotation = latestBufferedState.Rotation;
                }
                else
                {
                    var oldestBufferedState = m_SnapshotBuffer[0];
                    transform.position = oldestBufferedState.Position;
                    transform.rotation = oldestBufferedState.Rotation;
                }
                m_ClientPreviousInterpolatedPosition = transform.position;
                m_ClientInterpolatedSpeedSqr = 0f;
                return;
            }

            // Calculate interpolation factor
            double timeDiffBetweenStates = toSnapshot.Timestamp - fromSnapshot.Timestamp;
            float interpolationFactor = 0f;
            if (timeDiffBetweenStates > 0.0001)
            {
                interpolationFactor = (float)((renderTime - fromSnapshot.Timestamp) / timeDiffBetweenStates);
            }
            interpolationFactor = Mathf.Clamp01(interpolationFactor);

            // Interpolate position and rotation
            transform.position = Vector3.Lerp(fromSnapshot.Position, toSnapshot.Position, interpolationFactor);
            transform.rotation = Quaternion.Slerp(fromSnapshot.Rotation, toSnapshot.Rotation, interpolationFactor);

            // Calculate speed for animator based on interpolated movement
            if (Time.deltaTime > 0.0001f)
            {
                float distanceThisFrame = Vector3.Distance(transform.position, m_ClientPreviousInterpolatedPosition);
                float speedThisFrame = distanceThisFrame / Time.deltaTime;
                m_ClientInterpolatedSpeedSqr = speedThisFrame * speedThisFrame;
            }
            else
            {
                m_ClientInterpolatedSpeedSqr = 0f;
            }
            m_ClientPreviousInterpolatedPosition = transform.position;
        }


    

    }   

}