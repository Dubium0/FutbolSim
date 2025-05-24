using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CanPlayerPass", story: "Can [Player] Pass", category: "FootballPlayer/Conditions", id: "2d3d22b3d752e441dd73b2170d0a3f51")]
public partial class CanPlayerPassCondition : Condition
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
