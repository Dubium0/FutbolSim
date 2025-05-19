

using System;
using System.Collections.Generic;
using FootballSim.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FootballSim.FootballTeam
{
    public enum TeamFlag
    {   
        NotInitialized,
        Home,
        Away

    }
    public class FootballTeam : NetworkBehaviour
    {

        [SerializeField]
        private GameObject m_PlayerPrefab;


        [SerializeField]
        private FootballFormation m_DefenseFormation;
        [SerializeField]
        private FootballFormation m_AttackStartFormation;
        [SerializeField]
        private FootballFormation m_DefenseStartFormation;
        [SerializeField]
        private FootballFormation m_DefaultFormation;
        [SerializeField]
        private FootballFormation m_AttackFormation;

        private FootballFormation m_CurrentFormation;

        private bool m_IsInitialized = false;

        private InputAction m_CycleToClosestPlayerButton;

        public TeamFlag TeamFlag { get; private set; } = TeamFlag.NotInitialized;

        public List<FootballPlayer> FootballPlayers { get; private set; } = new();

        public FootballPlayer ClosestPlayerToBall { get; private set; } = null; // other than owner

        public FootballPlayer CurrentBallOwnerPlayer { get; private set; } = null;

        public FootballPlayer CurrentControlledPlayer { get; private set; } = null;
        private int m_PlayerControlIndex = 0;


        private bool m_IsHumanControlled = false;

        //server call
        public void Init(bool t_IsHumanControlled, TeamFlag t_TeamFlag, int t_PlayerControlIndex = 0,bool t_IsOnlineSpawn= false)
        {
            if (IsHost)
            {
                InitRpc(t_IsHumanControlled, t_TeamFlag);
            }
            TeamFlag = t_TeamFlag;
            m_CurrentFormation = TeamFlag == TeamFlag.Home ? m_AttackStartFormation : m_DefenseFormation;

            m_PlayerControlIndex = t_PlayerControlIndex;
            m_IsInitialized = true;
            m_IsHumanControlled = t_IsHumanControlled;
            SetupAndBindActions();

            if (IsHost)
            {
                SpawnTeam(t_IsOnlineSpawn);
            }

        }
        private void FixedUpdate()
        {
            if (IsHost)
            {
                FindClosestPlayerToBall();
            }
        }
        private void SetupAndBindActions()
        {
            
            string playerPrefix;
            if (m_PlayerControlIndex  == 1)
            {
                playerPrefix ="Keyboard Only/" ;
            }
            else if (m_PlayerControlIndex == 2)
            {
                playerPrefix ="Controller Only/";
            }
            else
            {
                playerPrefix = "Player/";
            }
            m_CycleToClosestPlayerButton = InputSystem.actions?.FindAction($"{playerPrefix}CycleToClosestPlayer");

            if (IsHost && IsOwner)
            {
                m_CycleToClosestPlayerButton.performed += context => { CycleToClosestPlayer(); };
            }

            if (IsClient && IsOwner && !IsHost)
            {
                m_CycleToClosestPlayerButton.performed += context => { CycleToClosestPlayerRpc(); };
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitRpc(bool t_IsHumanControlled,TeamFlag t_TeamFlag)
        {
            if (IsHost) return;
            Init(t_IsHumanControlled,t_TeamFlag,0,true);

        }


        //server call
        // all spawn logic server side
        public void SpawnTeam(bool t_IsOnlineSpawn = false)
        {
            if (IsHost) // only host can spawn the shit 
            {
                var networkObject = GetComponent<NetworkObject>();
                var ownerOfTheTeam = networkObject.OwnerClientId;

                // goalkeeper
                {
                    GameObject player = Instantiate(m_PlayerPrefab, m_CurrentFormation.GoalKeeperPosition.position,  m_CurrentFormation.GoalKeeperPosition.rotation);
                    player.GetComponent<NetworkObject>().SpawnWithOwnership(ownerOfTheTeam);

                    var playerScript = player.GetComponent<FootballPlayer>();
                    playerScript.Init(false, t_IsOnlineSpawn, m_PlayerControlIndex);
                    playerScript.OnBallWinCallback += player =>
                    {
                        CurrentBallOwnerPlayer = player;

                        if (m_IsHumanControlled)
                        {
                            if (CurrentControlledPlayer != null)
                            {
                                CurrentControlledPlayer.SetHumanControlled(false);
                            }
                            CurrentControlledPlayer = CurrentBallOwnerPlayer;
                            CurrentBallOwnerPlayer.SetHumanControlled(true);
                        }
                    };
                    FootballPlayers.Add(playerScript);

                }
                PlayerSpawnHelper(m_CurrentFormation.DefensePosition, ownerOfTheTeam, t_IsOnlineSpawn, m_PlayerControlIndex);
                PlayerSpawnHelper(m_CurrentFormation.MidfieldPosition, ownerOfTheTeam, t_IsOnlineSpawn, m_PlayerControlIndex);
                PlayerSpawnHelper(m_CurrentFormation.ForwardPosition, ownerOfTheTeam, t_IsOnlineSpawn, m_PlayerControlIndex);

                CycleToClosestPlayer();
            }
        }

        private void PlayerSpawnHelper(Transform[] t_Transforms,
            ulong t_OwnerOfTheTeam,
            bool t_IsOnlineSpawn = false,
            int t_PlayerIndex = 0)
        {
            foreach (var playerTransform in t_Transforms)
            {
                GameObject player = Instantiate(m_PlayerPrefab, playerTransform.position, playerTransform.rotation);
                player.GetComponent<NetworkObject>().SpawnWithOwnership(t_OwnerOfTheTeam);

                var playerScript = player.GetComponent<FootballPlayer>();
                playerScript.Init(false, t_IsOnlineSpawn, t_PlayerIndex);
                playerScript.OnBallWinCallback += player =>
                {
                    CurrentBallOwnerPlayer = player;

                    if (m_IsHumanControlled)
                    {
                        if (CurrentControlledPlayer != null)
                        {
                            CurrentControlledPlayer.SetHumanControlled(false);
                        }
                        CurrentControlledPlayer = CurrentBallOwnerPlayer;
                        CurrentBallOwnerPlayer.SetHumanControlled(true);
                    }
                };
                FootballPlayers.Add(playerScript);

            }
        }


        private void CycleToClosestPlayer()
        {
            if (ClosestPlayerToBall == null)
            {
                FindClosestPlayerToBall();
            }

            if (CurrentBallOwnerPlayer == null) 
            {
                if (CurrentControlledPlayer != ClosestPlayerToBall && m_IsHumanControlled)
                {
                    if(CurrentControlledPlayer !=null) CurrentControlledPlayer.SetHumanControlled(false);
                    CurrentControlledPlayer = ClosestPlayerToBall;
                    CurrentControlledPlayer.SetHumanControlled(true);
                }
            }

        }
        [Rpc(SendTo.Server)]
        private void CycleToClosestPlayerRpc()
        {
            CycleToClosestPlayer();
        }


        private void FindClosestPlayerToBall()
        {
            //can tick this for 1 second etc
            var ballPosition = Football.Football.Instance.GetDropPointAfterTSeconds(0.5f); // always 0.5f seconds later position
            ballPosition.y = 0;
            float minDistance =  9999999;
            FootballPlayer minDistancePlayer = null;

            foreach (var player in FootballPlayers)
            {

                if (CurrentBallOwnerPlayer != null) {
                    if (player == CurrentBallOwnerPlayer)
                    {
                        continue;
                    }
                }
                var distance = (player.transform.position - ballPosition).sqrMagnitude;

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minDistancePlayer = player;
                }


            }

            ClosestPlayerToBall = minDistancePlayer;
        

        }
    }



}