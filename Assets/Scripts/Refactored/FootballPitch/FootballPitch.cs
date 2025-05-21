using System.Collections.Generic;
using UnityEngine;

namespace FootballSim.FootballPitch
{
    public class FootballPitch : MonoBehaviour
    {   
        [SerializeField]
        private Transform m_HomeGoalTransform;
        public Transform HomeGoalTransform { get => m_HomeGoalTransform; }
        [SerializeField]
        private List<Transform> m_HomeGoalPoints;
        public Bounds HomeGoalBounds
        {
            get
            {
                Bounds bounds = new Bounds(m_HomeGoalPoints[0].position, Vector3.zero);
                for (int i = 1; i < m_HomeGoalPoints.Count; i++)
                {
                    bounds.Encapsulate(m_HomeGoalPoints[i].position);
                }
                return bounds;
            }
        }
        [SerializeField]
        private Transform m_AwayGoalTransform;
        public Transform AwayGoalTransform { get => m_AwayGoalTransform; }
        [SerializeField]
        private List<Transform> m_AwayGoalPoints;
        public Bounds AwayGoalBounds{get
            {
                Bounds bounds = new Bounds(m_AwayGoalPoints[0].position, Vector3.zero);
                for (int i = 1; i < m_AwayGoalPoints.Count; i++)
                {
                    bounds.Encapsulate(m_AwayGoalPoints[i].position);
                }
                return bounds;
            }
        }


    }
}