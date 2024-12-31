using UnityEngine;
using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine.ParticleSystemJobs;
using Player.Controller;

public class GoToTheIntersectionPosition : ActionNode
{


    public GoToTheIntersectionPosition(string name, Blackboard blackBoard) : base(name, blackBoard)
    {
    }

    public override BTResult Execute()
    {

        var futureBallPosition = Football.Instance.GetDropPointAfterTSeconds(1);

        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
        var currentEnemyOwner = Football.Instance.CurrentOwnerPlayer;

        var enemyCurrentLocation = currentEnemyOwner.Transform.position;
        var agentLocation = agent.Transform.position;

        var homeGoalPosition = GameManager.Instance.GetGoalPositionHome(agent.TeamFlag);

        var distanceToEnemy = Vector3.Distance(agentLocation, enemyCurrentLocation);

        var enemyLocationAfterTSeconds = enemyCurrentLocation + currentEnemyOwner.Rigidbody.linearVelocity * Mathf.Clamp(distanceToEnemy,0,3);


        var myPitchZone = footballTeam.GetPicthZone();

        var defenseRadius = agent.AgentInfo.LongDefenseRadius;
        if ( Football.Instance.PitchZone == myPitchZone && Football.Instance.SectorNumber < 3 )
        {
            defenseRadius = agent.AgentInfo.CloseDefenseRadius;
        }

        var offsetFromPlayer = (homeGoalPosition - enemyLocationAfterTSeconds).normalized * defenseRadius;

        var goalBounds = GameManager.Instance.GetBoundsHome(agent.TeamFlag);

        var distanceToGoal = Vector3.Distance(offsetFromPlayer, agentLocation);

        if(distanceToGoal > 0.2)
        {
            var direction = (offsetFromPlayer - agentLocation).normalized * agent.AgentInfo.MaxSpeed;
            direction.y = 0;
            var prevY = agent.Rigidbody.linearVelocity.y;
            agent.Rigidbody.linearVelocity = (direction.normalized * agent.AgentInfo.MaxSpeed) + Vector3.up * prevY;
            return BTResult.Running;
        }

        if (goalBounds.Contains(offsetFromPlayer))
        {
            return BTResult.Failure;

        }

        return BTResult.Success;
    }
}
