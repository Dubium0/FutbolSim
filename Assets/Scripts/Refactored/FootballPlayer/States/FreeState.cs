using System;
using System.Collections;
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

            m_FootballPlayer.OnBallWinCallback += HandleDribblingTransition;
        }

        public void OnExit()
        {
            m_FootballPlayer.OnBallWinCallback -= HandleDribblingTransition;
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

        public void HandleDribblingTransition(FootballPlayer _)
        {
            m_FootballPlayer.TransitionTo(new DribblingState(m_FootballPlayer));

        }

        private float m_SprintStartTime = 0.0f;
        private bool m_IsSprinting = false;
        private bool m_CanSprint = true;
        public void OnSprintEnter()
        {
            if (!m_CanSprint) return;
            m_CurrentAcceleration = m_FootballPlayer.Data.RunningAcceleration;
            m_FootballPlayer.Rigidbody.maxLinearVelocity = m_FootballPlayer.Data.MaxRunSpeed;

            m_SprintStartTime = Time.time;
            m_IsSprinting = true;
            m_FootballPlayer.StartCoroutine(HandleSprint());
        }

        public void OnSprintExit()
        {
            if (m_IsSprinting)
            {
                m_FootballPlayer.Rigidbody.maxLinearVelocity = m_FootballPlayer.Data.MaxWalkSpeed;
                m_CurrentAcceleration = m_FootballPlayer.Data.WalkingAcceleration;
                m_IsSprinting = false;
                float elapsedTimePercentage = (Time.time - m_SprintStartTime) / m_FootballPlayer.Data.MaxRunningTime;
                m_FootballPlayer.StartCoroutine(HandleSprintCooldown(m_FootballPlayer.Data.RunningCooldown * elapsedTimePercentage));

            }
        }
        private IEnumerator HandleSprintCooldown(float t_TimeToWait)
        {
            m_CanSprint = false;
            yield return new WaitForSeconds(t_TimeToWait);
            m_CanSprint = true;
        }
        private IEnumerator HandleSprint()
        {
            yield return new WaitForSeconds(m_FootballPlayer.Data.MaxRunningTime);
            if (m_IsSprinting)
            {
                m_FootballPlayer.Rigidbody.maxLinearVelocity = m_FootballPlayer.Data.MaxWalkSpeed;
                m_CurrentAcceleration = m_FootballPlayer.Data.WalkingAcceleration;
                m_IsSprinting = false;
                HandleSprintCooldown(m_FootballPlayer.Data.RunningCooldown);
            }
           
        }

        public void OnPassActionEnter()
        {
            //for now empty
        }

        public void OnPassActionExit()
        {
              //for now empty
        }

        public void OnThroughPassActionEnter()
        {
            //for now empty
        }

        public void OnThroughPassActionExit()
        {
             //for now empty
        }

        public void OnLobPassActionEnter()
        {
            //for now empty
        }

        public void OnLobPassActionExit()
        {
            //for now empty
        }

        public void OnShootActionEnter()
        {
           //for now empty
        }

        public void OnShootActionExit()
        {
            //for now empty
        }
    }

}







