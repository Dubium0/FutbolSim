using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "DoOwnerTeamHasTheBall", story: "Do [Player] 's team has the ball.", category: "FootballPlayer/Conditions", id: "9d3024734f94af5a514b8940c3abac86")]
public partial class DoOwnerTeamHasTheBallCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return Player.Value.OwnerTeam.CurrentBallOwnerPlayer != null;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
