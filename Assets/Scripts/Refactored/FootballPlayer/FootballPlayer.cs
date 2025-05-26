
using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FootballSim.FootballTeam;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;


namespace FootballSim.Player
{
    public struct TransformSnapshot {

        public double Timestamp;
        public Vector3 Position;
        public Quaternion Rotation;
    }
    public enum PlayerType
    {
        GoalKeeper,
        Defender,
        Midfielder,
        Forward
    }

    [RequireComponent(typeof(Rigidbody), typeof(FootballPlayerInputSource_Offline), typeof(FootballPlayerInputSource_Online))]
    public class FootballPlayer : NetworkBehaviour
    {
        public const int HomeLayerMask = 9;
        public const int AwayLayerMask = 10;
        private IFootballPlayerInputSource m_InputSource;
        public IFootballPlayerInputSource InputSource { get { return m_InputSource; } }

        public Rigidbody Rigidbody { get { return GetComponent<Rigidbody>(); } }
        public Transform Transform { get { return transform; } }

        public FootballPlayerData Data { get { return m_Data; } }

        [SerializeField]
        private FootballPlayerData m_Data;


        [SerializeField]
        private Animator m_Animator;

        [SerializeField]
        private FootballPlayerAnimation m_FootballPlayerAnimation;

        public FootballPlayerAnimation FootballPlayerAnimation { get { return m_FootballPlayerAnimation; } }
        public Animator Animator
        { get { return m_Animator; } }

        public FootballTeam.TeamFlag TeamFlag { get; private set; } = FootballTeam.TeamFlag.NotInitialized;

        public event Action<FootballPlayer> OnBallWinCallback;

        public event Action<FootballPlayer> OnBallLoseCallback;

        private IPlayerState m_CurrentState;

        private bool m_IsHumanControlled;
        public bool IsHumanControlled { get { return m_IsHumanControlled; } }

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
        [SerializeField]
        private Transform m_BallPositionTransform;
        [SerializeField]
        private GameObject m_Indicator;

        [SerializeField]
        private Renderer m_Renderer;

        public Vector3 BallPosition { get { return m_BallPositionTransform.position; } }


        public bool IsTheOwnerOfTheBall { get; private set; } = false;

        public bool CanPossesTheBall { get; private set; } = true;

        public bool IsForcefullyTakingBall { get; private set; } = false;

        public bool IsMovementLocked { get; private set; } = false;

        public FootballTeam.FootballTeam OwnerTeam { get; private set; }

        public PlayerType PlayerType { get; private set; }
        public bool IsAITicking { get; private set; } = true;


        public Transform CurrentHomePosition { get; private set; }



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

