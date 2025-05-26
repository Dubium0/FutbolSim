using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsEnemyPlayerInsideTheCriticalRegion", story: "Is enemy of the [Player] inside the critical region.", category: "FootballPlayer/Conditions", id: "644b861c59bd38c2b3e5caf946e4cc92")]
public partial class IsEnemyPlayerInsideTheCriticalRegionCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    public override bool IsTrue()
    {
        var distance = FootballSim.Football.Football.Instance.CurrentOwnerPlayer.transform.position - Player.Value.transform.position;
        var playerSTeam = Player.Value.TeamFlag;
        Transform allyGoalBounds;
        switch (playerSTeam)
        {
            case FootballSim.FootballTeam.TeamFlag.Home:
                allyGoalBounds = FootballSim.MatchManager.Instance.PitchData.HomeGoalTransform;
                break;
            case FootballSim.FootballTeam.TeamFlag.Away:
                allyGoalBounds = FootballSim.MatchManager.Instance.PitchData.AwayGoalTransform;
                break;
            default:
                Debug.LogError("This should not happen");
                return false;

        }
        var distanceToAllyGoalPosition = allyGoalBounds.position - FootballSim.Football.Football.Instance.CurrentOwnerPlayer.transform.position;
     
        return  ((distance.sqrMagnitude <= Player.Value.Data.AggressiveDefenseDistance) ||
        distanceToAllyGoalPosition.sqrMagnitude < Player.Value.Data.MinimumShootDistance ) && Player.Value.CanPossesTheBall;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
