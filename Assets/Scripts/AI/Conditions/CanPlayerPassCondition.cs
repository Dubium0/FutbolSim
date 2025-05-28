using FootballSim.Player;
using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CanPlayerPass", story: "Can [Player] Pass to any [PassDirectionCandidates]", category: "FootballPlayer/Conditions", id: "2d3d22b3d752e441dd73b2170d0a3f51")]
public partial class CanPlayerPassCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<List<Vector3>> PassDirectionCandidates;
    public override bool IsTrue()
    {
        PassDirectionCandidates.Value.Clear();
        var layerMaskToCheck = 1 << (Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Home ?
        FootballPlayer.AwayLayerMask : FootballPlayer.HomeLayerMask);

        Bounds enemyGoalBounds;

        var playerSTeam = Player.Value.TeamFlag;
        switch (playerSTeam)
        {
            case FootballSim.FootballTeam.TeamFlag.Home:
                enemyGoalBounds = FootballSim.MatchManager.Instance.PitchData.AwayGoalBounds;
                break;
            case FootballSim.FootballTeam.TeamFlag.Away:
                enemyGoalBounds = FootballSim.MatchManager.Instance.PitchData.HomeGoalBounds;
                break;
            default:
                Debug.LogError("This should not happen");
                return false;

        }
        var playerDistanceToEnemyGoal = enemyGoalBounds.center - Player.Value.transform.position;
        foreach (var player in Player.Value.OwnerTeam.FootballPlayers)
        {
            if (player != Player.Value)
            {
                var distanceVector = player.transform.position - Player.Value.transform.position;

                var distanceToEnemyGoal = enemyGoalBounds.center - player.transform.position;

                if (distanceToEnemyGoal.sqrMagnitude < playerDistanceToEnemyGoal.sqrMagnitude + 4)
                {
                    if (!Physics.Raycast(Player.Value.transform.position, distanceVector.normalized, distanceVector.sqrMagnitude, layerMaskToCheck))
                    {
                        Debug.DrawRay(Player.Value.transform.position, distanceVector, Color.cyan);
                        PassDirectionCandidates.Value.Add(distanceVector.normalized);
                    }
                }
            }

        }

        return PassDirectionCandidates.Value.Count > 0;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
