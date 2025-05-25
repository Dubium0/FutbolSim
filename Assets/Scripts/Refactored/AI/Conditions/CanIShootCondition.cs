using FootballSim.Player;
using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "CanIShoot", story: "Can [Player] shoot to any [ShootDirectionCandidates] ?", category: "FootballPlayer/Conditions", id: "c0a110a3566eac0842072e9282ff92aa")]
public partial class CanIShootCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<List<Vector3>> ShootDirectionCandidates;

    public override bool IsTrue()
    {
        ShootDirectionCandidates.Value.Clear();
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
                enemyGoalBounds = FootballSim.MatchManager.Instance.PitchData.HomeGoalBounds;
                Debug.LogError("This should not happen");
                break;

        }
        var distance = enemyGoalBounds.SqrDistance(Player.Value.transform.position);

        // strategy here is throwing rays like 10 by 6 grid.
        // then getting candidate shootable positions
        // but of course first check if it is in shootable distance
        var layerMaskToCheck = 1 << (Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Home ?
        FootballPlayer.AwayLayerMask : FootballPlayer.HomeLayerMask);
        if (distance >= Player.Value.Data.MinimumShootDistance)
        {
            float stepSizeX = enemyGoalBounds.extents.x / 5.0f;
            float stepSizeY = enemyGoalBounds.extents.y / 3.0f;
            float z = enemyGoalBounds.center.z;
            for (float x = -enemyGoalBounds.extents.x + enemyGoalBounds.center.x; x < enemyGoalBounds.extents.x + enemyGoalBounds.center.x; x += stepSizeX)
            {
                for (float y = -enemyGoalBounds.extents.y + enemyGoalBounds.center.y; x < enemyGoalBounds.extents.y + enemyGoalBounds.center.y; y += stepSizeY)
                {
                    var distanceVector = new Vector3(x, y, z) - Player.Value.transform.position;
                    Debug.DrawRay(Player.Value.transform.position, distanceVector,Color.blue);
                    if (!Physics.Raycast(Player.Value.transform.position, distanceVector.normalized, distanceVector.sqrMagnitude, layerMaskToCheck))
                    {
                        ShootDirectionCandidates.Value.Add(distanceVector.normalized);
                    }
                }

            }

        }
        return ShootDirectionCandidates.Value.Count > 0;


    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
