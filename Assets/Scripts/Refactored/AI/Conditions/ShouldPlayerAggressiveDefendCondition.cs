using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "ShouldPlayerAggressiveDefend", story: "Should [Player] aggressively defend", category: "FootballPlayer/Conditions", id: "7244f9240adeb58243e6467878682855")]
public partial class ShouldPlayerAggressiveDefendCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
