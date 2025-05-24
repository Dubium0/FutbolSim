using Player.Controller.States;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Android;


public enum TeamFlag
{
    Home = 0,
    Away = 1,
    None = 2
}

public enum FormationPhase
{
    AttackStart,
    DefenseStart,

    Default,
    Defense,
    Attack,
}
public class FootballTeam : NetworkBehaviour
{
    public bool isHumanControllable = false;
    public TeamFlag TeamFlag;
    public List<IFootballAgent> FootballAgents = new(10);
    public IFootballAgent GoalKeeper;

    private List<Transform> homePositions_ = new(11);

    public FootballFormation DefenseFormation;
    public FootballFormation AttackStartFormation;
    public FootballFormation DefenseStartFormation;
    public FootballFormation DefaultFormation;
    public FootballFormation AttackFormation;



    public GameObject DefenseAgentPrefab;
    public GameObject MidfieldAgentPrefab;
    public GameObject ForwardAgentPrefab;
    public GameObject GoalKeeperAgentPrefab;

    private FootballFormation currentFormation;

    private IFootballAgent closestPlayerToBall_ = null;
    public IFootballAgent ClosestPlayerToBall { get { return closestPlayerToBall_; } }

    private IFootballAgent playerControlledAgent = null;
    private List<int> playerIndices_ = new List<int>();

    private IFootballAgent currentBallOwnerTeamMate = null;

    public IFootballAgent CurrentBallOwnerTeamMate { get { return currentBallOwnerTeamMate; } }

    private FormationPhase currentFormationPhase;
    public FormationPhase CurrentFormationPhase => currentFormationPhase;

    private NetworkObject networkObject;
    private bool isOnStart = true;

    private bool isInitialized = false;

 
    private bool isInitWithOwner = false;
    //public void SetPlayerIndices(List<int> indices)
    //{
    //    if (!isInitialized)
    //    {
    //        Debug.Log("[Team Setup] This football team is not yet initialized"); return;
    //        
    //    }
    //    // Debug.Log($"[Team Setup] SetPlayerIndices called with {indices.Count} indices for {TeamFlag} team");
    //    if (indices == null || indices.Count == 0)
    //    {
    //        Debug.LogError("[Team Setup] SetPlayerIndices received null or empty indices list!");
    //        return;
    //    }
//
    //    playerIndices_ = indices;
    //    // Assign player indices to all agents in the team
    //    for (int i = 0; i < FootballAgents.Count; i++)
    //    {
    //        if (FootballAgents[i] is GenericAgent agent)
    //        {
    //            // Simply use the first index for all agents
    //            int playerIndex = indices[0];
    //            // Debug.Log($"[Team Setup] Setting player index {playerIndex} for {TeamFlag} team agent {i}");
    //            agent.SetPlayerIndex(playerIndex);
    //        }
    //    }
    //}

    private void Awake()
    {
        networkObject = GetComponent<NetworkObject>();
    }
    public void init(bool t_isHumanControlled, bool isHome, ulong? ownerId = null,int t_playerIndices = 0)
    {
      
      
        if (ownerId != null && IsServer)
        {

            networkObject.ChangeOwnership((ulong)ownerId);
            isInitWithOwner = true;

            if (IsServer && IsOwner)
            {
                Debug.Log("This football team is ownded by server");

            }
            else { 
            
                NotifyClientItIsTheOwnerRpc();
            }
        }
        Debug.Log("IsClient ? " + IsClient);
        if (IsClient) return;
        isHumanControllable = t_isHumanControlled;
        currentFormation = isHome ?  AttackStartFormation : DefenseStartFormation;
     
      
        CreateAgents(t_playerIndices);
        

        isInitialized = true;
    }

    [Rpc(SendTo.ClientsAndHost)]

    private void NotifyClientItIsTheOwnerRpc()
    {
        if( IsClient  && IsOwner) {
            Debug.Log("This football team is ownded by client");
        }
    }
 
    private void FixedUpdate()
    {
        if (IsClient) return;
        if( isInitialized && GameManager.Instance.GameState == EGameState.Playing)
        {

            SetClosestPlayerToBall(); 
       
            DecideStrategy();
        }
    }
    private void Update()
    {
        if(IsOwner && IsClient)
        {
            if(Input.GetKeyDown(KeyCode.C))
            {
                CycleToClosestPlayerRpc();
            }
            return;
        }
        if (isInitialized && GameManager.Instance.GameState == EGameState.Playing && IsOwner)  CycleToClosestPlayer();

    }
    private void SetClosestPlayerToBall()
    {
        //can tick this for 1 second etc
        var ballPosition = Football.Instance.transform.position;
        ballPosition.y = 0;
        Vector3 minDistance = Vector3.one * 9999;
        IFootballAgent minDistancePlayer = null;

        int chosenPlayerIndex = 0;
        for( int i = 0; i < FootballAgents.Count; i++ )
        {
            var agent  = FootballAgents[i];
            var distance = ballPosition - agent.Transform.position;
            if (distance.magnitude < minDistance.magnitude)
            {
                minDistance = distance;
                minDistancePlayer = agent;
                chosenPlayerIndex = i;
            }
        }
        var prevClosest = closestPlayerToBall_;
      
        closestPlayerToBall_ = minDistancePlayer;
      
        if (isHumanControllable && prevClosest != minDistancePlayer && isOnStart)
        {
            isOnStart = false;
            playerControlledAgent = closestPlayerToBall_;
            playerControlledAgent.SetAsHumanControlled();
           
        }


    }

