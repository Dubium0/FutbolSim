
using BT_Implementation;
using BT_Implementation.Control;
using BT_Implementation.Leaf;

using UnityEngine;

public class DefenseAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();
    public DefenseAIFacade(Blackboard blackBoard) : base(blackBoard)
    {
    }

    public override void ConstructBT()
    {
        SelectorNode defenseEntrySelector = new SelectorNode("Defense AI Entry Selector");

        SequenceNode hasBallOrSequence = new SequenceNode("Has ball Or Sequence");
        defenseEntrySelector.AddChild(hasBallOrSequence);

        ConditionNode doesTeamHasTheBall = new ConditionNode("Does team has the ball", blackBoard, blackBoard => 
        { 
            var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            if (footballTeam.CurrentBallOwnerTeamMate == null)
            {
                if (agent.IsDebugMode)
                {
                    Debug.Log("Team Has no ball");
                }

              
                return false;
            }
            return footballTeam.CurrentBallOwnerTeamMate == Football.Instance.CurrentOwnerPlayer; 
        });
        hasBallOrSequence.AddChild(doesTeamHasTheBall);

        SelectorNode goHomeOrDriveBallSelector = new SelectorNode("Go Home Or Drive Ball ");
        hasBallOrSequence.AddChild(goHomeOrDriveBallSelector);

        SequenceNode doIHaveTheBallOrSequence = new SequenceNode("Do I have the ball or");
        goHomeOrDriveBallSelector.AddChild(doIHaveTheBallOrSequence);

        ConditionNode doIhaveTheBall = new ConditionNode("Do I have the ball", blackBoard, blackBoard =>
        {

            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            var result = agent == Football.Instance.CurrentOwnerPlayer;

            if (agent.IsDebugMode) Debug.Log($"Do I have the ball? {result}");
            return result;

        });
        doIHaveTheBallOrSequence.AddChild(doIhaveTheBall);
        goHomeOrDriveBallSelector.AddChild(new TryToGetInReceivingPosition("Go to the Home Position", blackBoard));

        SelectorNode goHomeOrDefendSelector = new SelectorNode("Go Home Or Defend");
        defenseEntrySelector.AddChild(goHomeOrDefendSelector);

        SequenceNode amIClosestToBallOrSequence = new SequenceNode("Am I closest to the ball");
        goHomeOrDefendSelector.AddChild(amIClosestToBallOrSequence);

        goHomeOrDefendSelector.AddChild(new GoToTheHomePosition("Go to the home position", blackBoard));


        ConditionNode amIClosestToBall = new ConditionNode("Am I closest To ball", blackBoard, blackBoard =>
        {
            var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            var result = footballTeam.ClosestPlayerToBall == agent;
            if (agent.IsDebugMode)
            {
                Debug.Log($"Hello from am I Closest to ball! : {result}");
            }
        
            return result;

        });
        amIClosestToBallOrSequence.AddChild(amIClosestToBall);

        SelectorNode intersectOrRushSelector = new SelectorNode("Intersect or rush");
        amIClosestToBallOrSequence.AddChild(intersectOrRushSelector);


        SequenceNode isInReachableAreaOrSequence = new SequenceNode("Is in reachable area or");    
        intersectOrRushSelector.AddChild(isInReachableAreaOrSequence);

        ConditionNode isInReachableArea = new ConditionNode("Is in reachable area", blackBoard, blackBoard =>
        {

            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            
            var currentEnemyOwner = Football.Instance.CurrentOwnerPlayer;
            if(currentEnemyOwner == null)
            {
                return true;
            }
            var distance = Vector3.Distance( Football.Instance.transform.position, agent.Transform.position );


            return  agent.AgentInfo.CloseDefenseRadius > distance;
        });
        isInReachableAreaOrSequence.AddChild(isInReachableArea);
        isInReachableAreaOrSequence.AddChild(new GoToTheBall("Go to the ball", blackBoard));

        intersectOrRushSelector.AddChild(new GoToTheIntersectionPosition("Go to the intersection position", blackBoard));


         ////////////////////////////////
         entryPoint = defenseEntrySelector;

    }

    public override void ExecuteBT()
    {
        entryPoint.Execute();
    }

}
