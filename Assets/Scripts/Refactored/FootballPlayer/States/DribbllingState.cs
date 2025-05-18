using UnityEngine;

namespace FootballSim.Player
{

    public class DribblingState : IPlayerState
    {
        public FootballPlayer FootballPlayer { get { return m_FootballPlayer; } }

        private FootballPlayer m_FootballPlayer;

        private float m_CurrentAcceleration;
        public DribblingState(FootballPlayer t_player)
        {
            m_FootballPlayer = t_player;
        }
        public void OnEnter()
        {
            m_FootballPlayer.Rigidbody.maxLinearVelocity = m_FootballPlayer.Data.MaxWalkSpeed;
            m_CurrentAcceleration = m_FootballPlayer.Data.WalkingAcceleration;
        }

        public void OnExit()
        {
           //for now empty
        }

        public void OnFixedUpdate()
        {
            Move();
        }

        public void OnUpdate()
        {
            //for now empty
        }

        private void Move()
        {
            var inputVector = m_FootballPlayer.InputSource.MovementVector;

            m_FootballPlayer.Rigidbody.AddForce(inputVector * m_CurrentAcceleration, ForceMode.Acceleration);

            if (inputVector.magnitude > 0)
            {
                m_FootballPlayer.Transform.forward = MathExtra.MoveTowards(m_FootballPlayer.Transform.forward, inputVector, 1 / m_FootballPlayer.Data.RotationTime);
            }
        }

        public void HandleTransition()
        {
            throw new System.NotImplementedException();
        }

        public void OnSprintEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnSprintExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionAEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionAExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionBEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnLowActionBExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionAEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionAExit()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionBEnter()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighActionBExit()
        {
            throw new System.NotImplementedException();
        }
    }

}







