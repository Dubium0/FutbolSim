
using System;
using System.Collections.Generic;
using FootballSim.Player;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FootballSim
{
    public struct GameStartConfig
    {
        public int HomePlayerIndex;
        public int AwayPlayerIndex;
            
    }

    public enum GameMode
    {   
        NOT_INIT,
        PVA,
        PVP_LOCAL,
        PVP_ONLINE
    }
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; } = null;

        public GameMode GameMode { get { return m_GameMode; } }
        private GameMode m_GameMode = GameMode.NOT_INIT;

        private GameStartConfig m_CurrentGameSettings;
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
            DontDestroyOnLoad(this);
        }

        public void InitiatePVAGame(GameStartConfig t_Config)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("PVA_Arena", LoadSceneMode.Single);
            m_GameMode = GameMode.PVA;
            m_CurrentGameSettings = t_Config;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SetupGame;
        
        }

        private void SetupGame(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            var player = FindAnyObjectByType<FootballPlayer>();
            Debug.Log("Player");
            if (m_CurrentGameSettings.HomePlayerIndex != -1)
            {
                player.Init(true, false, m_CurrentGameSettings.HomePlayerIndex);
            }
            if (m_CurrentGameSettings.AwayPlayerIndex != -1) {
                player.Init(true, false, m_CurrentGameSettings.AwayPlayerIndex);
            }
        }
    }

}