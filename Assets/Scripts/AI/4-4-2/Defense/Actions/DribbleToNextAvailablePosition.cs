


using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class DribbleTheNextAvailablePosition : ActionNode
{
    public DribbleTheNextAvailablePosition(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }
    
    public override BTResult Execute()
    {

        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var goalPosition = GameManager.Instance.GetGoalPositionAway(agent.TeamFlag);
        //decide where to move
        var layerMask = GameManager.Instance.GetLayerMaskOfEnemy(agent.TeamFlag);

        var currentDir = (goalPosition - agent.Transform.position).normalized;

        Vector3 finalDir = currentDir;
        for (int i  =  1; i <= 12; i++)
        {
            if(!Physics.Raycast(agent.Transform.position, currentDir, 5, layerMask))
            {
                finalDir = currentDir;  
                break;
            }
            var rotation = Quaternion.AngleAxis(i * 30, Vector3.up);
            currentDir = rotation * currentDir;
        }

        finalDir.y = 0;
        var inputVector = finalDir;

        agent.Rigidbody.linearVelocity = inputVector * agent.AgentInfo.MaxRunSpeed;

        if (inputVector.magnitude > 0)
        {
            agent.Transform.forward = MathExtra.MoveTowards(agent.Transform.forward, inputVector, 1 / agent.AgentInfo.RotationTime);
        }
        return BTResult.Success;

    }
}