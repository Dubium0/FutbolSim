using FootballSim.Player;
using System;
using Unity.Behavior;
using Unity.Mathematics;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PassOrDribbleDice", story: "[Player] throws a dice to decide pass (true) or dribble (false)", category: "FootballPlayer/Conditions", id: "165ad41a1959691795573403e732a483")]
public partial class PassOrDribbleDiceCondition : Condition
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    private bool m_LatestDesicion = true;

    private float m_DesicionInterval = 1.0f;
    private float m_LastDesicionTime = 0.0f;

    private int m_PassDesicionInfluence = 5;
    private int m_DribbleDesicionInfluence = 5;

    public override bool IsTrue()
    {
     
        if (m_LastDesicionTime + m_DesicionInterval < Time.time)
        {
           
            var passDice = UnityEngine.Random.Range(0, 10) + m_PassDesicionInfluence;
            var dribbleDice = UnityEngine.Random.Range(0, 10) + m_DribbleDesicionInfluence;
            m_LatestDesicion = passDice > dribbleDice;
         
            if (m_LatestDesicion)
            {
                m_PassDesicionInfluence += 1;
                m_PassDesicionInfluence = math.clamp(m_PassDesicionInfluence, 0, 50);
            }
            else
            {
                m_DribbleDesicionInfluence += 1;
                m_DribbleDesicionInfluence = math.clamp(m_DribbleDesicionInfluence, 0, 50);
            }
            m_LastDesicionTime = Time.time;
        }

        return m_LatestDesicion;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
