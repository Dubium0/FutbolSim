using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PassTheBall", story: "[Player] passes to [PassDirectionCandidates]", category: "FootballPlayer/Action", id: "4a31dc54deba2e9e6146e287ca40462b")]
public partial class PassTheBallAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;
    [SerializeReference] public BlackboardVariable<List<Vector3>> PassDirectionCandidates;

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

