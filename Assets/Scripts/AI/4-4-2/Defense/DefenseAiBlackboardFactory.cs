using BT_Implementation;
using UnityEngine;


public class DefenseAiBlackboardFactory : IAbstractBlackboardFactory
{
    
    // Get all necessery references 

    private FootballAgent ownerAgent_;
    public DefenseAiBlackboardFactory(FootballAgent ownerAgent)
    {
        ownerAgent_ = ownerAgent;
    }
    public Blackboard GetBlackboard()
    {
        Blackboard blackboard = new Blackboard();
        blackboard.SetValue("Owner Agent", ownerAgent_);

        return blackboard;
    }
}
