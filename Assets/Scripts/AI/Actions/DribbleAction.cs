using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Steamworks;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Dribble", story: "[Player] dribbles to next available position.", category: "FootballPlayer/Action", id: "8e96fc841bb2092e3ce591d734c060b6")]
public partial class DribbleAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        var playerSTeam = Player.Value.TeamFlag;
        Bounds enemyGoalBounds;
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
                return Status.Failure; 
        }

     
        var layerMaskToCheck = 1 << (Player.Value.TeamFlag == FootballSim.FootballTeam.TeamFlag.Home ?
        FootballPlayer.AwayLayerMask : FootballPlayer.HomeLayerMask);

        var currentDir = (enemyGoalBounds.center - Player.Value.Transform.position).normalized;

        Vector3 finalDir = currentDir;
        for (int i = 1; i <= 12; i++)
        {
            if (!Physics.Raycast(Player.Value.transform.position, currentDir, 5, layerMaskToCheck))
            {
                finalDir = currentDir;
                break;
            }
            var rotation = Quaternion.AngleAxis(i * 30, Vector3.up);
            currentDir = rotation * currentDir;
        }

        finalDir.y = 0;
        var inputVector = finalDir;

        Player.Value.Rigidbody.linearVelocity = inputVector * Player.Value.Data.MaxRunSpeed;

        if (inputVector.magnitude > 0)
        {
            Player.Value.transform.forward = FootballSim.Utility.MathExtra.MoveTowards( Player.Value.transform.forward, inputVector, 1 / Player.Value.Data.RotationTime);
        }
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

