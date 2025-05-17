using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public enum GameMode
{
    OnlinePVP,
    LocalPVP,
    PVA
}
public struct GameStartConfig
{
    public int homePlayerCount;
    public int awayPlayerCount;

    public GameMode gameMode;
    public ulong clientId;
    
    public Dictionary<TeamFlag, List<int>> teamPlayerIndices;
}

public enum EGameState
{
    NotStarted,
    Playing,
    Frozen,
    RedWin,
    BlueWin,
    Draw
}

public class GameManager : NetworkBehaviour
{
   
    
    public Transform HomeGoalPosition;
    public Bounds HomeGoalBounds;

    public Goal AwayGoal;
    public Goal HomeGoal;
    
    public Transform AwayGoalPosition;
    public Bounds AwayGoalBounds;

    public FootballTeam homeFootballTeam;
    public FootballTeam awayFootballTeam;

    public static GameManager Instance;

    private EGameState state_;
    public EGameState GameState {  get { return state_; } }

    private void OnValidate()
    {
        AwayGoalBounds.center = AwayGoalPosition.position;

        HomeGoalBounds.center = HomeGoalPosition.position;
    }
    private void Awake()
    {
        if(IsClient) Destroy(gameObject);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

        }
        else
        {
            Instance = this;
            state_ = EGameState.NotStarted;
        }
    }
    private void Start()
    {
        //GameStartConfig config = new GameStartConfig();
        //config.awayPlayerCount = 0;
        //config.homePlayerCount = 1;
        //
        //
        //StartGame(config);
    }
    
    public void StartGame(GameStartConfig config)
    {

        switch (config.gameMode)
        {
            case GameMode.PVA:
                break;
            case GameMode.OnlinePVP:
                StartOnlineGame(config);
                break;
            case GameMode.LocalPVP:
                StartLocalPVP(config);
                break;
        }
        state_ = EGameState.Playing;
    }
    // probably temporary



    private void StartOnlineGame(GameStartConfig config)
    {
         
        awayFootballTeam.init(true, false, config.clientId);
        homeFootballTeam.init(true, true, NetworkManager.ServerClientId);
        state_ = EGameState.Playing;

        
    }
    private void StartLocalPVP(GameStartConfig config)
    {
        var homePlayerIndices = config.teamPlayerIndices[TeamFlag.Home];
        var awayPlayerIndices = config.teamPlayerIndices[TeamFlag.Away];
       
        if (homePlayerIndices.Count > 0)
        {
            Debug.Log("[Current Problem]  I am in home player init");
            homeFootballTeam.init(true, true,null,homePlayerIndices[0]);
     
        }
        else homeFootballTeam.init(false, true);

        if (awayPlayerIndices.Count > 0)
        {
             Debug.Log("[Current Problem]  I am in away player init");
            awayFootballTeam.init(true, false,null,awayPlayerIndices[0]);

        }
        else awayFootballTeam.init(false, false);

        
        
    }
    

    public void PauseGame()
    {
        state_ = EGameState.Frozen;
        Time.timeScale = 0;
    }
    public void ResumeGame()
    {
        state_ = EGameState.Playing;
        Time.timeScale = 1;
    }
    

   // public void StartGame()
   // {
   //     Debug.Log($"[Game Start] Starting game with isBothTeamsControlled={isBothTeamsControlled}, teamFlag={teamFlag}");
   //     
   //     bool isRedControlled = teamFlag == TeamFlag.Home;
   //     RedFootballTeam.enabled = true;
   //     BlueFootballTeam.enabled = true;
   //     state = GameState.Playing;
   // 
   //     if (!isBothTeamsControlled)
   //     {   
   //         RedFootballTeam.isHumanControllable = isRedControlled;
   //         BlueFootballTeam.isHumanControllable = !isRedControlled;
   //         Debug.Log($"[Team Control] Red Team human controlled: {isRedControlled}, Blue Team human controlled: {!isRedControlled}");
   //     }
   //     else
   //     {
   //         RedFootballTeam.isHumanControllable = true;
   //         BlueFootballTeam.isHumanControllable = true;
   //         Debug.Log("[Team Control] Both teams are human controlled");
   //     }
