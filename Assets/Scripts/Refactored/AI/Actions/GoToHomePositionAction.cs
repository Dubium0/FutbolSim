using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "GoToHomePosition", story: "[Player] goes to home position", category: "FootballPlayer/Action", id: "463224d5db85ac7c3412049191535b86")]
public partial class GoToHomePositionAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        
        var distance = Player.Value.CurrentHomePosition.position - Player.Value.transform.position;
        
        var distanceSwitch = distance.sqrMagnitude > 0.1f ? 1 : 0;
        Player.Value.Rigidbody.AddForce(distance.normalized * Player.Value.Data.RunningAcceleration * distanceSwitch, ForceMode.Acceleration);

       
        var distanceToBall =  FootballSim.Football.Football.Instance.transform.position - Player.Value.transform.position;
        distanceToBall.y = 0;
        if (distanceToBall.sqrMagnitude > 0.0f)
        {
            Debug.DrawRay(Player.Value.transform.position, distanceToBall);
            Player.Value.Transform.forward = distanceToBall.normalized;
        }
       
       
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

