using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Dribbling", story: "[Player] dribblings", category: "Player/Dribbling", id: "d40a1885e16ebd0b068ecd7c2125ee8e")]
public partial class DribblingAction : Action
{
    [SerializeReference] public BlackboardVariable<GenericAgent> Player;

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

