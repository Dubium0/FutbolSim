using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CanIShoot", story: "Can [Player] shoot?", category: "FootballPlayer/Conditions", id: "c0a110a3566eac0842072e9282ff92aa")]
public partial class CanIShootCondition : Condition
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