//
   //     // Get controller types and input map state from SelectSideManager
   //     var selectSideManager = FindObjectOfType<SelectSideManager>();
   //     if (selectSideManager != null)
   //     {
   //         var controllerTypes = selectSideManager.GetPlayerControllerTypes();
   //         bool isInputMapSwapped = selectSideManager.IsInputMapSwapped();
   //         Debug.Log($"[Controller Types] Got controller types: {string.Join(", ", controllerTypes)}");
   //         Debug.Log($"[Input Map] Input map swapped: {isInputMapSwapped}");
   //         
   //         // Set indices based on input map swap state
   //         List<int> redIndices = new List<int> { isInputMapSwapped ? 1 : 0 };
   //         List<int> blueIndices = new List<int> { isInputMapSwapped ? 0 : 1 };
   //         
   //         Debug.Log($"[Team Assignment] Setting team indices - Red Team: Player {redIndices[0]}, Blue Team: Player {blueIndices[0]}");
   //         RedFootballTeam.SetPlayerIndices(redIndices);
   //         BlueFootballTeam.SetPlayerIndices(blueIndices);
   //     }
   //     else
   //     {
   //         Debug.LogWarning("[Team Assignment] SelectSideManager not found, using default player indices");
   //         RedFootballTeam.SetPlayerIndices(new List<int> { 0 });
   //         BlueFootballTeam.SetPlayerIndices(new List<int> { 1 });
   //     }
   // }


    public Vector3 GetGoalPositionHome(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Away:
                return AwayGoalPosition.position;
            case TeamFlag.Home:
                return HomeGoalPosition.position;
            default:
                return AwayGoalPosition.position;

        }
    }

    public Bounds GetBoundsHome(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Away:
                AwayGoalBounds.center = AwayGoalPosition.position;  
                return AwayGoalBounds ;
            case TeamFlag.Home:
                HomeGoalBounds.center = HomeGoalPosition.position;
                return HomeGoalBounds;
            default:
                return AwayGoalBounds;

        }
    }

    public Bounds GetBoundsAway(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Away:
                HomeGoalBounds.center = HomeGoalPosition.position;
                return HomeGoalBounds;
              
            case TeamFlag.Home:
                AwayGoalBounds.center = AwayGoalPosition.position;
                return AwayGoalBounds;
            default:
                return AwayGoalBounds;

        }
    }
    public Vector3 GetGoalPositionAway(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Away:
                return HomeGoalPosition.position;
                
            case TeamFlag.Home:
                return AwayGoalPosition.position;
            default:
                return AwayGoalPosition.position;

        }
    }

    public Goal GetEnemyGoalInstance( TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Away:
                return HomeGoal;

            case TeamFlag.Home:
                return AwayGoal;
            default:
                return AwayGoal;

        }

    }

    [SerializeField]
    private LayerMask awayTeamLayerMask;

    [SerializeField]
    private LayerMask homeTeamLayerMask;

    public LayerMask GetLayerMaskOfEnemy(TeamFlag teamFlag)
    {
        switch(teamFlag)
        {
            case TeamFlag.Home:
                return awayTeamLayerMask;
            case TeamFlag.Away:
                return homeTeamLayerMask;
            default:
            return awayTeamLayerMask;


        }

    }

    public LayerMask GetLayerMaskOfTeamMate(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Home:
                return homeTeamLayerMask;
              
            case TeamFlag.Away:
                return awayTeamLayerMask;
            default:
                return awayTeamLayerMask;


        }

    }

    public List<GameObject> GetPlayersInGoalArea(TeamFlag agentTeamFlag)
    {
        List<GameObject> playersInGoalArea = new List<GameObject>();
        Bounds goalBounds = GetBoundsHome(agentTeamFlag);
        Collider[] colliders = Physics.OverlapBox(goalBounds.center, goalBounds.extents, Quaternion.identity);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
                playersInGoalArea.Add(collider.gameObject);
        }

        return playersInGoalArea;
    }
}

