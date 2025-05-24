

using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;

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

        private void Start()
        {
            m_HomeTeamName.text = "Home";
            m_AwayTeamName.text = "Away";
            MatchManager.Instance.OnScoreChanged += HandleScoreChange;
            MatchManager.Instance.MatchTime.OnValueChanged += HandleMatchTime;
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