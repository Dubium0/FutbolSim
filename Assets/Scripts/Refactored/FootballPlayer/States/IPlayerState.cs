namespace FootballSim.Player
{

    public interface IPlayerState
    {   
        public FootballPlayer FootballPlayer{ get; }
        public void OnEnter();
        public void OnExit();
        public void OnFixedUpdate();
        public void OnUpdate();
       
    }   

}







