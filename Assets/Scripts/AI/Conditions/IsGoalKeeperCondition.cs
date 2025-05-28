using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsGoalKeeper", story: "Is [Player] agent goalkeeper?", category: "FootballPlayer/Conditions", id: "845d98e3f31344d23f2c055f9f9e16d9")]
public partial class IsGoalKeeperCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        return Player.Value.PlayerType == FootballSim.Player.PlayerType.GoalKeeper;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
