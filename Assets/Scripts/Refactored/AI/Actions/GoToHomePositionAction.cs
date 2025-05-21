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
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

