
using System;
using Steamworks;
using Unity.Netcode;

namespace FootballSim.Networking
{
    public struct SteamClientInfo
    {
        public ulong SteamId;
        public string SteamName;
        public ulong ClientId;
    }




    public class NetworkConnectionRPCS : NetworkBehaviour
    {

        public static NetworkConnectionRPCS Instance { get; private set; }

        public SteamClientInfo? CurrentSteamClient { get; private set; } = null;

        public event Action<SteamClientInfo> OnSteamPlayerConnected;

        public event Action<SteamClientInfo> OnSteamHostInfoTransmission;
        public event Action OnSteamPlayerDisconnected;

        public event Action OnSteamPlayerReadyAction;
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
            DontDestroyOnLoad(gameObject);
        }

        [Rpc(SendTo.Server)]
        public void NotifySteamPlayerConnectedRpc(ulong t_SteamId, string t_SteamName, ulong t_ClientId)
        {
            SteamClientInfo steamClient = new()
            {
                SteamId = t_SteamId,
                SteamName = t_SteamName,
                ClientId = t_ClientId

            };
            CurrentSteamClient = steamClient;

            NotifySteamClientWithHostInfoClientRpc(SteamClient.SteamId, SteamClient.Name);

            OnSteamPlayerConnected.Invoke(steamClient);
        }

        [ClientRpc]
        public void NotifySteamClientWithHostInfoClientRpc(ulong t_SteamId, string t_SteamName)
        {
            if (IsHost)     return;
           
        
            SteamClientInfo steamClient = new()
            {
                SteamId = t_SteamId,
                SteamName = t_SteamName,
                ClientId = 0

            };
                
            OnSteamHostInfoTransmission.Invoke(steamClient);
            
            
        }

        [Rpc(SendTo.Server)]
        public void NotifySteamPlayerDisconnectedRpc()
        {
            CurrentSteamClient = null;

            OnSteamPlayerDisconnected.Invoke();
        }

        [Rpc(SendTo.Server)]
        public void NotifyClientReadyRpc()
        {
            OnSteamPlayerReadyAction();
        }




    }
}