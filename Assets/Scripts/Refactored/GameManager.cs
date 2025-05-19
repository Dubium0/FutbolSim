

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
        private GameObject m_HomeTeamPrefab;
        [SerializeField]
        private GameObject m_AwayTeamPrefab;
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

            switch (m_CurrentGameSettings.GameMode)
            {

                case GameMode.NOT_INIT:
                    Debug.LogError("Game Mod is not set!");
                    break;
                case GameMode.PVA:
                    SetupPvAGame();
                    break;
                case GameMode.PVP_LOCAL:
                    SetupLocalPvPGame();
                    break;
                case GameMode.PVP_ONLINE:
                    var excpectedClient = FootballSim.Networking.NetworkConnectionRPCS.Instance.CurrentSteamClient.Value.ClientId;
                    if (clientsCompleted.FindIndex(value => value == excpectedClient) != -1) SetupOnlinePvPGame(excpectedClient);
                    else Debug.LogError($"Receiving player is not expected player {excpectedClient}!");
                    break;
            }
        }

        private void SetupPvAGame()
        {
            //GameObject pvpPlayer1 = Instantiate(m_PlayerPrefab, new Vector3(2.5f, 2.5f, 0), Quaternion.identity);
            //GameObject pvpPlayer2 = Instantiate(m_PlayerPrefab, new Vector3(-2.5f, 2.5f, 0), Quaternion.identity);
            //pvpPlayer1.GetComponent<NetworkObject>().Spawn();
            //pvpPlayer2.GetComponent<NetworkObject>().Spawn();
            //var player1Script = pvpPlayer1.GetComponent<FootballPlayer>();
            //var player2Script = pvpPlayer2.GetComponent<FootballPlayer>();
            //player1Script.Init(true, false, m_CurrentGameSettings.HomePlayerIndex != -1 ? m_CurrentGameSettings.HomePlayerIndex :  m_CurrentGameSettings.AwayPlayerIndex );
            //player2Script.Init(false, false);
            GameObject homeTeamObj = Instantiate(m_HomeTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AwayTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().Spawn();
            var homeTeamScript = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            var awayTeamScript = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();
            if (m_CurrentGameSettings.HomePlayerIndex != -1)
            {

                homeTeamScript.Init(true, FootballTeam.TeamFlag.Home, m_CurrentGameSettings.HomePlayerIndex);

            }
            else
            {
                homeTeamScript.Init(false, FootballTeam.TeamFlag.Home);
            }

             if (m_CurrentGameSettings.AwayPlayerIndex != -1)
            {

                awayTeamScript.Init(true, FootballTeam.TeamFlag.Away, m_CurrentGameSettings.AwayPlayerIndex);

            }
            else
            {
                awayTeamScript.Init(false, FootballTeam.TeamFlag.Away);
            }



            
        }

        private void SetupLocalPvPGame()
        {


            //GameObject pvpPlayer1 = Instantiate(m_PlayerPrefab, new Vector3(2.5f, 2.5f, 0), Quaternion.identity);
            //GameObject pvpPlayer2 = Instantiate(m_PlayerPrefab, new Vector3(-2.5f, 2.5f, 0), Quaternion.identity);
            //
            //pvpPlayer1.GetComponent<NetworkObject>().Spawn();
            //pvpPlayer2.GetComponent<NetworkObject>().Spawn();
            //
            //var player1Script = pvpPlayer1.GetComponent<FootballPlayer>();
            //var player2Script = pvpPlayer2.GetComponent<FootballPlayer>();
            //player1Script.Init(true, false, m_CurrentGameSettings.HomePlayerIndex);
            //player2Script.Init(true, false, m_CurrentGameSettings.AwayPlayerIndex);
            GameObject homeTeamObj = Instantiate(m_HomeTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AwayTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().Spawn();
            var homeTeamScript = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            var awayTeamScript = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();
            if (m_CurrentGameSettings.HomePlayerIndex != -1)
            {

                homeTeamScript.Init(true, FootballTeam.TeamFlag.Home, m_CurrentGameSettings.HomePlayerIndex);

            }
            else
            {
                homeTeamScript.Init(false, FootballTeam.TeamFlag.Home);
            }

             if (m_CurrentGameSettings.AwayPlayerIndex != -1)
            {

                awayTeamScript.Init(true, FootballTeam.TeamFlag.Away, m_CurrentGameSettings.AwayPlayerIndex);

            }
            else
            {
                awayTeamScript.Init(false, FootballTeam.TeamFlag.Away);
            }
            

        }

        private void SetupOnlinePvPGame(ulong t_ClientId)
        {

            //Debug.Log("SteamClient successfully loaded!");
            //GameObject serverOwnedPlayer = Instantiate(m_PlayerPrefab, new Vector3(-2.5f, 2.5f, 0), Quaternion.identity);
            //NetworkObject serverNetworkObject = serverOwnedPlayer.GetComponent<NetworkObject>();
            //serverNetworkObject.Spawn();
            //
            //GameObject clientOwnedPlayer = Instantiate(m_PlayerPrefab, new Vector3(2.5f, 2.5f, 0), Quaternion.identity);
            //NetworkObject clientNetworkObject = clientOwnedPlayer.GetComponent<NetworkObject>();
            //clientNetworkObject.Spawn();
            //clientNetworkObject.ChangeOwnership(t_ClientId);
            //var serverPlayer = serverOwnedPlayer.GetComponent<FootballPlayer>();
            //var clientPlayer = clientOwnedPlayer.GetComponent<FootballPlayer>();
            //
            //serverPlayer.Init(true, true);
            //clientPlayer.Init(true, true);
            GameObject homeTeamObj = Instantiate(m_HomeTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            GameObject awayTeamObj = Instantiate(m_AwayTeamPrefab, new Vector3(0.0f, 1.0f, 0.0f), Quaternion.identity);
            homeTeamObj.GetComponent<NetworkObject>().Spawn();
            awayTeamObj.GetComponent<NetworkObject>().SpawnWithOwnership(t_ClientId);
            var homeTeamScript = homeTeamObj.GetComponent<FootballTeam.FootballTeam>();
            var awayTeamScript = awayTeamObj.GetComponent<FootballTeam.FootballTeam>();
           
            homeTeamScript.Init(true, FootballTeam.TeamFlag.Home,0,true);
            awayTeamScript.Init(true, FootballTeam.TeamFlag.Away,0,true);
                  
        }

    }

}