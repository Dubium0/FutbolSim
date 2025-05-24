using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ShootTheBall", story: "[Player] shoots", category: "FootballPlayer/Action", id: "70c6644a20009945af1a2396d9f4b719")]
public partial class ShootTheBallAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

