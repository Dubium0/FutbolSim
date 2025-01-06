using System.Collections.Generic;
using BT_Implementation;
using BT_Implementation.Control;
using BT_Implementation.Leaf;
using UnityEngine;

public class GoalkeeperAIFacade : BTRoot
{
    private BTNode entryPoint;

    public GoalkeeperAIFacade(Blackboard blackBoard) : base(blackBoard) { }

    public override void ConstructBT()
    {
        var defenseEntrySelector = new SelectorNode("Defense AI Entry Selector");

        defenseEntrySelector.AddChild(CreateHasBallOrSequence());
        defenseEntrySelector.AddChild(CreateGoHomeOrDefendSelector());

        entryPoint = defenseEntrySelector;
    }

    private SequenceNode CreateHasBallOrSequence()
    {
        var hasBallOrSequence = new SequenceNode("Has Ball Or Sequence");

        hasBallOrSequence.AddChild(new ConditionNode("Does team has the ball", blackBoard, DoesTeamHaveBall));

        var goHomeOrDriveBallSelector = new SelectorNode("Go Home Or Drive Ball");
        goHomeOrDriveBallSelector.AddChild(CreateDoIHaveTheBallOrSequence());
        goHomeOrDriveBallSelector.AddChild(new GoToTheGoalPost("Go to the Home Position", blackBoard));

        hasBallOrSequence.AddChild(goHomeOrDriveBallSelector);
        return hasBallOrSequence;
    }

    private SequenceNode CreateDoIHaveTheBallOrSequence()
    {
        var doIHaveTheBallOrSequence = new SequenceNode("Do I Have the Ball Or Sequence");

        doIHaveTheBallOrSequence.AddChild(new ConditionNode("Do I have the ball", blackBoard, DoIHaveTheBall));
        doIHaveTheBallOrSequence.AddChild(CreateAttackSelector());

        return doIHaveTheBallOrSequence;
    }

    private SelectorNode CreateAttackSelector()
    {
        var attackSelector = new SelectorNode("Attack Selector");

        attackSelector.AddChild(CreateCanIShootOrSequence());
        attackSelector.AddChild(CreateCanIPassOrSequence());
        attackSelector.AddChild(new GoToTheGoalPost("Dribble The Next Available Position", blackBoard));

        return attackSelector;
    }

    private SequenceNode CreateCanIShootOrSequence()
    {
        var canIShootOrSequence = new SequenceNode("Can I Shoot Or Sequence");

        canIShootOrSequence.AddChild(new ConditionNode("Can I shoot", blackBoard, CanIShoot));
        canIShootOrSequence.AddChild(new ShootTheBall("Shoot the ball", blackBoard));

        return canIShootOrSequence;
    }

    private SequenceNode CreateCanIPassOrSequence()
    {
        var canIPassOrSequence = new SequenceNode("Can I Pass Or Sequence");

        canIPassOrSequence.AddChild(new ConditionNode("Can I pass", blackBoard, CanIPass));
        canIPassOrSequence.AddChild(new PassTheBall("Pass the ball", blackBoard));

        return canIPassOrSequence;
    }

    private SelectorNode CreateGoHomeOrDefendSelector()
    {
        var goHomeOrDefendSelector = new SelectorNode("Go Home Or Defend Selector");

        var amIClosestToBallOrSequence = new SequenceNode("Am I Closest To The Ball Sequence");
        amIClosestToBallOrSequence.AddChild(new ConditionNode("Am I Closest To Ball", blackBoard, AmIClosestToBall));
        amIClosestToBallOrSequence.AddChild(CreateIntersectOrRushSelector());

        goHomeOrDefendSelector.AddChild(amIClosestToBallOrSequence);
        goHomeOrDefendSelector.AddChild(new GoToTheGoalPost("Go to the Home Position", blackBoard));

        return goHomeOrDefendSelector;
    }