    [Rpc(SendTo.Server)]
    private void CycleToClosestPlayerRpc()
    {
        
         CycleToClosestPlayerLogic();
        
    }

    private void CycleToClosestPlayerLogic()
    {
        var prevAgent = playerControlledAgent;
        if (prevAgent != null)
        {
            prevAgent.SetAsAIControlled();
        }
        playerControlledAgent = closestPlayerToBall_;
        playerControlledAgent.SetAsHumanControlled();  

      

    }

    private void CycleToClosestPlayer()
    {
        if(Input.GetKeyDown(KeyCode.C) && isHumanControllable) {
            CycleToClosestPlayerLogic();
        }

    }
    private void DecideStrategy()
    {
        if( currentBallOwnerTeamMate == Football.Instance.CurrentOwnerPlayer)
        {
            var sectorNumber = Football.Instance.SectorNumber;
            if (Football.Instance.PitchZone == GetPicthZone())
            {
              
                if(sectorNumber >= 6)
                {
                    currentFormation = DefaultFormation;
                    currentFormationPhase = FormationPhase.Default;
                }
                else
                {
                    currentFormation = DefenseFormation;
                    currentFormationPhase = FormationPhase.Defense;
                }
                

            }
            else
            {
                if(sectorNumber < 6)
                {
                    currentFormation = AttackFormation;
                    currentFormationPhase = FormationPhase.Attack;
                }
            }

        }
        else{
            var sectorNumber = Football.Instance.SectorNumber;
            if (Football.Instance.PitchZone == GetPicthZone())
            {
                

                if (sectorNumber < 6)
                {
                    currentFormation = DefenseFormation;
                    currentFormationPhase = FormationPhase.Defense;
                }
                else
                {
                    currentFormation = DefaultFormation;
                    currentFormationPhase = FormationPhase.Default;
                }


            }
            else
            {
                
                    currentFormation = DefaultFormation;
                currentFormationPhase = FormationPhase.Default;

            }
        }
        UpdateHomePositions();

    }
    private void CreateAgents(int t_playerIndices = 0)
    {

        var defCount = currentFormation.DefensePosition.Length;
        var midfieldCount = currentFormation.MidfieldPosition.Length;
        var attackCount = currentFormation.ForwardPosition.Length;

        int index = 0;


        int layerToSet = TeamFlag == TeamFlag.Home ? 10 : 9;
        
        GameObject goalkeeper = Instantiate(GoalKeeperAgentPrefab, currentFormation.GoalKeeperPosition.position, currentFormation.GoalKeeperPosition.rotation);
        if(IsServer)
        {

        goalkeeper.GetComponent<NetworkObject>().Spawn();
        }
        goalkeeper.layer = layerToSet;
        var goalkeeperComponent = goalkeeper.GetComponent<IFootballAgent>();
        if (isInitWithOwner)
        {
            goalkeeperComponent.init(networkObject.OwnerClientId,t_playerIndices);

        }
        else
        {
            goalkeeperComponent.init(null, t_playerIndices);
        }
      
        goalkeeperComponent.OnBallPossesionCallback = agent => 
        {
            currentBallOwnerTeamMate = agent;
            if (isHumanControllable)
            {
                var prevAgent = playerControlledAgent;
                if (prevAgent != null)
                {
                    prevAgent.SetAsAIControlled();
                }
                playerControlledAgent = agent;
                playerControlledAgent.SetAsHumanControlled();
            }
        };
        goalkeeperComponent.InitAISystems(this, PlayerType.Goalkeeper, index);
        FootballAgents.Insert(index, goalkeeperComponent);
        
        for (var i = 0; i < defCount; i++)
        {
            GameObject agent = Instantiate(DefenseAgentPrefab, currentFormation.DefensePosition[i].position, currentFormation.DefensePosition[i].rotation);
            if (IsServer)  agent.GetComponent<NetworkObject>().Spawn();
            agent.layer = layerToSet;
          
            var agentComponent = agent.GetComponent<GenericAgent>();
           

            if (isInitWithOwner)
            {
                agentComponent.init(networkObject.OwnerClientId,t_playerIndices);

            }
            else
            {
                agentComponent.init(null,t_playerIndices);
            }
            agentComponent.OnBallPossesionCallback = agent => {
                currentBallOwnerTeamMate = agent;
                if (isHumanControllable)
                {
                    var prevAgent = playerControlledAgent;
                    if (prevAgent != null)
                    {
                        prevAgent.SetAsAIControlled();
                    }
                    playerControlledAgent = agent;
                    playerControlledAgent.SetAsHumanControlled();
                }
            };
            agentComponent.InitAISystems(this,PlayerType.Defender, index);
            agentComponent.AssignTeamMaterial(TeamFlag);
            FootballAgents.Insert(index, agentComponent);
            homePositions_.Insert(index, currentFormation.DefensePosition[i]);
            index++;
        }

        for (var i = 0; i < midfieldCount; i++)
        {
            GameObject agent = Instantiate(MidfieldAgentPrefab, currentFormation.MidfieldPosition[i].position, currentFormation.MidfieldPosition[i].rotation);
          
            agent.layer = layerToSet;
            var agentComponent = agent.GetComponent<GenericAgent>();
            if (IsServer) agent.GetComponent<NetworkObject>().Spawn();

            if (isInitWithOwner)
            {
                agentComponent.init(networkObject.OwnerClientId,t_playerIndices);

            }
            else
            {
                agentComponent.init(null,t_playerIndices);
            }
            agentComponent.OnBallPossesionCallback = agent => {
                currentBallOwnerTeamMate = agent;
                if (isHumanControllable)
                {
                    var prevAgent = playerControlledAgent;
                    if (prevAgent != null)
                    {
                        prevAgent.SetAsAIControlled();
                    }
                    playerControlledAgent = agent;
                    playerControlledAgent.SetAsHumanControlled();
                }
            };
            agentComponent.InitAISystems(this, PlayerType.Midfielder, index);
            agentComponent.AssignTeamMaterial(TeamFlag);
            FootballAgents.Insert(index, agentComponent);
            homePositions_.Insert(index, currentFormation.MidfieldPosition[i]);
            index++;
        }

        for (var i = 0; i < attackCount; i++)
        {
            GameObject agent = Instantiate(ForwardAgentPrefab, currentFormation.ForwardPosition[i].position, currentFormation.ForwardPosition[i].rotation);
            if (IsServer) agent.GetComponent<NetworkObject>().Spawn();
            agent.layer = layerToSet;
           
            var agentComponent = agent.GetComponent<GenericAgent>();

            if (isInitWithOwner)
            {
                agentComponent.init(networkObject.OwnerClientId,t_playerIndices);

            }
            else
            {
                agentComponent.init(null,t_playerIndices);
            }
            agentComponent.OnBallPossesionCallback = agent => {
                currentBallOwnerTeamMate = agent;
                if (isHumanControllable)
                {
                    var prevAgent = playerControlledAgent;
                    if(prevAgent != null)
                    {
                        prevAgent.SetAsAIControlled();
                    }
                    playerControlledAgent = agent;
                    playerControlledAgent.SetAsHumanControlled();
                
                }
            };
            agentComponent.InitAISystems(this, PlayerType.Forward, index);
            agentComponent.AssignTeamMaterial(TeamFlag);
            FootballAgents.Insert(index, agentComponent);
            homePositions_.Insert(index, currentFormation.ForwardPosition[i]);
            index++;
        }
    }

