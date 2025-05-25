using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

using Unity.Mathematics;


[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ShootTheBall", story: "[Player] shoots to any [ShootDirectionCandidates] ", category: "FootballPlayer/Action", id: "70c6644a20009945af1a2396d9f4b719")]
public partial class ShootTheBallAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<List<Vector3>> ShootDirectionCandidates;

    private bool m_IsAlreadyShooted = false;
    private Vector3 m_MissOffset = new Vector3(1, 1, 1).normalized;
    protected override Status OnStart()
    {
        
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (!m_IsAlreadyShooted)
        {
                
            Player.Value.SetKickBallTrigger();

            Player.Value.FootballPlayerAnimation.OnBallTouchEvent += () =>
            {
                var shootDir = DecideShootDirection();
                var shootPower = DecideShootPower();
                FootballSim.Football.Football.Instance.HitBall(
                        shootDir,
                        shootPower,
                        Player.Value);
                m_IsAlreadyShooted = false;
            };
            m_IsAlreadyShooted = true;
        }
        return Status.Success;
    }
    private Vector3 DecideShootDirection()
    {
        int maxIndex = ShootDirectionCandidates.Value.Count - 1;

        int randomInt = UnityEngine.Random.Range(0, maxIndex);

        var chosenCandidate = ShootDirectionCandidates.Value[randomInt];

        var probabilisticMiss =  UnityEngine.Random.Range(1, 4) > 1;   // 1 out of 4 shoots misses
        if (probabilisticMiss) {

            chosenCandidate += m_MissOffset;
        }
        return chosenCandidate;
    }

    private float DecideShootPower()
    {
        float finalPower = 0;
        var maxPower = Player.Value.Data.MaximumShootPower;
        
        int randomInt = UnityEngine.Random.Range(0, 10);

        finalPower = math.lerp(Player.Value.Data.MaximumShootPower / 5, maxPower, randomInt / 10.0f);
        
        return finalPower;
    }
    protected override void OnEnd()
    {
    }
}

