using Unity.VisualScripting;
using UnityEngine;


namespace Player.Controller
{


public interface IPlayerActions
{
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


    public void HandleCollision(Collision collision);

    }
}

