
using BT_Implementation;
using BT_Implementation.Control;
using BT_Implementation.Leaf;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenericAIFacade : BTRoot
{
    private BTNode entryPoint = new NullBTNode();
    public GenericAIFacade(Blackboard blackBoard) : base(blackBoard)
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

      
        SelectorNode attackSelector = new SelectorNode("Attack Selector");
        doIHaveTheBallOrSequence.AddChild(attackSelector); 

        SequenceNode  canIShootOrSequence = new SequenceNode("Can I shoot or");
        attackSelector.AddChild(canIShootOrSequence);

        ConditionNode canIShoot = new ConditionNode("Can I shoot", blackBoard, blackBoard =>
        {
            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            var enemyGoal = GameManager.Instance.GetEnemyGoalInstance(agent.TeamFlag);
            var enemyLayer = GameManager.Instance.GetLayerMaskOfEnemy(agent.TeamFlag);
            var possibleLocations = enemyGoal.GetHitPointPositions();

            if ( Vector3.Distance( enemyGoal.transform.position, agent.Transform.position)  < agent.AgentInfo.MaximumShootDistance)
            {
                // Debug.Log("I am close enoguh to shoot");
                List<Transform> shootablePositions = new();
                foreach (var possibleLocation in possibleLocations)
                {
                    var direction = possibleLocation.position - agent.Transform.position;
                    if( !Physics.Raycast(agent.Transform.position, direction.normalized, direction.magnitude, enemyLayer))
                    {
                        shootablePositions.Add(possibleLocation);
                    }
                }

                if(shootablePositions.Count > 0)
                {
                    // Debug.Log("There are possible locations to shoot");
                    blackBoard.SetValue<List<Transform>>("Shootable Positions", shootablePositions);
                    if (agent.IsDebugMode) Debug.Log($"I am going to shoooot?");
                    return true;
                }

            }
            // Debug.Log($"I cant shoot");
            return false;

        });
        canIShootOrSequence.AddChild(canIShoot);
        canIShootOrSequence.AddChild(new ShootTheBall("Shoot the ball", blackBoard));

        SequenceNode canIPassOrSequence = new SequenceNode("Can I Pass Or ");
        attackSelector.AddChild(canIPassOrSequence);

        ConditionNode canIPass = new ConditionNode("Can I pass", blackBoard, blackBoard =>
        {
            var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
            var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
            var awayGoalPosition = GameManager.Instance.GetGoalPositionAway(agent.TeamFlag);

            var currentPlayerDistance =   Vector3.Distance(awayGoalPosition ,agent.Transform.position);

            List<IFootballAgent> passCandidates = new List<IFootballAgent>();
            var layermask = GameManager.Instance.GetLayerMaskOfEnemy(agent.TeamFlag);
            foreach (var mate in footballTeam.FootballAgents)
            {
                var distance = Vector3.Distance(awayGoalPosition , mate.Transform.position);
                if(Mathf.Abs(distance) < currentPlayerDistance ) { 
                    var direction = mate.Transform.position - agent.Transform.position;
                    if(!Physics.Raycast(agent.Transform.position, direction.normalized, direction.magnitude, layermask))
                    {
                        passCandidates.Add(mate);
                    }   
                }
            }

            blackBoard.SetValue<List<IFootballAgent>>("Pass Candidates", passCandidates);
            var result = passCandidates.Count > 0;
            return result;
        });
        canIPassOrSequence.AddChild(canIPass);
        canIPassOrSequence.AddChild(new PassTheBall("Pass the ball", blackBoard));

        attackSelector.AddChild(new DribbleTheNextAvailablePosition("Dribble The Next Available Position",blackBoard));

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
