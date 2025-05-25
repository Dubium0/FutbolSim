using System;
using System.Collections;
using UnityEngine;
using FootballSim.UI;

namespace FootballSim.Player
{

    public class DribblingState : IPlayerState
    {
        public FootballPlayer FootballPlayer { get { return m_FootballPlayer; } }

        private FootballPlayer m_FootballPlayer;

        private float m_CurrentAcceleration;

        private float m_BallHitTimer = 0;
        private float m_BallHitMaxPowerReachTime = 0.5f; // seconds
        private bool m_IsHittingBall = false;
        private bool m_CanHitTheBall = true;
        public DribblingState(FootballPlayer t_player)
        {
            m_FootballPlayer = t_player;
        }
        public void OnEnter()
        {
            m_FootballPlayer.Rigidbody.maxLinearVelocity = m_FootballPlayer.Data.MaxWalkSpeed;
            m_CurrentAcceleration = m_FootballPlayer.Data.WalkingAcceleration;

            m_FootballPlayer.OnBallLoseCallback += HandleFreeStateTransition;
        }

        public void OnExit()
        {
            m_FootballPlayer.OnBallLoseCallback -= HandleFreeStateTransition;
        }

        public void OnFixedUpdate()
        {
            Move();
        }

        public void OnUpdate()
        {
            //for now empty
        }

        private void HandleFreeStateTransition(FootballPlayer _)
        {
            m_FootballPlayer.TransitionTo(new FreeState(m_FootballPlayer));
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
            float startTime = Time.time;
            while (Time.time - startTime < t_TimeToWait)
            {
                float recoveryPercentage = (Time.time - startTime) / t_TimeToWait;
                bool isHomeTeam = m_FootballPlayer.TeamFlag == MatchManager.Instance.HomeTeam.TeamFlag;
                MatchUI.Instance.UpdateTeamStamina(isHomeTeam, recoveryPercentage);
                yield return null;
            }
            m_CanSprint = true;
            bool isHomeTeamFinal = m_FootballPlayer.TeamFlag == MatchManager.Instance.HomeTeam.TeamFlag;
            MatchUI.Instance.UpdateTeamStamina(isHomeTeamFinal, 1f);
        }
        private IEnumerator HandleSprint()
        {
            while (m_IsSprinting)
            {
                float elapsedTimePercentage = (Time.time - m_SprintStartTime) / m_FootballPlayer.Data.MaxRunningTime;
                bool isHomeTeam = m_FootballPlayer.TeamFlag == MatchManager.Instance.HomeTeam.TeamFlag;
                MatchUI.Instance.UpdateTeamStamina(isHomeTeam, 1f - elapsedTimePercentage);
                
                if (elapsedTimePercentage >= 1f)
                {
                    break;
                }
                yield return null;
            }
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
            InitiateHitBall(() =>
            {
                Quaternion rotation = Quaternion.AngleAxis(m_FootballPlayer.Data.PassAngle, m_FootballPlayer.transform.right);

                Vector3 rotatedForwardVector = rotation * m_FootballPlayer.transform.forward;
                Football.Football.Instance.HitBall(
                    rotatedForwardVector.normalized,
                    m_FootballPlayer.Data.MaximumPassPower,
                    m_FootballPlayer);
            });
        }

        public void OnPassActionExit()
        {
            float elapsedTimePercentage = (Time.time - m_BallHitTimer) / m_BallHitMaxPowerReachTime;
            FinalizeHitBall(() =>
            {

                Quaternion rotation = Quaternion.AngleAxis(m_FootballPlayer.Data.PassAngle, m_FootballPlayer.transform.right);

                Vector3 rotatedForwardVector = rotation * m_FootballPlayer.transform.forward;
                Football.Football.Instance.HitBall(
                    rotatedForwardVector.normalized,
                    Mathf.Lerp(
                        m_FootballPlayer.Data.MinimumPassPower,
                        m_FootballPlayer.Data.MaximumPassPower,
                        elapsedTimePercentage),
                    m_FootballPlayer);

            });
        }

        public void OnThroughPassActionEnter()
        {
            InitiateHitBall(() =>
            {
                Football.Football.Instance.HitBall(
                    m_FootballPlayer.transform.forward,
                    m_FootballPlayer.Data.MaximumThrougPassPower,
                    m_FootballPlayer);
            });
        }

        public void OnThroughPassActionExit()
        {
            float elapsedTimePercentage = (Time.time - m_BallHitTimer) / m_BallHitMaxPowerReachTime;
            FinalizeHitBall(() =>
            {
                Football.Football.Instance.HitBall(
                      m_FootballPlayer.transform.forward,
                    Mathf.Lerp(
                        m_FootballPlayer.Data.MinimumThroughPassPower,
                        m_FootballPlayer.Data.MaximumThrougPassPower,
                        elapsedTimePercentage),
                    m_FootballPlayer);

            });
        }

