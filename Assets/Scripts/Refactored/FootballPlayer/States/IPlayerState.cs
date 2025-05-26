namespace FootballSim.Player
{

    public interface IPlayerState
    {   
        public FootballPlayer FootballPlayer{ get; }
        public void OnEnter();
        public void OnExit();
        public void OnFixedUpdate();
        public void OnUpdate();
        public void OnNotMovementRelatedUpdate();
        public void OnSprintEnter();
        public void OnSprintExit();

        public void OnPassActionEnter();
        public void OnPassActionExit();
        public void OnThroughPassActionEnter();
        public void OnThroughPassActionExit();
        public void OnLobPassActionEnter();
        public void OnLobPassActionExit();
        public void OnShootActionEnter();
        public void OnShootActionExit();
       
    }   

}







