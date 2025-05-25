using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsMatchStateOut", story: "[Player] handles out if true", category: "FootballPlayer/Conditions", id: "f6f45adc7844bbfeaf46a9c7352dc650")]
public partial class IsMatchStateOutCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return FootballSim.MatchManager.Instance.CurrentMatchState == FootballSim.MatchState.Out;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