        public void OnLobPassActionEnter()
        {
            InitiateHitBall(() =>
            {
                Quaternion rotation = Quaternion.AngleAxis(m_FootballPlayer.Data.LobPassAngle, m_FootballPlayer.transform.right);

                Vector3 rotatedForwardVector = rotation * m_FootballPlayer.transform.forward;
                Football.Football.Instance.HitBall(
                    rotatedForwardVector.normalized,
                    m_FootballPlayer.Data.MaximumLobPassPower,
                    m_FootballPlayer);
            });
        }

        public void OnLobPassActionExit()
        {
            float elapsedTimePercentage = (Time.time - m_BallHitTimer) / m_BallHitMaxPowerReachTime;
            FinalizeHitBall(() =>
            {
                Quaternion rotation = Quaternion.AngleAxis(m_FootballPlayer.Data.LobPassAngle, m_FootballPlayer.transform.right);

                Vector3 rotatedForwardVector = rotation * m_FootballPlayer.transform.forward;
                Football.Football.Instance.HitBall(
                    rotatedForwardVector.normalized,
                    Mathf.Lerp(
                        m_FootballPlayer.Data.MinimumLobPassPower,
                        m_FootballPlayer.Data.MaximumLobPassPower,
                        elapsedTimePercentage),
                    m_FootballPlayer);

            });
        }

        public void OnShootActionEnter()
        {
            InitiateHitBall(() =>
            {
                Quaternion rotation = Quaternion.AngleAxis(m_FootballPlayer.Data.ShootAngle, m_FootballPlayer.transform.right);

                Vector3 rotatedForwardVector = rotation * m_FootballPlayer.transform.forward;
                Football.Football.Instance.HitBall(
                    rotatedForwardVector,
                    m_FootballPlayer.Data.MaximumShootPower,
                    m_FootballPlayer);
            });

        }

        public void OnShootActionExit()
        {   
            float elapsedTimePercentage = (Time.time - m_BallHitTimer) / m_BallHitMaxPowerReachTime;

            FinalizeHitBall(() =>
            {
                Quaternion rotation = Quaternion.AngleAxis(m_FootballPlayer.Data.ShootAngle, m_FootballPlayer.transform.right);

                Vector3 rotatedForwardVector = rotation * m_FootballPlayer.transform.forward;
                Football.Football.Instance.HitBall(
                    rotatedForwardVector,
                    Mathf.Lerp(
                        m_FootballPlayer.Data.MinimumShootPower,
                        m_FootballPlayer.Data.MaximumShootPower,
                        elapsedTimePercentage),
                    m_FootballPlayer);

            });

        }

        private void InitiateHitBall(Action t_OnMaxPowerReach)
        {
            if (m_FootballPlayer.IsTheOwnerOfTheBall && !m_IsHittingBall && m_CanHitTheBall)
            {
                m_IsHittingBall = true;
                m_BallHitTimer = Time.time;
                m_FootballPlayer.StartCoroutine(OnMaxPowerReachTime(t_OnMaxPowerReach));
                MatchUI.Instance.UpdatePowerBar(0f);
            }
        }

        private void FinalizeHitBall(Action t_BallHitAction)
        {
            if (m_FootballPlayer.IsTheOwnerOfTheBall && m_IsHittingBall && m_CanHitTheBall)
            {
                float elapsedTimePercentage = (Time.time - m_BallHitTimer) / m_BallHitMaxPowerReachTime;
                m_FootballPlayer.SetKickBallTrigger();
                m_FootballPlayer.FootballPlayerAnimation.OnBallTouchEvent += t_BallHitAction;
                m_IsHittingBall = false;
                m_FootballPlayer.StartCoroutine(WaitForBallHitCooldown());
                m_FootballPlayer.StartCoroutine(WaitBeforeResettingPowerBar(elapsedTimePercentage));
            }
        }

        private IEnumerator WaitBeforeResettingPowerBar(float finalPower)
        {
            yield return new WaitForSeconds(1f);
            MatchUI.Instance.UpdatePowerBar(0f);
        }

        private IEnumerator OnMaxPowerReachTime(Action t_Action)
        {
            while (m_IsHittingBall)
            {
                float elapsedTimePercentage = (Time.time - m_BallHitTimer) / m_BallHitMaxPowerReachTime;
                MatchUI.Instance.UpdatePowerBar(elapsedTimePercentage);
                yield return null;
            }
            FinalizeHitBall(t_Action);
        }
        private float m_BallHitCooldown = 0.5f;
        private IEnumerator WaitForBallHitCooldown()
        {
            m_CanHitTheBall = false;
            yield return new WaitForSeconds(m_BallHitCooldown);
            m_CanHitTheBall = true;
        }
        
        

    }

}







