using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using Unity.Mathematics;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PassTheBall", story: "[Player] passes to [PassDirectionCandidates]", category: "FootballPlayer/Action", id: "4a31dc54deba2e9e6146e287ca40462b")]
public partial class PassTheBallAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<List<Vector3>> PassDirectionCandidates;

    private bool m_IsAlreadyPassed = false;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
          if (!m_IsAlreadyPassed)
        {
                
            Player.Value.SetKickBallTrigger();
            var shootDir = DecidePassDirection();
            var shootPower = DecidePassPower();
            Player.Value.FootballPlayerAnimation.OnBallTouchEvent += () =>
            {
                
                FootballSim.Football.Football.Instance.HitBall(
                        shootDir,
                        shootPower,
                        Player.Value);
                m_IsAlreadyPassed = false;
            };
            m_IsAlreadyPassed = true;
        }
        return Status.Success;
    }

    private float DecidePassPower()
    {
        float finalPower;
        var maxPower = Player.Value.Data.MaximumPassPower;
        
        int randomInt = UnityEngine.Random.Range(0, 10);

        finalPower = math.lerp(Player.Value.Data.MaximumPassPower / 5, maxPower, randomInt / 10.0f);
        
        return finalPower;
    }

    private Vector3 DecidePassDirection()
    {
        int maxIndex = PassDirectionCandidates.Value.Count - 1;

        int randomInt = UnityEngine.Random.Range(0, maxIndex);

        var chosenCandidate = PassDirectionCandidates.Value[randomInt];

        return chosenCandidate;
    }

    protected override void OnEnd()
    {
    }
}

