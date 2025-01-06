using BT_Implementation;
using BT_Implementation.Leaf;
using UnityEngine;

public class RunIntoEnemy : ActionNode
{
    public RunIntoEnemy(string name, Blackboard blackBoard) : base(name, blackBoard) { }

    public override BTResult Execute()
    {
        var agent = blackBoard.GetValue<IFootballAgent>("Owner Agent");
        var enemies = GameManager.Instance.GetPlayersInGoalArea(agent.TeamFlag);

        if (enemies.Count == 0)
        {
            if (agent.IsDebugMode) Debug.Log("No enemies in the goal area.");
            return BTResult.Failure;
        }

        var closestEnemy = enemies[0];
        float minDistance = Vector3.Distance(agent.Transform.position, closestEnemy.transform.position);

        foreach (var enemy in enemies)
        {
            float distance = Vector3.Distance(agent.Transform.position, enemy.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestEnemy = enemy;
            }
        }

        Vector3 direction = (closestEnemy.transform.position - agent.Transform.position).normalized;
        agent.Rigidbody.linearVelocity = direction * agent.AgentInfo.MaxRunSpeed;

        if (agent.IsDebugMode) Debug.Log($"Running into enemy at {closestEnemy.transform.position}.");
        return BTResult.Running;
    }
}