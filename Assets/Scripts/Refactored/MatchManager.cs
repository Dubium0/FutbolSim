using System;
using System.Collections;
using System.Collections.Generic;
using FootballSim.FootballTeam;
using FootballSim.Networking;
using FootballSim.Player;
using Unity.Netcode;
using UnityEngine;

namespace FootballSim
{

    public enum MatchState
    {
        Santra,
        Out,
        Playing,
    }

    public class MatchManager : NetworkBehaviour
    {
        public static MatchManager Instance { get; private set; }
        [SerializeField]
        private GameObject m_HomeTeamPrefab;
        [SerializeField]
        private GameObject m_AwayTeamPrefab;

        [SerializeField]
        private GameObject m_AITestTeamPrefab;
        [SerializeField]
        private GameObject m_PlayerTestTeamPrefab;

        [SerializeField]
        private FootballPitch.FootballPitch m_PitchData;

        public FootballPitch.FootballPitch PitchData { get => m_PitchData; }

        public bool IsMatchInitialized { get; private set; } = false;

        public MatchState CurrentMatchState { get; private set; } = MatchState.Santra;

        public FootballTeam.FootballTeam HomeTeam { get; private set; }
        public FootballTeam.FootballTeam AwayTeam { get; private set; }

        public int HomeTeamScore { get; private set; } = 0;
        public int AwayTeamScore { get; private set; } = 0;

        public event Action<int, int> OnScoreChanged;

        public event Action OnMatchStarted;
        public event Action OnMatchResumed;
        public event Action OnMatchStopped;

        private bool m_IsFirstSantra = true;

        public NetworkVariable<int> MatchTime = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private bool m_IsTickingTime = false;

        [SerializeField]
        private bool m_InitWithDebug = false;
        
       

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
            if (m_InitWithDebug)
            {
                SetupOnlineTest(t_Config, t_ClientId);
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
            if (m_InitWithDebug)
            {
                SetupLocalTest(t_Config);
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
            if (m_InitWithDebug)
            {
                SetupLocalTest(t_Config);
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


        private void SetupLocalTest(GameStartConfig t_Config)
        {
            if (IsMatchInitialized)
            {
                Debug.Log("A match is already initialized!");
                return;
            }
            GameObject homeTeamObj = Instantiate(m_PlayerTestTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AITestTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
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

        private void SetupOnlineTest(GameStartConfig t_Config, ulong t_ClientId)
        {
            if (IsMatchInitialized)
            {
                Debug.Log("A match is already initialized!");
                return;
            }
            GameObject homeTeamObj = Instantiate(m_PlayerTestTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AITestTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().SpawnWithOwnership(t_ClientId);
            HomeTeam = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            AwayTeam = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();

            HomeTeam.Init(true, FootballTeam.TeamFlag.Home, 0, true);
            AwayTeam.Init(true, FootballTeam.TeamFlag.Away, 0, true);

            IsMatchInitialized = true;
            InitMatch();
        }


        private void InitMatch()
        {

            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);

            Football.Football.Instance.OnBallHit += HandleSantraAction;
            Football.Football.Instance.OnGoal += HandleGoalAction;
            Football.Football.Instance.OnOut += HandleOutAction;
            Football.Football.Instance.OnGoalKeeperPosses += HandleGoalKeeperPossesAction;
            CurrentMatchState = MatchState.Santra;
            OnMatchStarted += () => { m_IsTickingTime = true; };
            OnMatchStopped += () => { m_IsTickingTime = false; };
            OnMatchResumed += () => { m_IsTickingTime = true; };

        }

        private void HandleGoalKeeperPossesAction(FootballTeam.TeamFlag flag)
        {
            if (CurrentMatchState != MatchState.Out) {
                StartCoroutine(OnGoalKeeperPossesRoutine(flag));
            } 
        }
        private IEnumerator OnGoalKeeperPossesRoutine(FootballTeam.TeamFlag flag)
        {
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }
            CurrentMatchState = MatchState.Out;
            Vector3 targetTransform = Vector3.zero;
            switch (flag)
            {
                case FootballTeam.TeamFlag.Home:
                    targetTransform = PitchData.HomeOutKickPosition.position;
                    HomeTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, true);
                    
                    break;
                case FootballTeam.TeamFlag.Away:
                    targetTransform = PitchData.AwayOutKickPosition.position;
                    HomeTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, true);
                    break;
                default:
                    break;
            }
          
         
            yield return new WaitForSeconds(.5f);
          
            Football.Football.Instance.OnBallHit += HandleFreeKickAction;
            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
            Football.Football.Instance.transform.position = targetTransform;
         
        }
        private void HandleGoalAction(FootballTeam.TeamFlag t_ScorerTeam, FootballPlayer t_ScorerPlayer)
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
            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
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
                OnScoreChanged.Invoke(HomeTeamScore, AwayTeamScore);
            }

        }
        private void HandleSantraAction(FootballPlayer t_Player,Vector3 t_Force)
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


        private void HandleOutAction(FootballTeam.TeamFlag t_FreeKickTeam)
        {
            if (CurrentMatchState != MatchState.Out) {

                StartCoroutine(OnOutRoutine(t_FreeKickTeam));
            }
        }
        private void HandleFreeKickAction(FootballPlayer t_Player, Vector3 t_Force)
        {
            if (CurrentMatchState == MatchState.Out)
            {
                HomeTeam.LockPlayers(false);
                AwayTeam.LockPlayers(false);
                CurrentMatchState = MatchState.Playing;
                Football.Football.Instance.OnBallHit -= HandleFreeKickAction;
                 if (OnMatchResumed != null)
                {
                    OnMatchResumed.Invoke();
                }
            }
        }
        private IEnumerator OnOutRoutine(FootballTeam.TeamFlag t_FreeKickTeam)
        {
            Football.Football.Instance.SetInteractable(false);
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(0.1f);
            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }
            CurrentMatchState = MatchState.Out;
            yield return new WaitForSeconds(0.2f);
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(1.0f);
            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
            Football.Football.Instance.OnBallHit += HandleFreeKickAction;
            switch (t_FreeKickTeam)
            {
                case FootballTeam.TeamFlag.Home:
                    Football.Football.Instance.transform.position = PitchData.HomeOutKickPosition.position;
                    HomeTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, true);
                    break;
                case FootballTeam.TeamFlag.Away:
                    Football.Football.Instance.transform.position = PitchData.AwayOutKickPosition.position;
                    HomeTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, true);
                    break;
                default:
                    break;
            }
            
            Football.Football.Instance.SetInteractable(true);
            
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