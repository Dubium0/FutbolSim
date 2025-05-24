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

        StartCoroutine(Heuristic(projOnV, otherAxis));
    }

    int maxIteration = 10;
    private float GetOtherAxisSpeed(float max, float current)
    {
        return max * max - current * current;
    }

    private bool isCurrentlyCalculating = false;
    private IEnumerator Heuristic(Vector3 projAxis, Vector3 otherAxis)
    {
        if (isCurrentlyCalculating) yield break;
        isCurrentlyCalculating = true;
        float upVelocity = 30;
        float bottomVelocity = 0;
        float currentYAxisMagnitude = upVelocity; // strat with max speed

        float errorMargin = 0.5f; // on both axis

        Vector3? Result = null;
        for (int i = 0; i < maxIteration; i++)
        {
            var catchTime = otherAxis.sqrMagnitude / currentYAxisMagnitude;
            Debug.Log("Catch Time " +catchTime);
            var xAxisMagnitude = GetOtherAxisSpeed(30, currentYAxisMagnitude);

            var finalVelocityVector = projAxis.normalized * xAxisMagnitude + -otherAxis.normalized * currentYAxisMagnitude;

            var dropPointOfTarget = targetRb.linearVelocity * catchTime+ target.position;

            var ourEstimation = finalVelocityVector * catchTime + follower.position;

            dropPointOfTarget.y = ourEstimation.y;

            Debug.DrawRay(target.position, (dropPointOfTarget - target.position),Color.yellow,0.2f);

            var distance = ourEstimation - dropPointOfTarget;
            //Debug.Log("our Estimation : " + ourEstimation);
            dropPoint.position = dropPointOfTarget;
            estimation.position = ourEstimation;
            //Debug.Log("drop point : " + dropPointOfTarget);
            if (distance.sqrMagnitude < errorMargin)
            {
                var correctedDir = (dropPointOfTarget - follower.position).normalized;
                var correctVelocity = (dropPointOfTarget - follower.position).sqrMagnitude / catchTime;
                //distance is close enough
                Result = correctedDir * correctVelocity;
                break;
            }
            else
            {
                if ((targetRb.linearVelocity + distance.normalized).sqrMagnitude < targetRb.linearVelocity.sqrMagnitude)
                {
                    // it means that I were too fast on the the otherAxis
                    upVelocity = currentYAxisMagnitude;
                    currentYAxisMagnitude = (currentYAxisMagnitude + bottomVelocity) / 2.0f;
                }
                {
                    // I was so slow on the other axis
                    bottomVelocity = currentYAxisMagnitude;
                    currentYAxisMagnitude = (currentYAxisMagnitude + upVelocity) / 2.0f;
                }
            }

            Debug.DrawRay(follower.position, finalVelocityVector.normalized,Color.cyan,0.2f);
            yield return new WaitForSeconds(0.2f);
        }

        if (Result != null)
        {
            Debug.Log("Found the Vector!");
            Debug.DrawRay(follower.position, Result.Value,Color.green,0.2f);
        }
        isCurrentlyCalculating = false;
        
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