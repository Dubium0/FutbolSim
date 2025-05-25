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
        var distance = FootballSim.Football.Football.Instance.CurrentOwnerPlayer.transform.position - Player.Value.transform.position;
           
        return distance.sqrMagnitude <=  Player.Value.Data.AggressiveDefenseDistance ;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
