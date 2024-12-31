
using UnityEngine;


/// <summary>
/// This class consist constant data for the football agent. Maximum speed, max Stamina etc.
/// </summary>
[CreateAssetMenu(fileName = "FootballAgentData", menuName = "Football Agent", order = 1)]
public class FootballAgentInfo : ScriptableObject
{

    public float MaxSpeed = 5.0f;
    public float MaxStamina = 100.0f;
    public float DefenseRadius = 5.0f;

    public int BallAcqusitionStaminaReductionRate = 5;
    public int  MaxBallAcqusitionStamina = 100;
    public int BallAcqusitionPoint = 10;
}

