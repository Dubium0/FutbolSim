using FootballSim.Player;
using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using Unity.Mathematics;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TryToGetTheBall", story: "[Player] tries to take the ball", category: "FootballPlayer/Action", id: "8800c70ea81296693fa656ca6685bfd9")]
public partial class TryToGetTheBallAction : Action
{
    [SerializeReference] public BlackboardVariable<FootballPlayer> Player;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        
        var theVector = Player.Value.transform.position - FootballSim.Football.Football.Instance.transform.position;

        if (theVector.sqrMagnitude < 0.01f)
        {
            Player.Value.Rigidbody.linearVelocity = Vector3.zero;
            return Status.Success; // Already at the ball
        }

        var projOnV = Vector3.Project(theVector, FootballSim.Football.Football.Instance.Rigidbody.linearVelocity.normalized);

        var otherAxis = theVector - projOnV;

        var result = Heuristics(otherAxis, FootballSim.Football.Football.Instance.transform.position, projOnV);
       
      
            
        Player.Value.Rigidbody.linearVelocity = result;

        if (result.sqrMagnitude > 0 && result != null)
        {

            Player.Value.transform.forward = MathExtra.MoveTowards(Player.Value.transform.forward, result.normalized, 1 / Player.Value.Data.RotationTime);

        }
       
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
    private Vector3 Heuristics(Vector3 otherAxis, Vector3 currentBallPosition, Vector3 projOnV)
    {
        var upSpeed = Player.Value.Data.MaxRunSpeed;
        var bottomSpeed = 0.0f;
        var currentOtherAxisSpeed = upSpeed/2.0f;

        otherAxis.y = 0;
        currentBallPosition.y = 0;
        projOnV.y = 0;
        Vector3 resultSpeedVector = Vector3.zero;
        float catchTime = 0;
        for (int iter = 0; iter < 10; iter++)
        {
            currentOtherAxisSpeed = math.clamp(currentOtherAxisSpeed, 0.1f, 99999);
            catchTime = otherAxis.sqrMagnitude / currentOtherAxisSpeed;

            var ballPositionOnCatchTime = FootballSim.Football.Football.Instance.GetDropPointAfterTSeconds(catchTime);
            ballPositionOnCatchTime.y = 0;

            var currentPlayerProjAxisPosition = currentBallPosition + projOnV;

            var catchDistance = ballPositionOnCatchTime - currentPlayerProjAxisPosition;

            var catchDirection = catchDistance.normalized;

            var remainingPlayerSpeed = Mathf.Sqrt(30.0f * 30.0f - currentOtherAxisSpeed * currentOtherAxisSpeed);

            var currentPlayerProjAxisSpeed = catchDirection * remainingPlayerSpeed;
           
            var k = catchDistance.x / catchDirection.x;
 
            if (MathF.Abs(remainingPlayerSpeed * catchTime - k) < 0.5f)
            {
                Debug.Log("I catch !"); // increase Y speed
                resultSpeedVector = currentPlayerProjAxisSpeed;
                break;

            }
            else if (remainingPlayerSpeed * catchTime > k)
            {
                Debug.Log("I am fast");
                bottomSpeed = currentOtherAxisSpeed;
                currentOtherAxisSpeed = (currentOtherAxisSpeed + upSpeed) / 2f;
            }
            else if (remainingPlayerSpeed * catchTime < k)
            {
                Debug.Log("I am slow");
                upSpeed = currentOtherAxisSpeed;
                currentOtherAxisSpeed = (currentOtherAxisSpeed + bottomSpeed) / 2f;
            }
        }
        resultSpeedVector += -otherAxis * currentOtherAxisSpeed;
        //Debug.DrawRay(follower.position, resultSpeedVector.normalized, Color.green);

        //Debug.DrawRay(follower.position, resultSpeedVector * catchTime, Color.yellow);

        return resultSpeedVector;


    }
}


