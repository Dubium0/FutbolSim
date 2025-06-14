using System;
using System.Collections;
using FootballSim.FootballTeam;
using FootballSim.Networking;
using FootballSim.Player;
using Unity.Cinemachine;
using FootballSim.UI;
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
        [SerializeField]
        private CinemachineFollow m_MatchCamera;

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
        public event Action OnFirstHalfFinish;
        public event Action<FootballTeam.TeamFlag> OnGameFinish;
        public event Action OnGoldenBall;
        public event Action OnInit;
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
            if (OnInit != null)
            {
                OnInit.Invoke();
            }


        }

        private void HandleGoalKeeperPossesAction(FootballTeam.TeamFlag flag)
        {
            if (CurrentMatchState == MatchState.Playing) {
                StartCoroutine(OnGoalKeeperPossesRoutine(flag));
            } 
        }
        private IEnumerator OnGoalKeeperPossesRoutine(FootballTeam.TeamFlag flag)
        {
            HomeTeam.LockPlayers(true,true);
            AwayTeam.LockPlayers(true,true);
            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }
            Football.Football.Instance.SetInteractable(false);
            CurrentMatchState = MatchState.Out;
            Vector3 targetTransform = Vector3.zero;
            switch (flag)
            {
                case FootballTeam.TeamFlag.Home:
                    targetTransform = PitchData.HomeOutKickPosition.position;
                    HomeTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, false, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, false, true);

                    break;
                case FootballTeam.TeamFlag.Away:
                    targetTransform = PitchData.AwayOutKickPosition.position;
                    HomeTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, false, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, false, true);
                    break;
                default:
                    break;
            }
            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.MovePosition(targetTransform);

            yield return new WaitForSeconds(2.0f);
           
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            Football.Football.Instance.SetInteractable(true);
            Football.Football.Instance.OnBallHit += HandleFreeKickAction;
           
         
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
            SoundManager.Instance.PlayGoalSound();
            yield return new WaitForSeconds(0.2f);

            // Get the player who should take the kick-off
            FootballPlayer kickTaker = t_ScorerTeam == FootballTeam.TeamFlag.Home ? 
                HomeTeam.FootballPlayers[0] : 
                AwayTeam.FootballPlayers[0];


            Vector3 ballPosition = kickTaker.transform.position;
            ballPosition.y = 1.4f;
            Football.Football.Instance.Rigidbody.MovePosition(ballPosition);

            CurrentMatchState = MatchState.Santra;
            Football.Football.Instance.OnBallHit += HandleSantraAction;
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(1.0f);
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
            Football.Football.Instance.ResetToStartTransform();
            Football.Football.Instance.SetInteractable(true);
            SoundManager.Instance.PlayGoalKickWhistleSound();
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
            SoundManager.Instance.PlayGoalSound();

        }
        private void HandleSantraAction(FootballPlayer t_Player,Vector3 t_Force)
        {
            if (CurrentMatchState == MatchState.Santra)
            {
                HomeTeam.LockPlayers(false);
                AwayTeam.LockPlayers(false);
                CurrentMatchState = MatchState.Playing;
                Football.Football.Instance.OnBallHit -= HandleSantraAction;

                if (OnMatchStarted != null)
                {
                    OnMatchStarted.Invoke();
                }
                SoundManager.Instance.PlayMatchWhistleSound();
                m_IsFirstSantra = false;
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
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(1.0f);
            AwayTeam.LockPlayers(true,true);
            HomeTeam.LockPlayers(true,true);
            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
            Football.Football.Instance.OnBallHit += HandleFreeKickAction;
            SoundManager.Instance.PlayMatchWhistleSound();
            switch (t_FreeKickTeam)
            {
                case FootballTeam.TeamFlag.Home:
                    Football.Football.Instance.Rigidbody.MovePosition(PitchData.HomeOutKickPosition.position);
                    HomeTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, true);
                    break;
                case FootballTeam.TeamFlag.Away:
                    Football.Football.Instance.Rigidbody.MovePosition(PitchData.AwayOutKickPosition.position);
                    HomeTeam.ChangeFormation(FormationTag.FreeKickDefenseFormation, true);
                    AwayTeam.ChangeFormation(FormationTag.FreeKickAttackFormation, true);
                    break;
                default:
                    break;
            }
            Football.Football.Instance.SetInteractable(true);
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            SoundManager.Instance.PlayGoalKickWhistleSound();
        
            
            
        }

        private void HandleFirstHalfFinish()
        {
            CurrentMatchState = MatchState.Santra;
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            Football.Football.Instance.SetInteractable(false);

            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }
            CurrentMatchState = MatchState.Santra;

            StartCoroutine(ExecuteAfterSeconds(() =>
            {
                if (OnFirstHalfFinish != null)
                {
                    OnFirstHalfFinish.Invoke();
                }
            }));
        }
        private IEnumerator ExecuteAfterSeconds(Action t_Action, float t_Time = 1.0f)
        {
            yield return new WaitForSeconds(t_Time);
            t_Action();
        }

        public void PrepeareForSecondHalf()
        {
            HomeTeam.ChangeFormation(FormationTag.DefenseStartFormation, true);
            AwayTeam.ChangeFormation(FormationTag.AttackStartFormation, true);
            Football.Football.Instance.OnBallHit += HandleSantraAction;

            Football.Football.Instance.Rigidbody.linearVelocity = Vector3.zero;
            Football.Football.Instance.Rigidbody.angularVelocity = Vector3.zero;
            Football.Football.Instance.ResetToStartTransform();
            Football.Football.Instance.SetInteractable(true);
            m_MatchCamera.FollowOffset.z = -m_MatchCamera.FollowOffset.z; //sides are changed !!;
        }

        private void HandleGameFinish()
        {
            if (HomeTeamScore == AwayTeamScore)
            {
                //initiate golden goal
                if (OnGoldenBall != null)
                {
                    OnGoldenBall.Invoke();
                }
                Football.Football.Instance.OnGoal += HandleGoldenGoal;
                return;
            }
            
            HomeTeam.LockPlayers(true);
            AwayTeam.LockPlayers(true);
            Football.Football.Instance.SetInteractable(false);

            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }

            StartCoroutine(ExecuteAfterSeconds(() =>
            {
                if (OnGameFinish != null)
                {
                    if (HomeTeamScore > AwayTeamScore)
                    {
                        OnGameFinish.Invoke(FootballTeam.TeamFlag.Home);
                    }
                    else
                    {
                        OnGameFinish.Invoke(FootballTeam.TeamFlag.Away);
                    }
                }
            }));
        }

        private void HandleGoldenGoal(FootballTeam.TeamFlag t_ScorerTeam, FootballPlayer player)
        {
         
            Football.Football.Instance.SetInteractable(false);
            NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(0.1f);
            if (OnMatchStopped != null)
            {
                OnMatchStopped.Invoke();
            }

            StartCoroutine(ExecuteAfterSeconds(() =>
            {
                NetworkConnectionRPCS.Instance.SetGameTimeScaleRpc(1.0f);
                HomeTeam.LockPlayers(true);
                AwayTeam.LockPlayers(true);

                switch (t_ScorerTeam)
                {
                    case FootballTeam.TeamFlag.Home:
            
                        HomeTeamScore++;
                        break;
                    case FootballTeam.TeamFlag.Away:
                 
                        AwayTeamScore++;
                        break;
                    default:
                        break;
                }
                if (OnGameFinish != null)
                {
                   
                    OnGameFinish.Invoke(t_ScorerTeam);
                    
                }

            },
            0.2f));

         
        }

        private float m_ElapsedTickTime = 0;
        private bool m_IsFirstHalf = true;
        private bool m_IsGameFinishedCalled = false;
        private void Update()
        {
            if (IsHost && CurrentMatchState == MatchState.Playing)
            {
                if (MatchTime.Value >= 90 && m_IsFirstHalf)
                {
                    // first half
                    m_IsFirstHalf = false;
                    HandleFirstHalfFinish();
                }
                else if (MatchTime.Value >= 180 && !m_IsGameFinishedCalled)
                {
                    
                    m_IsGameFinishedCalled = true;
                    HandleGameFinish();
                }
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