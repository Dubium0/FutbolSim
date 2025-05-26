using System;
using System.Collections;
using System.Text.RegularExpressions;
using FootballSim.Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace FootballSim.UI
{

    public class MatchUI : NetworkBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_HomeTeamName;

        [SerializeField]
        private TextMeshProUGUI m_HomeTeamScore;

        [SerializeField]
        private TextMeshProUGUI m_AwayTeamName;

        [SerializeField]
        private TextMeshProUGUI m_AwayTeamScore;

        [SerializeField]
        private TextMeshProUGUI m_MatchTime;
        [SerializeField]
        private UnityEngine.UI.Slider m_HomeTeamStaminaBar;
        [SerializeField]
        private UnityEngine.UI.Slider m_AwayTeamStaminaBar;

        [SerializeField]
        private UnityEngine.UI.Slider m_ShotPowerBar;

        [SerializeField]
        private GameObject m_FirstHalfEndUI;

        [SerializeField]

        private GameObject m_FirstHalfEndUIClient;

        [SerializeField]
        private Button m_ContinueToSecondHalfButton;

        [SerializeField]
        private GameObject m_GameEndUI;

        [SerializeField]
        private TextMeshProUGUI m_GameEndWinnerTeam;

        
        [SerializeField]
        private Button m_ReturnToMainMenuButton;


        [SerializeField]

        private TextMeshProUGUI m_GoldenBallText;
        bool isHomeTeamHuman = false;
        bool isAwayTeamHuman = false;

        public static MatchUI Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void UpdatePowerBar(float normalizedValue)
        {
            m_ShotPowerBar.value = normalizedValue;
        }

        public void UpdateTeamStamina(bool isHomeTeam, float normalizedValue)
        {
            if (isHomeTeam)
            {
                m_HomeTeamStaminaBar.value = normalizedValue;
            }
            else
            {
                m_AwayTeamStaminaBar.value = normalizedValue;
            }
        }

        private void Start()
        {
            m_HomeTeamName.text = "Home";
            m_AwayTeamName.text = "Away";
            MatchManager.Instance.OnScoreChanged += HandleScoreChange;
            MatchManager.Instance.MatchTime.OnValueChanged += HandleMatchTime;
            MatchManager.Instance.OnFirstHalfFinish += HandleFirstHalfEnd;
            MatchManager.Instance.OnGameFinish += HandleGameFinishRpc;
            MatchManager.Instance.OnGoldenBall += HandleOnGoldenBallRpc;

            isHomeTeamHuman = MatchManager.Instance.HomeTeam.IsHumanControlled;
            isAwayTeamHuman = MatchManager.Instance.AwayTeam.IsHumanControlled;

            m_HomeTeamStaminaBar.gameObject.SetActive(isHomeTeamHuman);
            m_AwayTeamStaminaBar.gameObject.SetActive(isAwayTeamHuman);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void HandleOnGoldenBallRpc()
        {
            m_GoldenBallText.gameObject.SetActive(true);
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void HandleGameFinishRpc(FootballTeam.TeamFlag t_WinnerTeam)
        {
            m_GoldenBallText.gameObject.SetActive(false);
            m_GameEndUI.SetActive(true);
            switch (t_WinnerTeam)
            {
                case FootballTeam.TeamFlag.Home:
                    m_GameEndWinnerTeam.text = "Winner is Home Team!";
                    break;
                case FootballTeam.TeamFlag.Away:
                    m_GameEndWinnerTeam.text = "Winner is Away Team!";
                    break;
            }
        }

    
        private void HandleFirstHalfEnd()
        {
            if (m_FirstHalfEndUI != null)
            {
                m_FirstHalfEndUI.SetActive(true);
                HandleFirtsHalfUIChangeRpc(true);
            }
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void HandleFirtsHalfUIChangeRpc(bool t_Enable)
        {
            if (!IsHost)
            {
                
                if (m_FirstHalfEndUIClient != null)
                {
                    m_FirstHalfEndUIClient.SetActive(t_Enable);
                }
            }
        }
        public void OnReturnToMainMenuButtonClicked()
        {
            m_ReturnToMainMenuButton.interactable = false;
            
            StartCoroutine(GoToMainMenuAfterTime(2));

        }

        private IEnumerator GoToMainMenuAfterTime(float t_Time)
        {
            yield return new WaitForSeconds(t_Time);
            SceneManager.LoadScene("MainMenu");
            
        }
        public void OnContinueToSecondHalfButtonClicked()
        {
            m_ContinueToSecondHalfButton.interactable = false;
            StartCoroutine(OnContinueToSecondHalfButtonClickedRoutine());
        }
        private IEnumerator OnContinueToSecondHalfButtonClickedRoutine()
        {
            MatchManager.Instance.PrepeareForSecondHalf();
            
            yield return new WaitForSeconds(1.0f);
            if (m_FirstHalfEndUI != null)
            {
                m_FirstHalfEndUI.SetActive(false);
                HandleFirtsHalfUIChangeRpc(false);
               
            }
        }
        private void HandleMatchTime(int previousValue, int newValue)
        {
            int minutes = newValue / 60;
            int seconds = newValue % 60;

            string minutePrefix = minutes >= 10 ? "" : "0";
            string secondsPrefix = seconds >= 10 ? "" : "0";

            m_MatchTime.text = $"{minutePrefix}{minutes} : {secondsPrefix}{seconds}";
        }

        private void HandleScoreChange(int t_HomeScore, int t_AwayScore)
        {
            if (IsHost)
            {
                UpdateScoresRpc(t_HomeScore, t_AwayScore);
            }
        }


        [Rpc(SendTo.ClientsAndHost)]
        private void UpdateScoresRpc(int t_HomeScore, int t_AwayScore)
        {

            m_HomeTeamScore.text = t_HomeScore.ToString();
            m_AwayTeamScore.text = t_AwayScore.ToString();
        }
    }
}