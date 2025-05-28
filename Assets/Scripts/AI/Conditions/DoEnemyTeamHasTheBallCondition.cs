using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "DoEnemyTeamHasTheBall", story: "[Player] 's team does not have the ball and enemy team has the ball.", category: "FootballPlayer/Conditions", id: "07e8c805be660d0511878fc1246ad962")]
public partial class DoEnemyTeamHasTheBallCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        if (FootballSim.Football.Football.Instance.CurrentOwnerPlayer != null) {
            if (FootballSim.Football.Football.Instance.CurrentOwnerPlayer.TeamFlag != Player.Value.TeamFlag)
            {
                return true;
            }
            else
            {
                return false;
            }
        } else {
            return false;
        }
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
