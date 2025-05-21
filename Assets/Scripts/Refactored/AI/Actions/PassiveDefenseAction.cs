using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PassiveDefense", story: "[Player] passively defenses", category: "FootballPlayer/Action", id: "c8df348da43391d6ad8a67535a446207")]
public partial class PassiveDefenseAction : Action
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

