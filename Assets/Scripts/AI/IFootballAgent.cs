using Player.Controller.States;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlayerType
{
    Goalkeeper,
    Defender,
    Midfielder,
    Forward
}

public interface IFootballAgent
{
    public FootballAgentInfo AgentInfo { get; }
    public Transform Transform { get; }

    public Transform FocusPointTransform { get; }
    public PlayerType PlayerType { get;}

    public Rigidbody Rigidbody { get; }

    public bool IsInitialized { get; }

    public TeamFlag TeamFlag { get;}

    public bool IsDebugMode { get; }    
    public void InitAISystems(FootballTeam team, PlayerType playerType,int index);

    public void TickAISystem();


    public int TryToAcquireBall();

    public Action<IFootballAgent> OnBallPossesionCallback { get; set; }

    public void OnBallPossesion()
    {
        // Debug.Log("Ball Possed By : " + this.ToSafeString());
        OnBallPossesionCallback(this);
    }

    // fields for human controllable side

    public void SetAsHumanControlled();
    public void SetAsAIControlled();
    public Vector3 MovementVector{get;}

    public InputAction SprintAction { get; }

    public Vector3 WorldMousePosition {  get; }


    public void SetState(IPlayerState state);

    public void ChangeToGhostLayer(float time);

    public int GetShootScore();

    public void DisableAIForATime(float time);

}



