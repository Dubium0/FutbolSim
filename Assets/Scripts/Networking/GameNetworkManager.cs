
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;


namespace FootballSim.Networking
{
    [RequireComponent(typeof(FacepunchTransport))]
    public class GameNetworkManager : MonoBehaviour
    {
        public static GameNetworkManager Instance { get; private set; } = null;

        private FacepunchTransport m_Transport;

        public Lobby? CurrentLobby { get; private set; } = null;

        public ulong HostId { get { return m_HostId; } }

        private ulong m_HostId;

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

        private void OnEnable()
        {
            m_Transport = GetComponent<FacepunchTransport>();

            SteamMatchmaking.OnLobbyCreated += SteamMatchmaking_OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered += SteamMatchmaking_OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite += SteamMatchmaking_OnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated += SteamMatchmaking_OnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested += SteamFriends_OnGameLobbyJoinRequested;
        }

        private void OnDestroy()
        {
            SteamMatchmaking.OnLobbyCreated -= SteamMatchmaking_OnLobbyCreated;
            SteamMatchmaking.OnLobbyEntered -= SteamMatchmaking_OnLobbyEntered;
            SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
            SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
            SteamMatchmaking.OnLobbyInvite -= SteamMatchmaking_OnLobbyInvite;
            SteamMatchmaking.OnLobbyGameCreated -= SteamMatchmaking_OnLobbyGameCreated;
            SteamFriends.OnGameLobbyJoinRequested -= SteamFriends_OnGameLobbyJoinRequested;

            if (NetworkManager.Singleton == null)
            {
                return;
            }
            NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
            NetworkManager.Singleton.OnServerStopped -= Singleton_OnServerStopped;
            NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;

        }

        private void Singleton_OnServerStopped(bool obj)
        {
            
        }

        private void OnApplicationQuit()
        {
            Disconnected();
        }

        //when you accept the invite or Join on a friend
        private async void SteamFriends_OnGameLobbyJoinRequested(Lobby _lobby, SteamId _steamId)
        {
            RoomEnter joinedLobby = await _lobby.Join();
            if (joinedLobby != RoomEnter.Success)
            {
                Debug.Log("Failed to create lobby");
            }
            else
            {

                CurrentLobby = _lobby;
                //GameManager.instance.ConnectedAsClient();
                Debug.Log("Joined Lobby");
            }
        }

        private void SteamMatchmaking_OnLobbyGameCreated(Lobby _lobby, uint _ip, ushort _port, SteamId _steamId)
        {
            Debug.Log("Lobby was created");
            //GameManager.instance.SendMessageToChat($"Lobby was created", NetworkManager.Singleton.LocalClientId, true);

        }

        //friend send you an steam invite
        private void SteamMatchmaking_OnLobbyInvite(Friend _steamId, Lobby _lobby)
        {
            Debug.Log($"Invite from {_steamId.Name}");
        }

        private void SteamMatchmaking_OnLobbyMemberLeave(Lobby _lobby, Friend _steamId)
        {
            Debug.Log("member leave");
            //GameManager.instance.SendMessageToChat($"{_steamId.Name} has left", _steamId.Id, true);
            //NetworkTransmission.instance.RemoveMeFromDictionaryServerRPC(_steamId.Id);
        }

        private void SteamMatchmaking_OnLobbyMemberJoined(Lobby _lobby, Friend _steamId)
        {
            Debug.Log($"Member joined with ID{_steamId}");
        }

        private void SteamMatchmaking_OnLobbyEntered(Lobby _lobby)
        {
            Debug.Log("I am in on lobby enter!");
            if (NetworkManager.Singleton.IsHost)
            {
                return;
            }
            StartClient(CurrentLobby.Value.Owner.Id);

        }

        private void SteamMatchmaking_OnLobbyCreated(Result _result, Lobby _lobby)
        {
            if (_result != Result.OK)
            {
                Debug.Log("lobby was not created");
                return;
            }
            _lobby.SetPublic();
            _lobby.SetJoinable(true);
            _lobby.SetGameServer(_lobby.Owner.Id);
            Debug.Log($"lobby created FootballSim");
            //NetworkTransmission.instance.AddMeToDictionaryServerRPC(SteamClient.SteamId, "FakeSteamName", NetworkManager.Singleton.LocalClientId); //
        }

        public async void StartHost(int t_MaxMembers, Action<string,SteamId?> t_SuccessCallBack = null, Action t_FailCallBack = null)
        {
            NetworkManager.Singleton.OnServerStarted += Singleton_OnServerStarted;
            NetworkManager.Singleton.OnServerStopped += Singleton_OnServerStopped;
            NetworkManager.Singleton.StartHost();
            //GameManager.instance.myClientId = NetworkManager.Singleton.LocalClientId;
            CurrentLobby = await SteamMatchmaking.CreateLobbyAsync(t_MaxMembers);
            
            

            if (CurrentLobby == null)
            {
                if(t_FailCallBack!=null)
                    t_FailCallBack();
            }
            else
            {
                if(t_SuccessCallBack!=null)
                    t_SuccessCallBack(CurrentLobby?.Owner.Name, CurrentLobby?.Owner.Id);
               
            }
        }

        public void StartClient(SteamId _sId)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += Singleton_OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += Singleton_OnClientDisconnectCallback;
            m_Transport.targetSteamId = _sId;
            //GameManager.instance.myClientId = NetworkManager.Singleton.LocalClientId;
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client has started");
            }
        }

        public void Disconnected()
        {
            CurrentLobby?.Leave();
            CurrentLobby = null;
            if (NetworkManager.Singleton == null)
            {
                return;
            }
            if (NetworkManager.Singleton.IsHost)
            {
                NetworkManager.Singleton.OnServerStarted -= Singleton_OnServerStarted;
            }
            else
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= Singleton_OnClientConnectedCallback;
            }
            NetworkManager.Singleton.Shutdown(true);
            //GameManager.instance.ClearChat();
            //GameManager.instance.Disconnected();
            Debug.Log("disconnected");
            
        }

        private void Singleton_OnClientDisconnectCallback(ulong _cliendId)
        {   
            NetworkConnectionRPCS.Instance.NotifySteamPlayerDisconnectedRpc();
            NetworkManager.Singleton.OnClientDisconnectCallback -= Singleton_OnClientDisconnectCallback;
            if (_cliendId == 0)
            {
                Disconnected();
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }

        private void Singleton_OnClientConnectedCallback(ulong t_CliendId)
        {
            NetworkConnectionRPCS.Instance.NotifySteamPlayerConnectedRpc(SteamClient.SteamId, SteamClient.Name, t_CliendId);
            //GameManager.instance.myClientId = _cliendId;
            ///NetworkTransmission.instance.IsTheClientReadyServerRPC(false, _cliendId);
            Debug.Log($"Client has connected : {t_CliendId}");
        }
 

        private void Singleton_OnServerStarted()
        {
            Debug.Log("Host started");
            //GameManager.instance.HostCreated();
        }


    }

}