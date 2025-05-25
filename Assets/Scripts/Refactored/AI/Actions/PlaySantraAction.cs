using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlaySantra", story: "[Player] starts the game", category: "FootballPlayer/Action", id: "a373f0d1151d47232fb3d5c5ce05c1b6")]
public partial class PlaySantraAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    private bool m_IsAlreadyStarted = false;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!m_IsAlreadyStarted)
        {

            Player.Value.StartCoroutine(StartSantraAction());
        }
        return Status.Success;
    }
    private IEnumerator StartSantraAction()
    {
        Debug.Log("I am starting santra ");
        yield return new WaitForSeconds(1.0f);
        if (FootballSim.MatchManager.Instance.CurrentMatchState == FootballSim.MatchState.Santra && !m_IsAlreadyStarted)
        {
            
            Player.Value.SetKickBallTrigger();

            Player.Value.FootballPlayerAnimation.OnBallTouchEvent += () =>
            {
                FootballSim.MatchManager.Instance.OnMatchStarted += ReturnToDefault;
                FootballSim.MatchManager.Instance.OnMatchResumed += ReturnToDefault;
                FootballSim.Football.Football.Instance.HitBall(
                        Player.Value.transform.forward,
                        Player.Value.Data.MaximumPassPower / 2.0f,
                        Player.Value);
            };
            m_IsAlreadyStarted = true;
        }
    }
    private void ReturnToDefault()
    {
        m_IsAlreadyStarted = false;
        FootballSim.MatchManager.Instance.OnMatchStarted -= ReturnToDefault;
        FootballSim.MatchManager.Instance.OnMatchResumed -= ReturnToDefault;
    }

    protected override void OnEnd()
    {
    }
}

