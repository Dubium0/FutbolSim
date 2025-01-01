
using UnityEngine;


/// <summary>
/// This class consist constant data for the football agent. Maximum speed, max Stamina etc.
/// </summary>
[CreateAssetMenu(fileName = "FootballAgentData", menuName = "Football Agent", order = 1)]
public class FootballAgentInfo : ScriptableObject
{

    public float MaxSpeed = 5.0f;
    public float MaxStamina = 100.0f;
    public float CloseDefenseRadius = 2.0f;

    public float LongDefenseRadius = 5.0f;

    public int BallAcqusitionStaminaReductionRate = 5;
    public int  MaxBallAcqusitionStamina = 100;
    public int BallAcqusitionPoint = 10;

    public float RotationTime = 2;

    public float MaxWalkSpeed = 4.0f;

    public float MaxRunSpeed = 9.0f;

    public float MaxStrugglingSpeed = 1;

    public float WalkingAcceleration = 6.0f;

    public float RunningAcceleration = 12.0f;

    public float StrugglingAcceleration = 3.0f;
}

