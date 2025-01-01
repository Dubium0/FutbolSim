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

   

        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var footballTeam = blackBoard.GetValue<FootballTeam>("Owner Team");
        var currentEnemyOwner = Football.Instance.CurrentOwnerPlayer;

        var enemyCurrentLocation = currentEnemyOwner.Transform.position;
        var agentLocation = agent.Transform.position;

        var homeGoalPosition = GameManager.Instance.GetGoalPositionHome(agent.TeamFlag);


        var enemyLocationAfterTSeconds = enemyCurrentLocation + currentEnemyOwner.Rigidbody.linearVelocity * 1;


        var myPitchZone = footballTeam.GetPicthZone();

        var defenseRadius = agent.AgentInfo.LongDefenseRadius;
        if ( Football.Instance.PitchZone == myPitchZone && Football.Instance.SectorNumber < 6 )
        {
            defenseRadius = agent.AgentInfo.CloseDefenseRadius;
        }

        var offsetFromPlayer = (homeGoalPosition - enemyLocationAfterTSeconds).normalized * defenseRadius;

        var goalBounds = GameManager.Instance.GetBoundsHome(agent.TeamFlag);

     
        var direction = (enemyLocationAfterTSeconds + offsetFromPlayer - agentLocation);
        direction.y = 0;
        var prevY = agent.Rigidbody.linearVelocity.y;

        var enemySpeed =  currentEnemyOwner.Rigidbody.linearVelocity.magnitude;

        var followSpeed = Mathf.Clamp(enemySpeed, agent.AgentInfo.MaxWalkSpeed, agent.AgentInfo.MaxRunSpeed);


        agent.Rigidbody.linearVelocity = (Vector3.ClampMagnitude(direction, 1) * followSpeed) + Vector3.up * prevY;
    


        if (goalBounds.Contains(offsetFromPlayer))
        {
            return BTResult.Failure;

        }

        if (agent.IsDebugMode)
        {
            Debug.Log("I'm going to the Intersection!");
            Debug.Log($"Defense radius : {defenseRadius}");
        }


        return BTResult.Success;
    }
}
