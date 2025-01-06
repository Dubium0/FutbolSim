using BT_Implementation;
using BT_Implementation.Control;
using BT_Implementation.Leaf;
using UnityEngine;

public class GoalkeeperAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();

    public GoalkeeperAIFacade(Blackboard blackBoard) : base(blackBoard) { }

    public override void ConstructBT()
    {
        SelectorNode rootSelector = new SelectorNode("Goalkeeper AI");

        // 1. If the team has the ball, go to the home position
        SequenceNode hasTeamPossession = new SequenceNode("Team Has Possession");
        rootSelector.AddChild(hasTeamPossession);

        ConditionNode doesTeamHaveTheBall = new ConditionNode("Does Team Have the Ball?", blackBoard, bb =>
        {
            var footballTeam = bb.GetValue<FootballTeam>("Owner Team");
            return footballTeam.CurrentBallOwnerTeamMate != null &&
                   footballTeam.CurrentBallOwnerTeamMate.TeamFlag == footballTeam.TeamFlag;
        });
        hasTeamPossession.AddChild(doesTeamHaveTheBall);
        hasTeamPossession.AddChild(new GoToTheGoalPost("Go to Home Position", blackBoard));

        // 2. If the goalkeeper has the ball, clear it (boot it away)
        SequenceNode hasPossession = new SequenceNode("Goalkeeper Has Possession");
        rootSelector.AddChild(hasPossession);

        ConditionNode doesGoalkeeperHaveTheBall = new ConditionNode("Do I Have the Ball?", blackBoard, bb =>
        {
            var agent = bb.GetValue<IFootballAgent>("Owner Agent");
            return Football.Instance.CurrentOwnerPlayer == agent;
        });
        hasPossession.AddChild(doesGoalkeeperHaveTheBall);
        hasPossession.AddChild(new BootTheBall("Boot the Ball", blackBoard));

        // 3. If the ball is nearby, try to block or intercept
        SequenceNode ballIsNearby = new SequenceNode("Ball Is Nearby");
        rootSelector.AddChild(ballIsNearby);

        ConditionNode isBallClose = new ConditionNode("Is Ball Close?", blackBoard, bb =>
        {
            var agent = bb.GetValue<IFootballAgent>("Owner Agent");
            var ballPosition = Football.Instance.transform.position;
            return Vector3.Distance(agent.Transform.position, ballPosition) < agent.AgentInfo.CloseDefenseRadius;
        });
        ballIsNearby.AddChild(isBallClose);
        ballIsNearby.AddChild(new GoToTheBall("Move to Ball", blackBoard));

        // 4. Default action: Return to home position
        rootSelector.AddChild(new GoToTheGoalPost("Return to Home", blackBoard));

        entryPoint = rootSelector;
    }

    public override void ExecuteBT()
    {
        entryPoint.Execute();
    }
}
