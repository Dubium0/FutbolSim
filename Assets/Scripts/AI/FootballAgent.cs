using BT_Implementation;
using System;
using Unity.VisualScripting;
using UnityEngine;

public enum PlayerType
{
    GoalKeeper,
    Defender,
    Midfielder,
    Forward
}


/// <summary>
/// This class is responsible for providing necessery functions to operate the football agent.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FootballAgent : MonoBehaviour
{
    [SerializeField]
    private FootballAgentInfo agentInfo_;
    public FootballAgentInfo AgentInfo
    {
        get { return agentInfo_; }
    }

    public bool EnableAI = true;
    public  PlayerType PlayerType;
    private BTRoot btRoot_;
    private IAbstractBlackboardFactory blackboardFactory_;


    #region Unity Properties
    [HideInInspector]
    public Rigidbody Rigidbody;

    #endregion


    #region Unity Editor Functions

    private void OnValidate()
    {
        InitAISystems();
    }
    #endregion

    #region Unity Runtime Functions

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();

        InitAISystems();
    }

    private void FixedUpdate()
    {
        TickAISystem();
    }
    #endregion

    private void InitAISystems()
    {
        switch (PlayerType)
        {
            case PlayerType.GoalKeeper:
                break;
            case PlayerType.Defender:
                blackboardFactory_ = new DefenseAiBlackboardFactory(this);
                btRoot_ = new DefenseAIFacade(blackboardFactory_.GetBlackboard());
                btRoot_.ConstructBT();
                break;
            case PlayerType.Midfielder:
                break;
            case PlayerType.Forward:
                break;
        }

    }

    private void TickAISystem()
    {
        if (EnableAI)
        {
            btRoot_.ExecuteBT();
        }

    }
}
