

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
        [SerializeField]
        private GameObject m_PlayerPrefab;
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
            //placeholder logic

            bool isOnline = m_CurrentGameSettings.GameMode == GameMode.PVP_ONLINE ? true : false;
            var excpectedClient = FootballSim.Networking.NetworkConnectionRPCS.Instance.CurrentSteamClient.Value.ClientId;
            if (clientsCompleted.FindIndex(value => value == excpectedClient) != -1)
            {
                Debug.Log("SteamClient successfully loaded!");
                GameObject serverOwnedPlayer = Instantiate(m_PlayerPrefab, new Vector3(-2.5f, 2.5f, 0), Quaternion.identity);
                NetworkObject serverNetworkObject = serverOwnedPlayer.GetComponent<NetworkObject>();
                serverNetworkObject.Spawn();

                GameObject clientOwnedPlayer = Instantiate(m_PlayerPrefab, new Vector3(2.5f, 2.5f, 0), Quaternion.identity);
                NetworkObject clientNetworkObject = clientOwnedPlayer.GetComponent<NetworkObject>();
                clientNetworkObject.Spawn();
                clientNetworkObject.ChangeOwnership(excpectedClient);
                var serverPlayer = serverOwnedPlayer.GetComponent<FootballPlayer>();
                var clientPlayer = clientOwnedPlayer.GetComponent<FootballPlayer>();

                serverPlayer.Init(true, true);
                clientPlayer.Init(true, true);

            }
            
          
        }
        


    }

}