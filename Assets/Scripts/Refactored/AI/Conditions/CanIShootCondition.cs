using FootballSim.Player;
using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Mathematics;
using Unity.VisualScripting;
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
                Debug.LogError("This should not happen");
                return false;

        }
        var distance = enemyGoalBounds.center - Player.Value.transform.position;
       // Debug.Log(enemyGoalBounds.extents  + " and " + enemyGoalBounds.center );
        // strategy here is throwing rays like 10 by 6 grid.
        // then getting candidate shootable positions
        // but of course first check if it is in shootable distance
        var layerMaskToCheck = 1 << (Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Home ?
        FootballPlayer.AwayLayerMask : FootballPlayer.HomeLayerMask);
        if (distance.sqrMagnitude <= Player.Value.Data.MinimumShootDistance)
        {
            
            var zLeftExtend = enemyGoalBounds.center.z - enemyGoalBounds.extents.z;
            var zRightExtend = enemyGoalBounds.center.z + enemyGoalBounds.extents.z;

            var minZ = math.min(zLeftExtend, zRightExtend);
            var maxZ = math.max(zLeftExtend, zRightExtend);

            var zStepValue = (math.abs(zLeftExtend) + math.abs(zRightExtend))/10.0f;
           
            var yLeftExtend = enemyGoalBounds.center.y - enemyGoalBounds.extents.y;
            var yRightExtend = enemyGoalBounds.center.y + enemyGoalBounds.extents.y;
            var minY = math.min(yLeftExtend, yRightExtend);
            var maxY = math.max(yLeftExtend, yRightExtend);
            var yStepValue = (math.abs(yLeftExtend) + math.abs(yRightExtend))/5.0f;
           

            for (var z = minZ; z <= maxZ; z += zStepValue)
            {
                for (var y = minY; y <= maxY; y += yStepValue)
                {   

                    var distanceToTargetPos = new Vector3(enemyGoalBounds.center.x, y, z) - Player.Value.transform.position;
                    if (!Physics.Raycast(Player.Value.transform.position, distanceToTargetPos.normalized, distanceToTargetPos.sqrMagnitude, layerMaskToCheck))
                    {
                        Debug.DrawRay(Player.Value.transform.position, distanceToTargetPos, Color.blue);
                        ShootDirectionCandidates.Value.Add(distanceToTargetPos.normalized);
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
