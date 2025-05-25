using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class MathTest : MonoBehaviour
{

    public Transform target;
    public Transform follower;
    private Rigidbody targetRb;
    private Rigidbody followerRb;

    public float followerSpeed;

    public Transform targetLook;

    public Transform estimation;
    public Transform dropPoint;



    void Awake()
    {
        targetRb = target.GetComponent<Rigidbody>();
        followerRb = follower.GetComponent<Rigidbody>();
    }
    void Update()
    {
        var theVector = follower.position - target.position;

        targetRb.linearVelocity = (targetLook.position - target.position).normalized;
        var projOnV = Vector3.Project(theVector, targetRb.linearVelocity.normalized);

        var otherAxis = theVector - projOnV;

        Debug.DrawRay(target.position, projOnV, Color.red);
        Debug.DrawRay(target.position, otherAxis, Color.blue);
        Debug.DrawRay(target.position, targetRb.linearVelocity.normalized);

        // always move to -otherAxis
        Heuristics(otherAxis, target.position, projOnV);
        


    }

    private void Heuristics(Vector3 otherAxis, Vector3 currentBallPosition, Vector3 projOnV)
    {
        var currentOtherAxisSpeed = 3.50f;
        var upSpeed = 7.50f;
        var bottomSpeed = 0.0f;

        otherAxis.y = 0;
        currentBallPosition.y = 0;
        projOnV.y = 0;
        Vector3 resultSpeedVector = Vector3.zero;
        float catchTime = 0;
        for (int iter = 0; iter < 10; iter++)
        {

            catchTime = otherAxis.sqrMagnitude / currentOtherAxisSpeed;

            var ballPositionOnCatchTime = GetDropPointAfterTSeconds(catchTime);
            ballPositionOnCatchTime.y = 0;

            var currentPlayerProjAxisPosition = currentBallPosition + projOnV;

            var catchDistance = ballPositionOnCatchTime - currentPlayerProjAxisPosition;

            var catchDirection = catchDistance.normalized;

            var remainingPlayerSpeed = Mathf.Sqrt(30 * 30 - currentOtherAxisSpeed * currentOtherAxisSpeed);

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
        Debug.DrawRay(follower.position, resultSpeedVector.normalized, Color.green);

        Debug.DrawRay(follower.position, resultSpeedVector * catchTime, Color.yellow);

        followerRb.linearVelocity = resultSpeedVector;


    }



    public Vector3 GetDropPointAfterTSeconds(float time)
    {
        Vector3 currentPosition = targetRb.position;
        Vector3 currentVelocity = targetRb.linearVelocity;

        Vector3 acceleration = Physics.gravity;

        float deltaTime = 0.02f;
        Vector3 position = currentPosition;
        Vector3 velocity = currentVelocity;
        var collider = targetRb.GetComponent<Collider>();
        var physicsMaterial = collider.material;

        float raycastDistance = 0.1f;

        for (float t = 0; t < time; t += deltaTime)
        {

            var isOnGroundNow = Physics.Raycast(position, Vector3.down, raycastDistance, 1 << 7) || Physics.Raycast(position, Vector3.up, 10, 1 << 7);

            if (isOnGroundNow)
            {
                velocity.y = 0;
                acceleration.y = 0;
                if (physicsMaterial != null)
                    velocity *= Mathf.Clamp01(1 - physicsMaterial.dynamicFriction * deltaTime);
            }

            velocity += acceleration * deltaTime;

            position += velocity * deltaTime;
        }

        return position;
    }


}