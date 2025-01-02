using System.Collections.Generic;

using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField]
    private Transform hitPointPositions;

  

    public Transform[] GetHitPointPositions()
    {
        Transform[] hitPoints = new Transform[hitPointPositions.childCount];

        for (int i = 0; i < hitPointPositions.childCount; i++)
        {
            hitPoints[i] = hitPointPositions.GetChild(i).transform;
        }
        return hitPoints;   

    }


}