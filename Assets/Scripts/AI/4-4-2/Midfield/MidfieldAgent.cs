
using BT_Implementation;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MidfieldAgent : MonoBehaviour, IFootballAgent
{
    private const PlayerType PlayerType_ = PlayerType.Forward;

    [SerializeField]
    private FootballAgentInfo agentInfo_;

    private Rigidbody rigidbody_;

    private bool isInitialized_ = false;

    [HideInInspector]
    public FootballAgentInfo AgentInfo => agentInfo_;
    [HideInInspector]
    public PlayerType PlayerType => PlayerType_;
    [HideInInspector]
    public Rigidbody Rigidbody => rigidbody_;

    public bool IsInitialized => isInitialized_;

    public Transform Transform => transform;

    private BTRoot btRoot_;


    private void Awake()
    {
        rigidbody_ = GetComponent<Rigidbody>();
        
    }

    private void FixedUpdate()
    {
        TickAISystem();
    }

    public void InitAISystems(FootballTeam team, int index)
    {
        
        var blackboardFactory = new DefenseAiBlackboardFactory(this,team,index);
        btRoot_ = new DefenseAIFacade(blackboardFactory.GetBlackboard());
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
}

