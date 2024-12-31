using BT_Implementation;
using System;
using UnityEngine;


public class FootballAiBlackboardFactory : IAbstractBlackboardFactory
{

    // Get all necessery references 

    private IFootballAgent ownerAgent_;
    private FootballTeam ownerTeam_;
    private int teamIndex_; 
    public FootballAiBlackboardFactory(IFootballAgent ownerAgent, FootballTeam ownerTeam, int index)
    {
        ownerAgent_ = ownerAgent;
        ownerTeam_ = ownerTeam;
        teamIndex_ = index;
    }
    public Blackboard GetBlackboard()
    {
        Blackboard blackboard = new Blackboard();
        blackboard.SetValue("Owner Agent", ownerAgent_);
        blackboard.SetValue("Owner Team", ownerTeam_);
        blackboard.SetValue("Team Index", teamIndex_);
        
        return blackboard;
    }
}