using System.Collections.Generic;
using UnityEngine;

namespace FootballSim.FootballTeam
{

    public class FootballFormation : MonoBehaviour
    {
        [SerializeField]
        private Transform m_GoalKeeperPosition;
        public Transform GoalKeeperPosition => m_GoalKeeperPosition;

        [SerializeField]
        private List<Transform> m_DefensePosition = new();
        public List<Transform> DefensePosition
        {
            get { return m_DefensePosition; }
        }
        [SerializeField]
        private List<Transform> m_MidfieldPosition = new();
        public List<Transform> MidfieldPosition
        {
            get { return m_MidfieldPosition; }
        }
        [SerializeField]
        private List<Transform> m_ForwardPosition =new();
        public List<Transform> ForwardPosition
        {
            get { return m_ForwardPosition; }
        }
        [SerializeField]
        private Transform m_DefenseLine;
        public Transform DefenseLine
        {
            get { return m_DefenseLine; }
        }
        [SerializeField]
        private Transform m_MidfieldLine;
        public Transform MidfieldLine
        {
            get { return m_MidfieldLine; }
        }
        [SerializeField]
        private Transform m_ForwardLine;
        public Transform ForwardLine
        {
            get { return m_ForwardLine; }
        }
         [SerializeField]
        private bool m_EnableDebug = true;

        private void OnValidate()
        {
            FillPositions();
        }

        private void Awake()
        {
            FillPositions();
        }

        private void FillPositions()
        {
            if(m_EnableDebug && (m_DefenseLine == null || m_MidfieldLine == null || m_ForwardLine == null || m_GoalKeeperPosition == null))
            {
                Debug.LogError("Please assign all the lines");
                return;
            }
            m_DefensePosition.Clear();
            m_MidfieldPosition.Clear();
            m_ForwardPosition.Clear();
            if (m_DefenseLine != null)
            {       
                for (int i = 0; i < m_DefenseLine.childCount; i++)
                {
                    m_DefensePosition.Add(m_DefenseLine.GetChild(i));
                }
            }
            if (m_MidfieldLine != null)
            {       
                for (int i = 0; i < m_MidfieldLine.childCount; i++)
                {
                    m_MidfieldPosition.Add(m_MidfieldLine.GetChild(i));
                }
            }

            if (m_ForwardLine != null)
            {       
                for (int i = 0; i < m_ForwardLine.childCount; i++)
                {
                    m_ForwardPosition.Add(m_ForwardLine.GetChild(i));
                }
            }

        }
    



    }


}