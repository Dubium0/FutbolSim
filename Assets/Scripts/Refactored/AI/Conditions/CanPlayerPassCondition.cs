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

        foreach (var player in Player.Value.OwnerTeam.FootballPlayers)
        {
            if (player != Player.Value)
            {
                var distanceVector = player.transform.position - Player.Value.transform.position;
                if (!Physics.Raycast(Player.Value.transform.position,distanceVector.normalized,distanceVector.sqrMagnitude,layerMaskToCheck))
                {
                    PassDirectionCandidates.Value.Add(distanceVector);
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
