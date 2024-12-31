
using BT_Implementation;
using Player.Controller;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DefenseAgent : MonoBehaviour, IFootballAgent
{
    private PlayerType playerType_;

    [SerializeField]
    private FootballAgentInfo agentInfo_;

    private Rigidbody rigidbody_;

    private bool isInitialized_ = false;

    [HideInInspector]
    public FootballAgentInfo AgentInfo => agentInfo_;
    [HideInInspector]
    public PlayerType PlayerType => playerType_;
    [HideInInspector]
    public Rigidbody Rigidbody => rigidbody_;

    public bool IsInitialized => isInitialized_;

    public Transform Transform => transform;

    private TeamFlag teamFlag_;
    public TeamFlag TeamFlag => teamFlag_;

    [SerializeField]
    private Transform focusPointTransform_;
    public Transform FocusPointTransform => focusPointTransform_;

    private BTRoot btRoot_;


    private void Awake()
    {
        rigidbody_ = GetComponent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        TickAISystem();
    }

    public void InitAISystems(FootballTeam team, PlayerType playerType,int index)
    {
        playerType_ = playerType;
        teamFlag_ = team.TeamFlag;
        var blackboardFactory = new FootballAiBlackboardFactory(this,team,index);

        switch (playerType_)
        {
            case PlayerType.GoalKeeper:
                break;
            case PlayerType.Defender:
                btRoot_ = new DefenseAIFacade(blackboardFactory.GetBlackboard());
                break;
            case PlayerType.Midfielder:
                btRoot_ = new DefenseAIFacade(blackboardFactory.GetBlackboard());
                break;
            case PlayerType.Forward:
                btRoot_ = new DefenseAIFacade(blackboardFactory.GetBlackboard());
                break;

        }
      
        btRoot_.ConstructBT();

        isInitialized_ = true;

    }

    public void TickAISystem()
    {
        if(!isInitialized_)
        {
            return;
        }
        btRoot_.ExecuteBT();


    }

    private int currentBallAcqusitionStamina_;
    public int TryToAcquireBall()
    {
        if (currentBallAcqusitionStamina_ - agentInfo_.BallAcqusitionStaminaReductionRate > 0)
        {
            currentBallAcqusitionStamina_ -= agentInfo_.BallAcqusitionStaminaReductionRate;

        }
        else
        {
            currentBallAcqusitionStamina_ = 0;
        }
        return (currentBallAcqusitionStamina_ / agentInfo_.MaxBallAcqusitionStamina) * agentInfo_.BallAcqusitionPoint;
    }
}

