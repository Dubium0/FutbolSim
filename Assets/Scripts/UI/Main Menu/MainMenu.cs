using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{

    [SerializeField]
    private Transform goBackButton;

    [SerializeField]
    private Transform menuOptions;

    [SerializeField]
    private Transform chooseVsOptions;

    [SerializeField]
    private Transform chooseLocationOptions;

    [SerializeField]
    private Transform chooseOnlineOptions;

    private Transform currentDisplay;
    
    Stack<Transform> navigationHistory = new Stack<Transform>();


    private void Awake()
    {
        currentDisplay = menuOptions;
    }

    private void Update()
    {
        if(navigationHistory.Count > 0)
        {
            goBackButton.gameObject.SetActive(true);
        }
        else
        {
            goBackButton.gameObject.SetActive(false);
        }
    }

    public void OnPlayButtonPressed()
    {
        menuOptions.gameObject.SetActive(false);
        chooseVsOptions.gameObject.SetActive(true);
        currentDisplay = chooseVsOptions;
        navigationHistory.Push(menuOptions);

    }

    public void OnSettingsButtonPressed()
    {
        //do nothing for now

    }

    public void OnExitButtonPressed()
    {
        Application.Quit();

        #if UNITY_EDITOR
               
            UnityEditor.EditorApplication.isPlaying = false;
        #endif

    }

    public void OnPlayerVsAIButtonPressed()
    {
        SceneManager.LoadScene(2);

    }

    public void OnPlayerVsPlayerButtonPressed()
    {
        chooseVsOptions.gameObject.SetActive(false);
        chooseLocationOptions.gameObject.SetActive(true);
        currentDisplay = chooseLocationOptions;
        navigationHistory.Push(chooseVsOptions);

    }

    public void OnOnlineButtonPressed()
    {
        chooseLocationOptions.gameObject.SetActive(false);
        chooseOnlineOptions.gameObject.SetActive(true);
        currentDisplay = chooseOnlineOptions;
        navigationHistory.Push(chooseLocationOptions);
    }
    public void OnLocalButtonPressed()
    {
        SceneManager.LoadScene(2);

    }

    public void OnHostButtonPressed()
    {

        SteamNetworkManager.Instance.StartHost(2);
        Debug.Log("In host");
        NetworkManager.Singleton.SceneManager.LoadScene("Networked Player Vs Player",LoadSceneMode.Single);
    }
    public void OnConnectButtonPressed()
    {
        var id =  NetworkManager.Singleton.GetComponent<FacepunchTransport>().targetSteamId;
        SteamNetworkManager.Instance.StartClient(id );
       
    }

    public void OnGoBackButtonPressed()
    {

        currentDisplay.gameObject.SetActive(false);
        currentDisplay = navigationHistory.Pop();
        currentDisplay.gameObject.SetActive(true);
    }

}
