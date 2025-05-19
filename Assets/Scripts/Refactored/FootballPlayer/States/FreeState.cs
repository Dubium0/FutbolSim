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

        private float m_ShootTimer = 0;
        private float m_ShootMaxPowerReachTime = 0.5f; // seconds
        private bool m_IsShooting = false;
        public void OnPassActionEnter()
        {
            if (m_FootballPlayer.IsTheOwnerOfTheBall)
            {
                m_IsShooting = true;
                m_ShootTimer = Time.time;
                m_FootballPlayer.StartCoroutine(ShootPowerAdjustRoutine());


            }
        }

        public void OnPassActionExit()
        {
            if (m_FootballPlayer.IsTheOwnerOfTheBall && m_IsShooting)
            {
                float elapsedTime = Time.time - m_ShootTimer;

                float clampValue01 = Mathf.Clamp(elapsedTime, 0, m_ShootMaxPowerReachTime) / m_ShootMaxPowerReachTime;
                m_FootballPlayer.Animator.SetTrigger("BallHit");
                m_FootballPlayer.FootballPlayerAnimation.OnBallTouchEvent +=  () =>
                {
                    HitBallLogic(Mathf.Lerp(m_FootballPlayer.Data.MinimumShootPower, m_FootballPlayer.Data.MaximumShootPower, clampValue01) );
                    m_IsShooting = false;
                };
            }
            m_IsShooting = false;
        }

        
        private IEnumerator ShootPowerAdjustRoutine() {
            yield return new WaitForSeconds(m_ShootMaxPowerReachTime);
            if (m_IsShooting)
            {   
                m_FootballPlayer.Animator.SetTrigger("BallHit");
                m_FootballPlayer.FootballPlayerAnimation.OnBallTouchEvent +=  () =>
                {
                    HitBallLogic(m_FootballPlayer.Data.MaximumShootPower);
                    m_IsShooting = false;
                };
               
            }
        }
        
        
        private void HitBallLogic(float t_ShootPower)
        {

            if (m_FootballPlayer.IsTheOwnerOfTheBall)
            {

                Football.Football.Instance.HitBall(
                    m_FootballPlayer.transform.forward,
                    t_ShootPower,
                    m_FootballPlayer);

            }
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







