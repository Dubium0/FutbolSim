using BT_Implementation;
using BT_Implementation.Control;
using BT_Implementation.Leaf;
using UnityEngine;

public class GoalkeeperAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();

    public GoalkeeperAIFacade(Blackboard blackBoard) : base(blackBoard)
    {
    }

    public override void ConstructBT()
    {
        SelectorNode goalkeeperSelector = new SelectorNode("Goalkeeper AI Selector");

        SequenceNode ballInDangerousAreaSequence = new SequenceNode("Ball in Dangerous Area Sequence");
        goalkeeperSelector.AddChild(ballInDangerousAreaSequence);

        ConditionNode isBallNearGoal = new ConditionNode("Is Ball Near Goal", blackBoard, blackBoard =>
        {
            var goal = GameManager.Instance.GetGoalPositionHome(blackBoard.GetValue<TeamFlag>("Team Flag"));
            var ballPosition = Football.Instance.transform.position;
            float dangerRadius = 10f;
            return Vector3.Distance(goal, ballPosition) < dangerRadius;
        });
        ballInDangerousAreaSequence.AddChild(isBallNearGoal);

        SelectorNode dangerResponseSelector = new SelectorNode("Danger Response Selector");
        ballInDangerousAreaSequence.AddChild(dangerResponseSelector);

        SequenceNode blockBallSequence = new SequenceNode("Block Ball Sequence");
        dangerResponseSelector.AddChild(blockBallSequence);

        ConditionNode canBlockBall = new ConditionNode("Can Block Ball", blackBoard, blackBoard =>
        {
            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            var ballPosition = Football.Instance.transform.position;
            return Vector3.Distance(agent.Transform.position, ballPosition) < agent.AgentInfo.MaxRunSpeed;
        });
        blockBallSequence.AddChild(canBlockBall);
        blockBallSequence.AddChild(new SaveTheBall("Save the Ball", blackBoard));

        dangerResponseSelector.AddChild(new ClearTheBall("Clear the Ball", blackBoard));

        SequenceNode goToHomeSequence = new SequenceNode("Go To Home Position Sequence");
        goalkeeperSelector.AddChild(goToHomeSequence);

        ConditionNode shouldGoHome = new ConditionNode("Should Go Home", blackBoard, blackBoard =>
        {
            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            var homePosition = GameManager.Instance.GetGoalPositionHome(agent.TeamFlag);
            return Vector3.Distance(agent.Transform.position, homePosition) > 1;
        });
        goToHomeSequence.AddChild(shouldGoHome);
        goToHomeSequence.AddChild(new GoToTheHomePosition("Go to Home Position", blackBoard));

        entryPoint = goalkeeperSelector;
    }

    public override void ExecuteBT()
    {
        entryPoint.Execute();
    }
}