        public void Init(FootballTeam.FootballTeam t_OwnerTeam, FootballTeam.TeamFlag t_TeamFlag, PlayerType t_PlayerType, bool t_IsHummanControlled = false, bool t_IsOnlinePlayer = false, int t_PlayerIndex = 0)
        {

            if (t_IsOnlinePlayer)
            {
                m_InputSource = GetComponent<FootballPlayerInputSource_Online>();
                m_InputSource.Init(t_PlayerIndex);
                if (IsHost)
                {
                    Debug.Log("Server will send client an RPC");
                    InitRpc(t_TeamFlag, t_PlayerType, t_IsHummanControlled);
                }
            }
            else
            {
                m_InputSource = GetComponent<FootballPlayerInputSource_Offline>();
                m_InputSource.Init(t_PlayerIndex);
            }
            TeamFlag = t_TeamFlag;
            OwnerTeam = t_OwnerTeam;
            PlayerType = t_PlayerType;
            SetTeamColor(TeamFlag);
            if (TeamFlag == FootballTeam.TeamFlag.Home)
            {
                gameObject.layer = 9;
            }
            else if (TeamFlag == FootballTeam.TeamFlag.Away)
            {
                gameObject.layer = 10;
            }

            m_IsHumanControlled = t_IsHummanControlled;
            if (m_InputSource != null)
            {

                m_InputSource.OnPassActionPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnPassActionEnter(); };
                m_InputSource.OnPassActionCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnPassActionExit(); };
                m_InputSource.OnThroughPassActionPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnThroughPassActionEnter(); };
                m_InputSource.OnThroughPassActionCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnThroughPassActionExit(); };
                m_InputSource.OnLobPassActionPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnLobPassActionEnter(); };
                m_InputSource.OnLobPassActionCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnLobPassActionExit(); };
                m_InputSource.OnShootActionPerformed = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnShootActionEnter(); };
                m_InputSource.OnShootActionCanceled = () => { if (IsHost && m_IsHumanControlled) m_CurrentState.OnShootActionExit(); };
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

            OnBallLoseCallback += player =>
            {
                IsTheOwnerOfTheBall = false;

                StartCoroutine(StartBallPossesCooldown());


            };
            OnBallWinCallback += player =>
            {
                Debug.Log("OnBallWinCallback");
                IsTheOwnerOfTheBall = true;

                StartCoroutine(StartForcefullyTakingBallCooldown());


            };



            Football.Football.Instance.OnBallOwnerChanged += (t_PrevOwner, t_NewOwner) =>
            {
                if (IsHost)
                {
                    if (t_PrevOwner == this)
                    {
                        if (OnBallLoseCallback != null)
                        {
                            OnBallLoseCallback.Invoke(this);

                        }
                    }
                    else if (t_NewOwner == this)
                    {
                        if (OnBallWinCallback != null)
                        {

                            OnBallWinCallback.Invoke(this);
                        }
                    }

                }
            };

        }

        private IEnumerator StartBallPossesCooldown()
        {
            CanPossesTheBall = false;
            yield return new WaitForSeconds(1); // 1 second cooldown
            CanPossesTheBall = true;
        }

        private IEnumerator StartForcefullyTakingBallCooldown()
        {
            IsForcefullyTakingBall = true;
            yield return new WaitForSeconds(0.5f); // 1 second cooldown
            IsForcefullyTakingBall = false;
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitRpc(FootballTeam.TeamFlag t_TeamFlag, PlayerType t_PlayerType, bool t_IsHummanControlled = false)
        {
            if (!IsHost)
                Init(null, t_TeamFlag, t_PlayerType, t_IsHummanControlled, true);
        }

        private float m_TickIntervalMs = 66.6f;
        private float m_ElapsedTime = 0.0f;

        private void SetTeamColor(FootballTeam.TeamFlag t_TeamFlag)
        {
            switch (t_TeamFlag)
            {
                case FootballTeam.TeamFlag.Home:
                    m_Renderer.materials[4].color = Color.red;
                    break;
                case FootballTeam.TeamFlag.Away:
                    m_Renderer.materials[4].color = Color.blue;
                    break;
                default:
                    break;
            }
        }

        private void Update()
        {
            if (!m_IsInitialized) return;
            if (IsHost)
            {
                if (m_IsHumanControlled)
                {
                    if (!IsMovementLocked)
                    {

                        m_CurrentState.OnUpdate();
                    }
                    m_CurrentState.OnNotMovementRelatedUpdate();
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
            if (IsHost)
            {
                if (m_IsHumanControlled && !IsMovementLocked)
                {
                    m_CurrentState.OnFixedUpdate();
                }
                m_Animator.SetFloat("Velocity", Rigidbody.linearVelocity.sqrMagnitude);
            }
            if (IsClient && !IsHost)
            {
                m_Animator.SetFloat("Velocity", m_ClientInterpolatedSpeedSqr);
            }

        }

        public void LockPlayerMovement(bool t_Value)
        {
            IsMovementLocked = t_Value;
        }


        public void EnableIndicator(bool t_Enable)
        {
            if (t_Enable)
            {
                m_Indicator.SetActive(true);
            }
            else
            {
                m_Indicator.SetActive(false);
            }
        }
        [Rpc(SendTo.ClientsAndHost)]
        public void EnableIndicatorRpc(bool t_Enable)
        {
            if (IsHost) return;
            EnableIndicator(t_Enable);
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

        public void TransitionTo(IPlayerState t_NextState)
        {
            if (m_CurrentState != null)
            {
                m_CurrentState.OnExit();
            }
            m_CurrentState = t_NextState;
            m_CurrentState.OnEnter();
        }

        public void SetHumanControlled(bool t_IsHumanControlled)
        {
            if (IsHost && m_IsHumanControlled != t_IsHumanControlled)
            {
                EnableIndicator(t_IsHumanControlled);
                EnableIndicatorRpc(t_IsHumanControlled);
            }

            m_IsHumanControlled = t_IsHumanControlled;
        }
        public void SetKickBallTrigger()
        {
            if (IsHost)
            {
                Animator.SetTrigger("BallHit");
                SetKickBallTriggerRpc();
            }

        }
        [Rpc(SendTo.ClientsAndHost)]
        public void SetKickBallTriggerRpc()
        {
            if (!IsHost)
                Animator.SetTrigger("BallHit");
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


        public void SetHomePosition(Transform t_Target)
        {
            CurrentHomePosition = t_Target;
        }
        public void ImmidiatelyMoveToHomePosition()
        {
            transform.position = CurrentHomePosition.position;
            transform.rotation = CurrentHomePosition.rotation;
        }

        public void SmoothlyGoToHomePosition()
        {

        }

        
    }   

}