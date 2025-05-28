using System;
using Unity.Behavior;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsSantra", story: "Is match state santra", category: "FootballPlayer/Conditions", id: "24d815bc57b467950ba25de10b9a26ca")]
public partial class IsSantraCondition : Condition
{

    public override bool IsTrue()
    {
        return FootballSim.MatchManager.Instance.CurrentMatchState == FootballSim.MatchState.Santra;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
