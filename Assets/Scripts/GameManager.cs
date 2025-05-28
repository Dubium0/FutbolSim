

using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FootballSim
{
    public struct GameStartConfig
    {
        public int HomePlayerIndex;
        public int AwayPlayerIndex;

        public GameMode GameMode;
            
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

        public void InitiateGame(GameStartConfig t_Config)
        {
            if (IsHost)
            {
                NetworkManager.Singleton.SceneManager.LoadScene("PVA_Arena", LoadSceneMode.Single);
                m_CurrentGameSettings = t_Config;
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SetupGame;

            }

        }


        private void SetupGame(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {

            switch (m_CurrentGameSettings.GameMode)
            {

                case GameMode.NOT_INIT:
                    Debug.LogError("Game Mod is not set!");
                    break;
                case GameMode.PVA:
                    MatchManager.Instance.SetupLocalPVA(m_CurrentGameSettings);
                    break;
                case GameMode.PVP_LOCAL:
                    MatchManager.Instance.SetupLocalPVP(m_CurrentGameSettings);
                    break;
                case GameMode.PVP_ONLINE:
                    var excpectedClient = FootballSim.Networking.NetworkConnectionRPCS.Instance.CurrentSteamClient.Value.ClientId;
                    if (clientsCompleted.FindIndex(value => value == excpectedClient) != -1)
                      MatchManager.Instance.SetupOnlinePVP(m_CurrentGameSettings, excpectedClient);
                    else Debug.LogError($"Receiving player is not expected player {excpectedClient}!");
                    break;
            }
        }


     

        
    }

}