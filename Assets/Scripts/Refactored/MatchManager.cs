using System;
using System.Collections;
using FootballSim.FootballTeam;
using FootballSim.Networking;
using FootballSim.Player;
using NUnit.Framework.Internal.Filters;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

namespace FootballSim
{

    public enum MatchState
    {
        Santra,
        Playing,

    }

    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }
        [SerializeField]
        private GameObject m_HomeTeamPrefab;
        [SerializeField]
        private GameObject m_AwayTeamPrefab;

        public bool IsMatchInitialized { get; private set; } = false;

        public MatchState CurrentMatchState { get; private set; } = MatchState.Santra;

        public FootballTeam.FootballTeam HomeTeam { get; private set; }
        public FootballTeam.FootballTeam AwayTeam { get; private set; }

        public int HomeTeamScore { get; private set; } = 0;
        public int AwayTeamScore { get; private set; } = 0;

        public event Action<int,int> OnScoreChanged;

        public event Action OnMatchStarted;
        public event Action OnMatchResumed;
        public event Action OnMatchStopped;

        private bool m_IsFirstSantra = true;

        public NetworkVariable<int> MatchTime = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private bool m_IsTickingTime = false;
        
        private void Awake()
        {
            if (Instance == null)
            {

                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetupOnlinePVP(GameStartConfig t_Config, ulong t_ClientId)
        {
            if (IsMatchInitialized)
            {
                Debug.Log("A match is already initialized!");
                return;
            }
            GameObject homeTeamObj = Instantiate(m_HomeTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AwayTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().SpawnWithOwnership(t_ClientId);
            HomeTeam = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            AwayTeam = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();

            HomeTeam.Init(true, FootballTeam.TeamFlag.Home, 0, true);
            AwayTeam.Init(true, FootballTeam.TeamFlag.Away, 0, true);

            IsMatchInitialized = true;
            InitMatch();
        }

        public void SetupLocalPVP(GameStartConfig t_Config)
        {
            if (IsMatchInitialized)
            {
                Debug.Log("A match is already initialized!");
                return;
            }
            GameObject homeTeamObj = Instantiate(m_HomeTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AwayTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().Spawn();
            HomeTeam = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            AwayTeam = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();
            if (t_Config.HomePlayerIndex != -1)
            {

                HomeTeam.Init(true, FootballTeam.TeamFlag.Home, t_Config.HomePlayerIndex);

            }
            else
            {
                HomeTeam.Init(false, FootballTeam.TeamFlag.Home);
            }

            if (t_Config.AwayPlayerIndex != -1)
            {

                AwayTeam.Init(true, FootballTeam.TeamFlag.Away, t_Config.AwayPlayerIndex);

            }
            else
            {
                AwayTeam.Init(false, FootballTeam.TeamFlag.Away);
            }
            InitMatch();
        }

        public void SetupLocalPVA(GameStartConfig t_Config)
        {
            if (IsMatchInitialized)
            {
                Debug.Log("A match is already initialized!");
                return;
            }
            GameObject homeTeamObj = Instantiate(m_HomeTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AwayTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().Spawn();
            HomeTeam = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            AwayTeam = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();
            if (t_Config.HomePlayerIndex != -1)
            {

                HomeTeam.Init(true, FootballTeam.TeamFlag.Home, t_Config.HomePlayerIndex);

            }
            else
            {
                HomeTeam.Init(false, FootballTeam.TeamFlag.Home);
            }

            if (t_Config.AwayPlayerIndex != -1)
            {

                AwayTeam.Init(true, FootballTeam.TeamFlag.Away, t_Config.AwayPlayerIndex);

            }
            else
            {
                AwayTeam.Init(false, FootballTeam.TeamFlag.Away);
            }
            InitMatch();
        }


        private void InitMatch()
        {

            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);

            Football.Football.Instance.OnBallHit += HandleSantraAction;
            Football.Football.Instance.OnGoal += HandleGoalAction;
            CurrentMatchState = MatchState.Santra;
            OnMatchStarted += () => { m_IsTickingTime = true; };
            OnMatchStopped += () => { m_IsTickingTime = false; };
            OnMatchResumed += () => { m_IsTickingTime = true; };

        }

        private void HandleGoalAction(FootballTeam.TeamFlag t_ScorerTeam,FootballPlayer t_ScorerPlayer)
        {
            StartCoroutine(OnGoalRoutine(t_ScorerTeam, t_ScorerPlayer));
        }
        private IEnumerator OnGoalRoutine(FootballTeam.TeamFlag t_ScorerTeam, FootballPlayer t_ScorerPlayer)
        {
            //first zoom into scorerplayer
            Football.Football.Instance.SetInteractable(false);
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(0.1f);
            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }
            yield return new WaitForSeconds(0.2f);
            CurrentMatchState = MatchState.Santra;
            Football.Football.Instance.OnBallHit += HandleSantraAction;
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(1.0f);
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            Football.Football.Instance.ResetToStartTransform();
            Football.Football.Instance.SetInteractable(true);
            switch (t_ScorerTeam)
            {
                case FootballTeam.TeamFlag.Home:
                    HomeTeam.ChangeFormation(FormationTag.DefenseStartFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.AttackStartFormation, true);
                    HomeTeamScore++;
                    break;
                case FootballTeam.TeamFlag.Away:
                    AwayTeam.ChangeFormation(FormationTag.DefenseStartFormation, true);
                    HomeTeam.ChangeFormation(FormationTag.AttackStartFormation, true);
                    AwayTeamScore++;
                    break;
                default:
                    break;
            }
            if (OnScoreChanged != null)
            {
                OnScoreChanged.Invoke(HomeTeamScore,AwayTeamScore);
            }

        }
        private void HandleSantraAction(FootballPlayer t_Player)
        {
            if (CurrentMatchState == MatchState.Santra)
            {
                HomeTeam.LockPlayers(false);
                AwayTeam.LockPlayers(false);
                CurrentMatchState = MatchState.Playing;
                Football.Football.Instance.OnBallHit -= HandleSantraAction;

                if (m_IsFirstSantra)
                {
                    if (OnMatchStarted != null)
                    {
                        OnMatchStarted.Invoke();
                    }
                    m_IsFirstSantra = false;
                }
                else
                {
                    if (OnMatchResumed != null)
                    {
                        OnMatchResumed.Invoke();
                    }
                }
            }
        }



        private float m_ElapsedTickTime = 0;
        private void Update()
        {
            if (IsHost && CurrentMatchState == MatchState.Playing)
            {

            }
            if (m_IsTickingTime)
            {
                m_ElapsedTickTime += Time.deltaTime;
                
                if (m_ElapsedTickTime >= 1.0f)
                {
                    MatchTime.Value += 1;
                    m_ElapsedTickTime = 0;
                }
            }
        }


    }


}