using Unity.Netcode;
using UnityEngine;

public class NetworkedGameManager : NetworkBehaviour
{
    private bool clientConnected = false;

    private void Awake()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }
 
    private void OnClientConnected(ulong clientId)
    {
        if (!clientConnected)
        {
            clientConnected = true;
            Debug.Log($"Client {clientId} connected.");
        }
        else
        {
            Debug.Log("Another client tried to join, but the game is full.");
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
    }

    
    

}
