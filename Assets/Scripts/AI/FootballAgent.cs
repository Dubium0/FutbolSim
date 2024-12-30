
using UnityEngine;
/// <summary>
/// This class is responsible for providing necessery functions to operate the football agent.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class FootballAgent : MonoBehaviour {
  
    private FootballAgentInfo agentInfo_;
    public FootballAgentInfo AgentInfo
    {
        get { return agentInfo_; }
    }


    #region Unity Properties
    public Rigidbody Rigidbody;

    #endregion


    #region Unity Functions

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    #endregion




}

