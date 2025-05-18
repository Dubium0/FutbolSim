using UnityEngine;

namespace FootballSim.Player
{

    public class FreeState : IPlayerState
    {
        public FootballPlayer FootballPlayer { get { return m_FootballPlayer; } }

        private FootballPlayer m_FootballPlayer;

        private float m_CurrentAcceleration;
        public FreeState(FootballPlayer t_player)
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
            //for now empty
        }

        public void OnSprintEnter()
        {
             //for now empty
        }

        public void OnSprintExit()
        {
             //for now empty
        }

        public void OnLowActionAEnter()
        {
            m_FootballPlayer.Animator.SetTrigger("BallHit");
        }

        public void OnLowActionAExit()
        {
             //for now empty
        }

        public void OnLowActionBEnter()
        {
             //for now empty
        }

        public void OnLowActionBExit()
        {
             //for now empty
        }

        public void OnHighActionAEnter()
        {
            //for now empty
        }

        public void OnHighActionAExit()
        {
            //for now empty
        }

        public void OnHighActionBEnter()
        {
           //for now empty
        }

        public void OnHighActionBExit()
        {
            //for now empty
        }
    }

}







