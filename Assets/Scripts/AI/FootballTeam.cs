
using Player.Controller.States;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public enum TeamFlag
{
    Red,
    Blue
}

public enum FormationPhase
{
    Defense,
    Start,
    Default,
    Attack
}
public class FootballTeam : MonoBehaviour
{
    [SerializeField]
    private bool isHumanControllable = false;

    public TeamFlag TeamFlag;
    public List<IFootballAgent> FootballAgents = new(10);
    public IFootballAgent GoalKeeper;

    private List<Transform> homePositions_ = new(11);

    public FootballFormation DefenseFormation;
    public FootballFormation StartFormation;
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

    private IFootballAgent currentBallOwnerTeamMate = null;

    public IFootballAgent CurrentBallOwnerTeamMate { get { return currentBallOwnerTeamMate; } }

    private FormationPhase currentFormationPhase;
    public FormationPhase CurrentFormationPhase => currentFormationPhase;
    private bool isOnStart = true;
    private void Awake()
    {
        currentFormation = StartFormation;
        CreateAgents();
    }

    private void FixedUpdate()
    {
        SetClosestPlayerToBall(); 
       
        DecideStrategy();
    }
    private void Update()
    {
        CycleToClosestPlayer();
    }
    private void SetClosestPlayerToBall()
    {
        //can tick this for 1 second etc
        var ballPosition = Football.Instance.transform.position;
        ballPosition.y = 0;
        Vector3 minDistance = Vector3.one * 9999;
        IFootballAgent minDistancePlayer = null;
        FootballAgents.ForEach(agent => 
        {

            var distance = ballPosition - agent.Transform.position;
            if (distance.magnitude < minDistance.magnitude)
            {
                minDistance = distance;
                minDistancePlayer = agent;
            }
        }
        
        );
        var prevClosest = closestPlayerToBall_;
      
        closestPlayerToBall_ = minDistancePlayer;
        if (isHumanControllable && prevClosest != minDistancePlayer && isOnStart)
        {
            isOnStart = false;
            playerControlledAgent = closestPlayerToBall_;
            playerControlledAgent.SetAsHumanControlled();
        }


    }

    private void CycleToClosestPlayer()
    {
        if(Input.GetKeyDown(KeyCode.C) && isHumanControllable) {
            var prevAgent = playerControlledAgent;
            if (prevAgent != null)
            {
                prevAgent.SetAsAIControlled();
            }
            playerControlledAgent = closestPlayerToBall_;
            playerControlledAgent.SetAsHumanControlled();

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
    private void CreateAgents()
    {
        var defCount = currentFormation.DefensePosition.Length;
        var midfieldCount = currentFormation.MidfieldPosition.Length;
        var attackCount = currentFormation.ForwardPosition.Length;

        int index = 0;


        int layerToSet = TeamFlag == TeamFlag.Red ? 10 : 9;
        
        GameObject goalkeeper = Instantiate(GoalKeeperAgentPrefab, currentFormation.GoalKeeperPosition.position, currentFormation.GoalKeeperPosition.rotation);
        goalkeeper.layer = layerToSet;
        var goalkeeperComponent = goalkeeper.GetComponent<IFootballAgent>();
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
            agent.layer = layerToSet;
          
            var agentComponent = agent.GetComponent<IFootballAgent>();
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
            FootballAgents.Insert(index, agentComponent);
            homePositions_.Insert(index, currentFormation.DefensePosition[i]);
            index++;
        }

        for (var i = 0; i < midfieldCount; i++)
        {
            GameObject agent = Instantiate(MidfieldAgentPrefab, currentFormation.MidfieldPosition[i].position, currentFormation.MidfieldPosition[i].rotation);
          
            agent.layer = layerToSet;
            var agentComponent = agent.GetComponent<IFootballAgent>();
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
            FootballAgents.Insert(index, agentComponent);
            homePositions_.Insert(index, currentFormation.MidfieldPosition[i]);
            index++;
        }

        for (var i = 0; i < attackCount; i++)
        {
            GameObject agent = Instantiate(ForwardAgentPrefab, currentFormation.ForwardPosition[i].position, currentFormation.ForwardPosition[i].rotation);
            agent.layer = layerToSet;
           
            var agentComponent = agent.GetComponent<IFootballAgent>();
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
            case TeamFlag.Blue:
                return PicthZone.BlueZone;
            case TeamFlag.Red: 
                return PicthZone.RedZone;
            default:
                return PicthZone.BlueZone;
        }
    }

    
}
