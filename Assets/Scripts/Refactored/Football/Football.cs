
using System;
using System.Linq;
using FootballSim.Player;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;

namespace FootballSim.Football
{
    [RequireComponent(typeof(Rigidbody),typeof(Collider))]
    public class Football : NetworkBehaviour
    {

        public static Football Instance { get; private set; }
        [SerializeField]
        private float m_PlayerCheckRadius = 2.0f;

        [SerializeField]
        private LayerMask m_GroundCheckLayer;

        [SerializeField]
        private LayerMask m_PlayerCheckLayer;

        [SerializeField]
        private bool m_EnableDebug = true;

        [SerializeField]
        private Transform m_StartTransform;
        public Rigidbody Rigidbody { get { return GetComponent<Rigidbody>(); } }
        
        public FootballPlayer CurrentOwnerPlayer { get; private set; }

        public FootballTeam.TeamFlag LastTouchedTeam { get; private set; } = FootballTeam.TeamFlag.NotInitialized;

        public event Action<FootballPlayer, FootballPlayer> OnBallOwnerChanged;

        public event Action<FootballPlayer,Vector3> OnBallHit;
        public event Action<FootballTeam.TeamFlag,FootballPlayer> OnGoal;

        public event Action<FootballTeam.TeamFlag> OnOut;

        public event Action<FootballTeam.TeamFlag> OnGoalKeeperPosses;
        public FootballPlayer LastHitPlayer { get; private set; }
        public int CurrentSector { get; private set; } = -1;
        public bool IsGrounded
        {
            get
            {
                return Physics.Raycast(Rigidbody.position, Vector3.down, 0.1f, m_GroundCheckLayer);
            }
        }

        public bool IsInteractable { get; private set; } = true;
    

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                OnBallOwnerChanged += HandleGoalKeeperOwn;

            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void HandleGoalKeeperOwn(FootballPlayer t_OldOwner, FootballPlayer t_NewOwner)
        {
            if (t_NewOwner != null && t_NewOwner.PlayerType == Player.PlayerType.GoalKeeper)
            {
                if (OnGoalKeeperPosses != null)
                {
                    OnGoalKeeperPosses.Invoke(t_NewOwner.TeamFlag);
                }
            }
        }

        private void FixedUpdate()
        {
            if (IsHost)
            {
              
               
            }
        }
        private void Update()
        {
            if (IsHost && IsInteractable)
            {
                CheckPlayerCollision();
                AdjustPosition();
            }
        }

        public void HitBall(Vector3 t_ForceVector, float t_ShootPower, FootballPlayer t_Player)
        {
            if (!IsInteractable) return;

            Rigidbody.MovePosition(t_Player.BallPosition);

            if (t_Player != CurrentOwnerPlayer)
            {
                Debug.Log("Only owner can hit the ball!");
                return;
            }
            Rigidbody.AddForce(t_ForceVector * t_ShootPower, ForceMode.VelocityChange);
            CurrentOwnerPlayer = null;
            if (OnBallOwnerChanged != null)
            {
                OnBallOwnerChanged(t_Player, CurrentOwnerPlayer);
            }
            if (OnBallHit != null)
            {
                OnBallHit(t_Player,t_ForceVector * t_ShootPower);
                LastHitPlayer = t_Player;
            }
                
        }

        public void SetInteractable(bool t_Value)
        {
            IsInteractable = t_Value;
            var prevPlayer = CurrentOwnerPlayer;
            CurrentOwnerPlayer = null;
            if (OnBallOwnerChanged != null)
            {
                OnBallOwnerChanged(prevPlayer, CurrentOwnerPlayer);
            }
        }

