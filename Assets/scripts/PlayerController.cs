using NUnit.Framework;
using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    /*
     I need movement on xz plane
     There should be walking and running
     I want to adjust maximum speed and acceleration
     */

    private enum PlayerState
    {
        HasBall,
        NoBall,
        JustShooted
    }
    private PlayerState state_;

    private InputAction moveAction_;
    private InputAction sprintAction_;
    private InputAction attackAction_;

    private Rigidbody rigidbody_;

    [SerializeField]  private float maxWalkSpeed_ = 4.0f;
    [SerializeField]  private float maxRunSpeed_ = 9.0f;

    [SerializeField]  private float walkingAcceleration_ = 6.0f;
    [SerializeField] private float runningAcceleration_ = 12.0f;

    private float currentAcceleration_;

    private const string SprintDecelerationKey = "SlowDown";

    [SerializeField]  private float sprintDecelerationRate_ = 4.0f;

    private Dictionary<string ,Action> perFixedUpdateActions_ = new ();

    [SerializeField] private float ballAcqusitionRadius_ = 1;
    [SerializeField] private float ballAcqusitionRate_ = 2;
    [SerializeField] private LayerMask ballAcquisitonLayers_;

    [SerializeField] private float ballDrivingPower_= 2;
    [SerializeField] private Transform playerFrontPosition_;

    public Transform PlayerFrontTransform {  get { return playerFrontPosition_; } }
    [SerializeField] private float rotationTime= 2;

    SoccerBall soccerBall_ = null;

    private float shootPowerSlider = 0.0f;
    [SerializeField] private float maxShootPower = 5.0f;

    
    private void Awake()
    {

        rigidbody_ = GetComponent<Rigidbody>(); 

        moveAction_ = InputSystem.actions.FindAction("Move");
        sprintAction_ = InputSystem.actions.FindAction("Sprint");
        attackAction_ = InputSystem.actions.FindAction("Attack");

        sprintAction_.performed += context=> { OnSprint(); };
        sprintAction_.canceled += context=> { ExitSprint(); };

        attackAction_.performed += context => { OnShootStart(); };
        attackAction_.canceled += context => { OnShootExit(); };

        state_ = PlayerState.NoBall;

        RegisterPerFixedUpdateAction("KeepBall", TryToKeepBall);

       
        rigidbody_.maxLinearVelocity = maxRunSpeed_;


        currentAcceleration_ = walkingAcceleration_;
    }

  
    private void FixedUpdate()
    {
        ExecutePerFixedUpdateActions();
        Move();


    }


    private void RegisterPerFixedUpdateAction(string key,Action newAction)
    {
        perFixedUpdateActions_.Add(key, newAction);

    }
    private void UnregisterPerFixedUpdateAction(string key)
    {

        perFixedUpdateActions_.Remove(key);
    }
    private void ExecutePerFixedUpdateActions()
    {
        foreach (var action in perFixedUpdateActions_)
        {
            action.Value.Invoke();
        }
    }

    public void OnSprint()
    {
        UnregisterPerFixedUpdateAction(SprintDecelerationKey);
        rigidbody_.maxLinearVelocity = maxRunSpeed_;
        currentAcceleration_ = runningAcceleration_ ;
    }
    public void ExitSprint()
    {
        
        currentAcceleration_ = walkingAcceleration_;
        RegisterPerFixedUpdateAction(SprintDecelerationKey, () =>{

            if(rigidbody_.maxLinearVelocity > maxWalkSpeed_ + 0.1)
            {
                rigidbody_.maxLinearVelocity -= (rigidbody_.maxLinearVelocity - maxWalkSpeed_)/ sprintDecelerationRate_;
            }
            else
            {
                rigidbody_.maxLinearVelocity = maxWalkSpeed_;
            }

        });

    }

 
    public void Move()
    {
        
        // TODO: Adjust movement space accordingly camera
        var inputVector =  moveAction_.ReadValue<Vector2>()
                .normalized
                .ToZXMinus(); 

        rigidbody_.AddForce(inputVector * currentAcceleration_,ForceMode.Acceleration);

        if (inputVector.magnitude > 0)
        {
            Debug.Log("Input Vector: " + inputVector);
            transform.forward = MathExtra.MoveTowards(transform.forward, inputVector, 1 / rotationTime);

        }
        
    }

    public void AdjustLookDirection()
    {
        Quaternion newDirection = Quaternion.Euler(0.0f,-90 , 0.0f);

        rigidbody_.MoveRotation(newDirection);

    }

   
    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position, ballAcqusitionRadius_);
    }


    public void OnShootStart()
    {
        var forceVector = transform.forward;
        forceVector.y = 0.3f;
        if (soccerBall_ != null &&  state_ == PlayerState.HasBall) { 
            soccerBall_.ApplyForce(forceVector * 10);
            soccerBall_ = null;
            state_ = PlayerState.JustShooted;
        }
       
    }
    public void OnShootExit()
    {
        
    }
    public void TryToKeepBall()
    {
        if (state_ == PlayerState.NoBall || state_ == PlayerState.HasBall)
        {

            var collisions = Physics.OverlapSphere(transform.position, ballAcqusitionRadius_, ballAcquisitonLayers_) ;


            if(collisions.Length> 0)
            {

                var collision = collisions[0]; // there will be 1 ball so can only occur one 
           
                if(collision.TryGetComponent<SoccerBall>(out var soccerBall))
                {

                    soccerBall.AcqusitionRequest(this);
                    this.soccerBall_ = soccerBall;
                    state_ = PlayerState.HasBall;

                }

            }
        }
    }

}
