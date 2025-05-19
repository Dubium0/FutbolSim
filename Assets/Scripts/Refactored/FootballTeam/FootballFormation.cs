using UnityEngine;

namespace FootballSim.FootballTeam
{

    public class FootballFormation : MonoBehaviour
    {
        [SerializeField]
        private Transform m_GoalKeeperPosition;
        public Transform GoalKeeperPosition => m_GoalKeeperPosition;

        [SerializeField]
        private Transform[] m_DefensePosition;
        public Transform[] DefensePosition
        {
            get { return m_DefensePosition; }
        }
        [SerializeField]
        private Transform[] m_MidfieldPosition;
        public Transform[] MidfieldPosition
        {
            get { return m_MidfieldPosition; }
        }
        [SerializeField]
        private Transform[] m_ForwardPosition;
        public Transform[] ForwardPosition
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
            if(m_DefenseLine == null || m_MidfieldLine == null || m_ForwardLine == null || m_GoalKeeperPosition == null)
            {
                Debug.LogError("Please assign all the lines");
                return;
            }
            m_DefensePosition = new  Transform[4];
            m_MidfieldPosition = new  Transform[4];
            m_ForwardPosition = new  Transform[2];
            for (int i = 0; i < m_DefensePosition.Length; i++)
            {
                m_DefensePosition[i] = m_DefenseLine.GetChild(i);
            }
            for (int i = 0; i < m_MidfieldPosition.Length; i++)
            {
                m_MidfieldPosition[i] = m_MidfieldLine.GetChild(i);
            }
            for (int i = 0; i < m_ForwardPosition.Length; i++)
            {
                m_ForwardPosition[i] = m_ForwardLine.GetChild(i);
            }

        }
    



    }


}