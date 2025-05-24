using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "DoPlayerHasTheBall", story: "Do [Player] has the ball.", category: "FootballPlayer/Conditions", id: "d4d32fa99ae4d65371b6dcf1f3c0ed61")]
public partial class DoPlayerHasTheBallCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return Player.Value.IsTheOwnerOfTheBall;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
