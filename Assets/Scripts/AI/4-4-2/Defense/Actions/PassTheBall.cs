

using BT_Implementation;
using BT_Implementation.Leaf;
using System.Collections.Generic;


public class PassTheBall : ActionNode
{
    public PassTheBall(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {
        var passCandidates =blackBoard.GetValue<List<IFootballAgent>>("Pass Candidates");

        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");

        int maxValue = -1;
        IFootballAgent footballAgentToPass = null;
        foreach(var passCandidate in passCandidates)
        {
            var shootScore = passCandidate.GetShootScore();
            if(shootScore > maxValue)
            {
                maxValue = shootScore;
                footballAgentToPass = passCandidate;
            }

        }

        var direction = footballAgentToPass.Transform.position - agent.Transform.position;


        Football.Instance.HitBall(direction.normalized, agent.AgentInfo.MaximumPassPower);

        agent.DisableAIForATime(0.2f);

        return BTResult.Success;

    }
}