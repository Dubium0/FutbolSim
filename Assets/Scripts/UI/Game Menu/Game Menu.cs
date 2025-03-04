using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameMenu : NetworkBehaviour
{
    public Transform gameMenuCanvas;
    public TextMeshProUGUI infoText;
    public Transform startGameButton;

    

    private ulong? connectedClientId = null;


    private void Awake()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("Hello from client");
            infoText.text = "Waiting for host to start the game";
        }
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("Server is initiating callbacks");
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        }
    }
   
    [Rpc(SendTo.ClientsAndHost,RequireOwnership = false)]
    private void ClientInformGameIsStartedRpc()
    {
        if (NetworkManager.Singleton.IsClient)
        {
            gameMenuCanvas.gameObject.SetActive(false);

        }
    }


    public void OnStartGameButton()
    {
        if(NetworkManager.Singleton.IsServer)
        {

            if (connectedClientId == null) return;
            ClientInformGameIsStartedRpc();
            GameStartConfig config = new();
            config.isOnline = true;
            config.clientId = (ulong)connectedClientId;
           
            gameMenuCanvas.gameObject.SetActive(false);
            GameManager.Instance.StartGame(config);

        }

    }

    public void OnGamePause()
    {
        if(NetworkManager.Singleton.IsServer) {
        GameManager.Instance.PauseGame();
        }
    }

    public void OnResumeGameButton()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            GameManager.Instance.ResumeGame();
        }
    }

    public void OnReplayButton()
    {
        
    }

    public void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
            if(connectedClientId == null || connectedClientId == clientId)
            {
                infoText.text = $"Client with ID : {clientId} is connected!";
                 connectedClientId = clientId;
                startGameButton.gameObject.SetActive(true);
                Debug.Log("A client connected");
            }
            else 
            {
                NetworkManager.Singleton.DisconnectClient(clientId);
                Debug.Log("A client Disconnected");
            }
        }
    }
    public void OnClientDisconnect(ulong clientId)
    {
        if (NetworkManager.Singleton.IsServer)
        {
           if( clientId ==  connectedClientId)
            {
                connectedClientId = null;
                Debug.Log("A client Disconnected 2");
                infoText.text = $"Waiting for client to connect ...";
                startGameButton.gameObject.SetActive(false);
            }
        }
    }
}
