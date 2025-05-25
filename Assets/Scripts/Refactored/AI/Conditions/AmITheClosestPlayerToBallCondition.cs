using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "AmITheClosestPlayerToBall", story: "Is [Player] closest player to ball?", category: "FootballPlayer/Conditions", id: "cb11b0943250f8a7f569d55febd941c4")]
public partial class AmITheClosestPlayerToBallCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return Player.Value.OwnerTeam.ClosestPlayerToBall == Player.Value;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
