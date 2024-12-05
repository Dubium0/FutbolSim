
namespace Player.Controller.States
{
    

    public interface IPlayerState
    {
    public void OnEnter();
    public void OnExit();
    public void OnFixedUpdate();

        public void HandleTransition();
    public void Move();

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