    private SelectorNode CreateIntersectOrRushSelector()
    {
        var intersectOrRushSelector = new SelectorNode("Intersect Or Rush Selector");

        var isInReachableAreaOrSequence = new SequenceNode("Is In Reachable Area Or Sequence");
        isInReachableAreaOrSequence.AddChild(new ConditionNode("Is In Reachable Area", blackBoard, IsInReachableArea));
        isInReachableAreaOrSequence.AddChild(new GoToTheBall("Go to the Ball", blackBoard));

        intersectOrRushSelector.AddChild(isInReachableAreaOrSequence);
        intersectOrRushSelector.AddChild(new GoToTheGoalPost("Go to the Intersection Position", blackBoard));

        return intersectOrRushSelector;
    }

    private bool DoesTeamHaveBall(Blackboard blackBoard)
    {
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        if (footballTeam.CurrentBallOwnerTeamMate == null)
        {
            if (agent.IsDebugMode)
                Debug.Log("Team has no ball");

            return false;
        }
        return footballTeam.CurrentBallOwnerTeamMate == Football.Instance.CurrentOwnerPlayer;
    }

    private bool DoIHaveTheBall(Blackboard blackBoard)
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var result = agent == Football.Instance.CurrentOwnerPlayer;

        if (agent.IsDebugMode)
            Debug.Log($"Do I have the ball? {result}");

        return result;
    }

    private bool CanIShoot(Blackboard blackBoard)
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var enemyGoal = GameManager.Instance.GetEnemyGoalInstance(agent.TeamFlag);
        var enemyLayer = GameManager.Instance.GetLayerMaskOfEnemy(agent.TeamFlag);
        var possibleLocations = enemyGoal.GetHitPointPositions();

        if (Vector3.Distance(enemyGoal.transform.position, agent.Transform.position) < agent.AgentInfo.MaximumShootDistance)
        {
            var shootablePositions = new List<Transform>();
            foreach (var location in possibleLocations)
            {
                var direction = location.position - agent.Transform.position;
                if (!Physics.Raycast(agent.Transform.position, direction.normalized, direction.magnitude, enemyLayer))
                {
                    shootablePositions.Add(location);
                }
            }

            if (shootablePositions.Count > 0)
            {
                blackBoard.SetValue("Shootable Positions", shootablePositions);
                if (agent.IsDebugMode)
                    Debug.Log("There are possible locations to shoot");

                return true;
            }
        }

        return false;
    }

    private bool CanIPass(Blackboard blackBoard)
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
        var awayGoalPosition = GameManager.Instance.GetGoalPositionAway(agent.TeamFlag);
        var currentPlayerDistance = Vector3.Distance(awayGoalPosition, agent.Transform.position);

        var passCandidates = new List<IFootballAgent>();
        var layerMask = GameManager.Instance.GetLayerMaskOfEnemy(agent.TeamFlag);

        foreach (var mate in footballTeam.FootballAgents)
        {
            var distance = Vector3.Distance(awayGoalPosition, mate.Transform.position);
            if (distance < currentPlayerDistance)
            {
                var direction = mate.Transform.position - agent.Transform.position;
                if (!Physics.Raycast(agent.Transform.position, direction.normalized, direction.magnitude, layerMask))
                {
                    passCandidates.Add(mate);
                }
            }
        }

        blackBoard.SetValue("Pass Candidates", passCandidates);
        return passCandidates.Count > 0;
    }

    private bool AmIClosestToBall(Blackboard blackBoard)
    {
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var result = footballTeam.ClosestPlayerToBall == agent;

        if (agent.IsDebugMode)
            Debug.Log($"Am I closest to the ball? {result}");

        return result;
    }

    private bool IsInReachableArea(Blackboard blackBoard)
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var currentEnemyOwner = Football.Instance.CurrentOwnerPlayer;

        if (currentEnemyOwner == null)
            return true;

        var distance = Vector3.Distance(Football.Instance.transform.position, agent.Transform.position);
        return agent.AgentInfo.CloseDefenseRadius > distance;
    }

    public override void ExecuteBT()
    {
        entryPoint.Execute();
    }
}
