namespace FootballSim.Player
{

    public interface IPlayerState
    {   
        public FootballPlayer FootballPlayer{ get; }
        public void OnEnter();
        public void OnExit();
        public void OnFixedUpdate();
        public void OnUpdate();

        public void HandleTransition();

        public void OnSprintEnter();
        public void OnSprintExit();

        public void OnLowActionAEnter();
        public void OnLowActionAExit();
        public void OnLowActionBEnter();
        public void OnLowActionBExit();
        public void OnHighActionAEnter();
        public void OnHighActionAExit();
        public void OnHighActionBEnter();
        public void OnHighActionBExit();
       
    }   

}







