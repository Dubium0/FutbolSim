

using System;
using System.Collections.Generic;
using FootballSim.Player;
using JetBrains.Annotations;
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
    public enum FormationTag
    {
        DefenseFormation,
        AttackStartFormation,
        DefenseStartFormation,
        DefaultFormation,
        AttackFormation
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

        public FormationTag FormationTag { get; private set; }

        private bool m_IsInitialized = false;

        private InputAction m_CycleToClosestPlayerButton;


        public event Action OnSantraAction;


        public TeamFlag TeamFlag { get; private set; } = TeamFlag.NotInitialized;

        public List<FootballPlayer> FootballPlayers { get; private set; } = new();

        public FootballPlayer ClosestPlayerToBall { get; private set; } = null; // other than owner

        public FootballPlayer CurrentBallOwnerPlayer { get; private set; } = null;

        public FootballPlayer CurrentControlledPlayer { get; private set; } = null;


        private int m_PlayerControlIndex = 0;


        private bool m_IsHumanControlled = false;
        public bool IsHumanControlled { get { return m_IsHumanControlled; } }

        //server call
        public void Init(bool t_IsHumanControlled, TeamFlag t_TeamFlag, int t_PlayerControlIndex = 0, bool t_IsOnlineSpawn = false)
        {
            if (IsHost)
            {
                InitRpc(t_IsHumanControlled, t_TeamFlag);
            }
            TeamFlag = t_TeamFlag;

            m_CurrentFormation = TeamFlag == TeamFlag.Home ? m_AttackStartFormation : m_DefenseFormation;
            FormationTag = TeamFlag == TeamFlag.Home ? FormationTag.AttackStartFormation : FormationTag.DefenseFormation;
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
                //CycleToClosestPlayer();
                DetectCurrentFormation();
            }
        }
        private void SetupAndBindActions()
        {

            string playerPrefix;
            if (m_PlayerControlIndex == 1)
            {
                playerPrefix = "Keyboard Only/";
            }
            else if (m_PlayerControlIndex == 2)
            {
                playerPrefix = "Controller Only/";
            }
            else
            {
                playerPrefix = "Player/";
            }
            m_CycleToClosestPlayerButton = InputSystem.actions?.FindAction($"{playerPrefix}CycleToClosestPlayer");


            if (IsHost && IsOwner)
            {
                Debug.Log("Setting up for the cycle player input");
                m_CycleToClosestPlayerButton.performed += context => { CycleToClosestPlayer(); };


            }

            if (IsClient && IsOwner && !IsHost)
            {
                m_CycleToClosestPlayerButton.performed += context => { CycleToClosestPlayerRpc(); };

            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        public void InitRpc(bool t_IsHumanControlled, TeamFlag t_TeamFlag)
        {
            if (IsHost) return;
            Init(t_IsHumanControlled, t_TeamFlag, 0, true);

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
                    GameObject player = Instantiate(m_PlayerPrefab, m_CurrentFormation.GoalKeeperPosition.position, m_CurrentFormation.GoalKeeperPosition.rotation);
                    player.GetComponent<NetworkObject>().SpawnWithOwnership(ownerOfTheTeam);

                    var playerScript = player.GetComponent<FootballPlayer>();
                    playerScript.Init(this, TeamFlag, Player.PlayerType.GoalKeeper, false, t_IsOnlineSpawn, m_PlayerControlIndex);
                    playerScript.SetHomePosition(m_CurrentFormation.GoalKeeperPosition);

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

                    playerScript.OnBallLoseCallback += player =>
                    {
                        if (CurrentBallOwnerPlayer == player)
                        {
                            CurrentBallOwnerPlayer = null;
                        }

                    };
                    FootballPlayers.Add(playerScript);

                }
                PlayerSpawnHelper(m_CurrentFormation.DefensePosition, ownerOfTheTeam, Player.PlayerType.Defender, t_IsOnlineSpawn, m_PlayerControlIndex);
                PlayerSpawnHelper(m_CurrentFormation.MidfieldPosition, ownerOfTheTeam, Player.PlayerType.Midfielder, t_IsOnlineSpawn, m_PlayerControlIndex);
                PlayerSpawnHelper(m_CurrentFormation.ForwardPosition, ownerOfTheTeam, Player.PlayerType.Forward, t_IsOnlineSpawn, m_PlayerControlIndex);

                CycleToClosestPlayer();
            }
        }

        private void PlayerSpawnHelper(List<Transform> t_Transforms,
            ulong t_OwnerOfTheTeam,
            Player.PlayerType t_PlayerType,
            bool t_IsOnlineSpawn = false,
            int t_PlayerIndex = 0)
        {
            foreach (var playerTransform in t_Transforms)
            {
                GameObject player = Instantiate(m_PlayerPrefab, playerTransform.position, playerTransform.rotation);
                player.GetComponent<NetworkObject>().SpawnWithOwnership(t_OwnerOfTheTeam);

                var playerScript = player.GetComponent<FootballPlayer>();
                playerScript.Init(this, TeamFlag, t_PlayerType, false, t_IsOnlineSpawn, t_PlayerIndex);
                playerScript.SetHomePosition(playerTransform);
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

                playerScript.OnBallLoseCallback += player =>
                {
                    if (CurrentBallOwnerPlayer == player)
                    {
                        CurrentBallOwnerPlayer = null;
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
                    if (CurrentControlledPlayer != null) CurrentControlledPlayer.SetHumanControlled(false);
                    CurrentControlledPlayer = ClosestPlayerToBall;
                    CurrentControlledPlayer.SetHumanControlled(true);
                    Debug.Log("Cycle to closest player clicked");
                }
            }

        }
        [Rpc(SendTo.Server)]
        private void CycleToClosestPlayerRpc()
        {
            CycleToClosestPlayer();
        }

        public void LockPlayers(bool t_Value)
        {
            if (m_IsInitialized)
            {
                foreach (var player in FootballPlayers)
                {
                    player.LockPlayerMovement(t_Value);
                }
            }
        }

        private void FindClosestPlayerToBall()
        {
            //can tick this for 1 second etc
            var ballPosition = Football.Football.Instance.GetDropPointAfterTSeconds(0.5f); // always 0.5f seconds later position
            ballPosition.y = 0;
            float minDistance = 9999999;
            FootballPlayer minDistancePlayer = null;

            foreach (var player in FootballPlayers)
            {

                if (CurrentBallOwnerPlayer != null)
                {
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

        public void ChangeFormation(FormationTag t_TargetFormation, bool t_ImmidietalyMove = false)
        {
            FormationTag = t_TargetFormation;

            switch (FormationTag)
            {
                case FormationTag.DefenseFormation:
                    MovePlayersToFormationPositions(m_DefenseFormation, t_ImmidietalyMove);
                    break;
                case FormationTag.AttackFormation:
                    MovePlayersToFormationPositions(m_AttackFormation, t_ImmidietalyMove);
                    break;
                case FormationTag.AttackStartFormation:
                    MovePlayersToFormationPositions(m_AttackStartFormation, t_ImmidietalyMove);
                    break;
                case FormationTag.DefenseStartFormation:
                    MovePlayersToFormationPositions(m_DefenseStartFormation, t_ImmidietalyMove);
                    break;
                case FormationTag.DefaultFormation:
                    MovePlayersToFormationPositions(m_DefaultFormation, t_ImmidietalyMove);
                    break;
            }
            print($"Changing formation to {FormationTag}");
        }
        private void MovePlayersToFormationPositions(FootballSim.FootballTeam.FootballFormation t_TargetFormation, bool t_ImmidietalyMove = false)
        {
            if (!m_IsInitialized) return;
            m_CurrentFormation = t_TargetFormation;

            int defenseIndex = 0;
            int midfieldIndex = 0;
            int forwardIndex = 0;
            foreach (var player in FootballPlayers)
            {
                Transform targetPosition = player.CurrentHomePosition;
                switch (player.PlayerType)
                {

                    case Player.PlayerType.GoalKeeper:
                        targetPosition = t_TargetFormation.GoalKeeperPosition;
                        break;
                    case Player.PlayerType.Defender:
                        targetPosition = t_TargetFormation.DefensePosition[defenseIndex];
                        defenseIndex++;
                        break;
                    case Player.PlayerType.Midfielder:
                        targetPosition = t_TargetFormation.MidfieldPosition[midfieldIndex];
                        midfieldIndex++;
                        break;
                    case Player.PlayerType.Forward:
                        targetPosition = t_TargetFormation.ForwardPosition[forwardIndex];
                        forwardIndex++;
                        break;

                }
                player.SetHomePosition(targetPosition);
                if (t_ImmidietalyMove)
                    player.ImmidiatelyMoveToHomePosition();
            }


        }


        public int GetPlayerIndex(FootballPlayer t_Player) => FootballPlayers.FindIndex(player => t_Player == player);


        private void DetectCurrentFormation()
        {
            if (Football.Football.Instance == null || !m_IsInitialized) return;
            int currentSector = FootballSim.Football.Football.Instance.CurrentSector;
            
            if (TeamFlag == TeamFlag.Home) // Home team attacks right
            {
                if (CurrentBallOwnerPlayer != null)
                {
                    // according to next position of ball( is it closer to enemy goal or our goal) change into attack or

                    var nextBallPosition = CurrentBallOwnerPlayer.transform.position +  CurrentBallOwnerPlayer.Rigidbody.linearVelocity * 0.5f;
                    var currentBallPosition = CurrentBallOwnerPlayer.transform.position;

                    var enemyGoalPosition = MatchManager.Instance.PitchData.AwayGoalTransform.position;

                    if ((enemyGoalPosition - nextBallPosition).sqrMagnitude < (enemyGoalPosition - currentBallPosition).sqrMagnitude)
                    {
                        // means ball is going to enemy goal position
                        ChangeFormation(FormationTag.AttackFormation);
                    }
                    else
                    {
                        ChangeFormation(FormationTag.DefaultFormation);
                    }

                }
                else
                {
                    switch (currentSector)
                    {
                        case 0:
                            ChangeFormation(FormationTag.DefenseFormation);
                            break;
                        case 1:
                            ChangeFormation(FormationTag.DefaultFormation);
                            break;
                        case 2:
                            ChangeFormation(FormationTag.AttackFormation);
                            break;
                    }
                }
            }
            else 
            {
                if (CurrentBallOwnerPlayer != null)
                {
                  
                    var nextBallPosition = CurrentBallOwnerPlayer.transform.position +  CurrentBallOwnerPlayer.Rigidbody.linearVelocity * 0.5f;
                    var currentBallPosition = CurrentBallOwnerPlayer.transform.position;

                    var enemyGoalPosition = MatchManager.Instance.PitchData.HomeGoalTransform.position;

                    if ((enemyGoalPosition - nextBallPosition).sqrMagnitude < (enemyGoalPosition - currentBallPosition).sqrMagnitude)
                    {
                        // means ball is going to enemy goal position
                        ChangeFormation(FormationTag.AttackFormation);
                    }
                    else
                    {
                        ChangeFormation(FormationTag.DefaultFormation);
                    }

                }
                else
                {
                        
                    switch (currentSector)
                    {
                        case 0:
                            ChangeFormation(FormationTag.AttackFormation);
                            break;
                        case 1:
                            ChangeFormation(FormationTag.DefaultFormation);
                            break;
                        case 2:
                            ChangeFormation(FormationTag.DefenseFormation);
                            break;
                    }
                }
            }
        }
    }



}