        private void CheckPlayerCollision()
        {

            /*
                if currentOwner == null:
                give to ball to the highest dice thrower., put others into ghost layer for a second.

            
            */


            var colliders = Physics.OverlapSphere(transform.position, m_PlayerCheckRadius, m_PlayerCheckLayer);

            FootballPlayer playerToAssign = CurrentOwnerPlayer;



            foreach (var collider in colliders)
            {

                if (collider.TryGetComponent<FootballPlayer>(out var otherPlayer))
                {
                    if (CurrentOwnerPlayer == otherPlayer)
                    {
                        continue;
                    }
                    if (playerToAssign != null && playerToAssign.TeamFlag != otherPlayer.TeamFlag)
                    {
                        var currentPlayerScore = UnityEngine.Random.Range(1, 7);
                        var otherPlayerScore = UnityEngine.Random.Range(1, 7);

                        var finalCondition = currentPlayerScore < otherPlayerScore && otherPlayer.CanPossesTheBall;

                        playerToAssign = finalCondition ? otherPlayer : playerToAssign;
                    }
                    else
                    {
                        if (otherPlayer.CanPossesTheBall)
                            playerToAssign = otherPlayer;
                    }
                }
            }

            var prevOwner = CurrentOwnerPlayer;

            CurrentOwnerPlayer = playerToAssign;
            if (CurrentOwnerPlayer != prevOwner)
            {
                LastTouchedTeam = CurrentOwnerPlayer.TeamFlag;
                if (OnBallOwnerChanged != null)
                {
                    OnBallOwnerChanged(prevOwner, CurrentOwnerPlayer);
                }


            }
        }
        public void ResetToStartTransform()
        {
            transform.position = m_StartTransform.position;
            transform.rotation = m_StartTransform.rotation;
            
        }
        private void OnTriggerEnter(Collider other)
        {
            if (!IsHost) return;
            if (other.CompareTag("GoalAway"))
            {
                if (OnGoal != null)
                {
                    OnGoal.Invoke(FootballTeam.TeamFlag.Home, LastHitPlayer);
                }
            }
            else if (other.CompareTag("GoalHome"))
            {

                if (OnGoal != null)
                {
                    OnGoal.Invoke(FootballTeam.TeamFlag.Away, LastHitPlayer);
                }
            }

            if (other.CompareTag("Sector"))
            {
                if (int.TryParse(other.gameObject.name, out int sectorNumber))
                {
                    if (CurrentSector != sectorNumber)
                    {
                        CurrentSector = sectorNumber;
                    }
                }
            }
            if (other.CompareTag("HomeOutZone"))
            {
                if (OnOut != null)
                {
                    OnOut.Invoke(FootballTeam.TeamFlag.Home);
                }
            }
            else if (other.CompareTag("AwayOutZone"))
            {
                if (OnOut != null)
                {
                    OnOut.Invoke(FootballTeam.TeamFlag.Away);
                }
            }
        }

        private void AdjustPosition()
        {
            if (CurrentOwnerPlayer != null && IsInteractable )  
            {
                var targetPosition = CurrentOwnerPlayer.BallPosition;
                transform.position = targetPosition;
            }

        }

       
        public Vector3 GetDropPointAfterTSeconds(float time)
        {
            Vector3 currentPosition = Rigidbody.position;
            Vector3 currentVelocity = Rigidbody.linearVelocity;

            Vector3 acceleration = Physics.gravity;

            float deltaTime = time / 20.0f;
            if (deltaTime < 0.02f)
            {
                deltaTime = 0.02f;
            }


            Vector3 position = currentPosition;
            Vector3 velocity = currentVelocity;
            var collider = GetComponent<Collider>();
            var physicsMaterial = collider.material;

            float raycastDistance = 0.1f;

            for (float t = 0; t < time; t += deltaTime)
            {

                var isOnGroundNow = Physics.Raycast(position, Vector3.down, raycastDistance, m_GroundCheckLayer) || Physics.Raycast(position, Vector3.up, 10, m_GroundCheckLayer);

                if (isOnGroundNow)
                {
                    velocity.y = 0;
                    acceleration.y = 0;
                    velocity *= Mathf.Clamp01(1 - physicsMaterial.dynamicFriction * deltaTime);
                }

                velocity += acceleration * deltaTime;

                position += velocity * deltaTime;
            }
            //Debug.Log("Ball Position After " + time + " with delta " + deltaTime + " will be " + position);
            return position;
        }

        private void OnDrawGizmos()
        {
            if (!m_EnableDebug) return;
            
           for ( float t = 0 ; t < 2; t += 0.2f)
           {
               var drop_point = GetDropPointAfterTSeconds(t);
               float raycastDistance = 0.1f;
               var isOnGroundNow =  Physics.Raycast(drop_point, Vector3.down, raycastDistance, m_GroundCheckLayer) || Physics.Raycast(drop_point, Vector3.up, 10, m_GroundCheckLayer);
               if (isOnGroundNow) break;
               Gizmos.DrawSphere(drop_point, 0.5f);
           }
           Gizmos.DrawSphere(transform.position, m_PlayerCheckRadius);
        }
    }


}