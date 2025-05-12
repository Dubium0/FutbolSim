using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Start,
    Playing,
    RedWin,
    BlueWin,
    Draw
}

public class GameManager : MonoBehaviour
{
    public GameState state = GameState.Start;
    
    public Transform RedGoalPosition;
    public Bounds RedGoalBounds;

    public Goal BlueGoal;
    public Goal RedGoal;
    
    public Transform BlueGoalPosition;
    public Bounds BlueGoalBounds;
    
    public FootballTeam RedFootballTeam;
    public FootballTeam BlueFootballTeam;

    public static GameManager Instance;

    private void OnValidate()
    {
        BlueGoalBounds.center = BlueGoalPosition.position;

        RedGoalBounds.center = RedGoalPosition.position;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {

            Destroy(this);

        }
        else
        {
            Instance = this;
        }
    }
    
    private void Start()
    {
        state = GameState.Start;
        RedFootballTeam.enabled = false;
        BlueFootballTeam.enabled = false;
    }

    public void StartGame(bool isBothTeamsControlled, TeamFlag teamFlag = TeamFlag.Blue, Dictionary<TeamFlag, List<int>> teamPlayerIndices = null)
    {
        Debug.Log($"[Game Start] Starting game with isBothTeamsControlled={isBothTeamsControlled}, teamFlag={teamFlag}");
        
        bool isRedControlled = teamFlag == TeamFlag.Red;
        RedFootballTeam.enabled = true;
        BlueFootballTeam.enabled = true;
        state = GameState.Playing;
    
        if (!isBothTeamsControlled)
        {   
            RedFootballTeam.isHumanControllable = isRedControlled;
            BlueFootballTeam.isHumanControllable = !isRedControlled;
            Debug.Log($"[Team Control] Red Team human controlled: {isRedControlled}, Blue Team human controlled: {!isRedControlled}");
        }
        else
        {
            RedFootballTeam.isHumanControllable = true;
            BlueFootballTeam.isHumanControllable = true;
            Debug.Log("[Team Control] Both teams are human controlled");
        }

        // Get controller types and input map state from SelectSideManager
        var selectSideManager = FindObjectOfType<SelectSideManager>();
        if (selectSideManager != null)
        {
            var controllerTypes = selectSideManager.GetPlayerControllerTypes();
            bool isInputMapSwapped = selectSideManager.IsInputMapSwapped();
            Debug.Log($"[Controller Types] Got controller types: {string.Join(", ", controllerTypes)}");
            Debug.Log($"[Input Map] Input map swapped: {isInputMapSwapped}");
            
            // Set indices based on input map swap state
            List<int> redIndices = new List<int> { isInputMapSwapped ? 1 : 0 };
            List<int> blueIndices = new List<int> { isInputMapSwapped ? 0 : 1 };
            
            Debug.Log($"[Team Assignment] Setting team indices - Red Team: Player {redIndices[0]}, Blue Team: Player {blueIndices[0]}");
            RedFootballTeam.SetPlayerIndices(redIndices);
            BlueFootballTeam.SetPlayerIndices(blueIndices);
        }
        else
        {
            Debug.LogWarning("[Team Assignment] SelectSideManager not found, using default player indices");
            RedFootballTeam.SetPlayerIndices(new List<int> { 0 });
            BlueFootballTeam.SetPlayerIndices(new List<int> { 1 });
        }
    }


    public Vector3 GetGoalPositionHome(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                return BlueGoalPosition.position;
            case TeamFlag.Red:
                return RedGoalPosition.position;
            default:
                return BlueGoalPosition.position;

        }
    }

    public Bounds GetBoundsHome(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                BlueGoalBounds.center = BlueGoalPosition.position;  
                return BlueGoalBounds ;
            case TeamFlag.Red:
                RedGoalBounds.center = RedGoalPosition.position;
                return RedGoalBounds;
            default:
                return BlueGoalBounds;

        }
    }

    public Bounds GetBoundsAway(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                RedGoalBounds.center = RedGoalPosition.position;
                return RedGoalBounds;
              
            case TeamFlag.Red:
                BlueGoalBounds.center = BlueGoalPosition.position;
                return BlueGoalBounds;
            default:
                return BlueGoalBounds;

        }
    }
    public Vector3 GetGoalPositionAway(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                return RedGoalPosition.position;
                
            case TeamFlag.Red:
                return BlueGoalPosition.position;
            default:
                return BlueGoalPosition.position;

        }
    }

    public Goal GetEnemyGoalInstance( TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Blue:
                return RedGoal;

            case TeamFlag.Red:
                return BlueGoal;
            default:
                return BlueGoal;

        }

    }

    [SerializeField]
    private LayerMask blueTeamLayerMask;

    [SerializeField]
    private LayerMask redTeamLayerMask;

    public LayerMask GetLayerMaskOfEnemy(TeamFlag teamFlag)
    {
        switch(teamFlag)
        {
            case TeamFlag.Red:
                return blueTeamLayerMask;
            case TeamFlag.Blue:
                return redTeamLayerMask;
            default:
            return blueTeamLayerMask;


        }

    }

    public LayerMask GetLayerMaskOfTeamMate(TeamFlag teamFlag)
    {
        switch (teamFlag)
        {
            case TeamFlag.Red:
                return redTeamLayerMask;
              
            case TeamFlag.Blue:
                return blueTeamLayerMask;
            default:
                return blueTeamLayerMask;


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