    private void UpdateHomePositions()
    {
        var defCount = currentFormation.DefensePosition.Length;
        var midfieldCount = currentFormation.MidfieldPosition.Length;
        var attackCount = currentFormation.ForwardPosition.Length;

        int index = 0;
        for (var i = 0; i < defCount; i++)
        {
         
            homePositions_[index] = currentFormation.DefensePosition[i];
            index++;
        }

        for (var i = 0; i < midfieldCount; i++)
        {
           
            homePositions_[index] = currentFormation.MidfieldPosition[i];
            index++;
        }

        for (var i = 0; i < attackCount; i++)
        {
            homePositions_[index] = currentFormation.ForwardPosition[i];
            index++;
        }
    }

    public Vector3 GetHomePosition(int index)
    {
        if (index < 0 || index >= FootballAgents.Count)
        {
            Debug.LogError("Index out of range");
            return Vector3.zero;
        }
        return homePositions_[index].position;

    }

    public PicthZone GetPicthZone()
    {
        switch(TeamFlag)
        {
            case TeamFlag.Away:
                return PicthZone.AwayZone;
            case TeamFlag.Home: 
                return PicthZone.HomeZone;
            default:
                return PicthZone. AwayZone;
        }
    }

    public void ResetToFormation(FormationPhase formation)
    {
    }
}
