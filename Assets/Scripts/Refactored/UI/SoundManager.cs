using UnityEngine;
using System.Collections;

namespace FootballSim.UI
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource m_EffectsSource;
        [SerializeField] private AudioSource m_GoalSource;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip m_BallHitSound;
        [SerializeField] private AudioClip m_GoalSound;
        [SerializeField] private AudioClip m_BallWinSound;
        [SerializeField] private AudioClip m_MatchWhistleSound;
        [SerializeField] private AudioClip m_GoalKickWhistleSound;

        [Header("Pitch Variation")]
        [SerializeField] private float m_BallHitPitchVariation = 0.1f;
        [SerializeField] private float m_GoalPitchVariation = 0.05f;
        [SerializeField] private float m_WhistlePitchVariation = 0.02f;

        private const float NORMAL_VOLUME = 0.1f;
        private const float GOAL_VOLUME = 1f;
        private const float GOAL_DURATION = 3f;
        private const float FADE_DURATION = 1f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (m_GoalSource == null)
            {
                m_GoalSource = gameObject.AddComponent<AudioSource>();
                m_GoalSource.playOnAwake = false;
                m_GoalSource.loop = true;
                m_GoalSource.volume = NORMAL_VOLUME;
            }
        }

        private void Start()
        {
            if (m_GoalSound != null)
            {
                m_GoalSource.clip = m_GoalSound;
                m_GoalSource.time = m_GoalSound.length * 0.4f;
                m_GoalSource.Play();
            }
        }

        private void PlaySoundWithPitch(AudioClip clip, float pitchVariation)
        {
            if (clip != null)
            {
                float originalPitch = m_EffectsSource.pitch;
                m_EffectsSource.pitch = Random.Range(1f - pitchVariation, 1f + pitchVariation);
                m_EffectsSource.PlayOneShot(clip);
                m_EffectsSource.pitch = originalPitch;
            }
        }

        public void PlayBallHitSound()
        {
            PlaySoundWithPitch(m_BallHitSound, m_BallHitPitchVariation);
        }

        public void PlayGoalSound()
        {
            if (m_GoalSound != null)
            {
                StartCoroutine(HandleGoalSound());
            }
        }

        private IEnumerator HandleGoalSound()
        {
            // Fade up
            float startVolume = m_GoalSource.volume;
            float timer = 0;
            while (timer < FADE_DURATION)
            {
                timer += Time.deltaTime;
                m_GoalSource.volume = Mathf.Lerp(startVolume, GOAL_VOLUME, timer / FADE_DURATION);
                yield return null;
            }
            m_GoalSource.volume = GOAL_VOLUME;

            // Wait at goal volume
            yield return new WaitForSeconds(GOAL_DURATION);

            // Fade down
            timer = 0;
            while (timer < FADE_DURATION)
            {
                timer += Time.deltaTime;
                m_GoalSource.volume = Mathf.Lerp(GOAL_VOLUME, NORMAL_VOLUME, timer / FADE_DURATION);
                yield return null;
            }
            m_GoalSource.volume = NORMAL_VOLUME;
        }

        public void PlayBallWinSound()
        {
            PlaySoundWithPitch(m_BallWinSound, m_BallHitPitchVariation);
        }

        public void PlayMatchWhistleSound()
        {
            PlaySoundWithPitch(m_MatchWhistleSound, m_WhistlePitchVariation);
        }

        public void PlayGoalKickWhistleSound()
        {
            PlaySoundWithPitch(m_GoalKickWhistleSound, m_WhistlePitchVariation);
        }
    }
} 