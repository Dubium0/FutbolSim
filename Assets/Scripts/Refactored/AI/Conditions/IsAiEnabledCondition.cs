using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsAIEnabled", story: "Is [Player] AI enabled and ticking?", category: "FootballPlayer/Conditions", id: "07fd206182d3527a4dec14f42edfd4b2")]
public partial class IsAiEnabledCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return !Player.Value.IsHumanControlled && Player.Value.IsAITicking && Player.Value.IsHost; //only tick on host
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
