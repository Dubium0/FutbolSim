using System;
using System.Collections.Generic;
using FootballSim.Networking;
using Steamworks;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


namespace FootballSim.UI
{
    public class MainMenu : MonoBehaviour
    {

        [SerializeField]
        private Transform m_GoBackButton;

        [SerializeField]
        private Transform m_MenuOptions;

        [SerializeField]
        private Transform m_ChooseVsOptions;

        [SerializeField]
        private Transform m_ChooseLocationOptions;

        [SerializeField]
        private Transform m_ChooseOnlineOptions;

        [SerializeField]
        private Transform m_HostMenu;
        [SerializeField]
        private TextMeshProUGUI m_HostMenu_HostName;

        [SerializeField]
        private TextMeshProUGUI m_HostMenu_HostID;


        [SerializeField]
        private Transform m_HostMenu_WaitingText;

        [SerializeField]
        private Transform m_HostMenu_ClientHoverHold;

        [SerializeField]
        private TextMeshProUGUI m_HostMenu_ClientName;

        [SerializeField]
        private Button m_HostMenu_StartButton;

        [SerializeField]
        private TMP_InputField m_JoinMenu_HostID;

        [SerializeField]
        private Transform m_JoinMenu;

        [SerializeField]
        private Transform m_WaitScreen;
        [SerializeField]
        private TextMeshProUGUI m_WaitScreen_Text;



        [SerializeField]
        private Transform m_HostMenuAsClient;

        [SerializeField]
        private TextMeshProUGUI m_HostMenuAsClient_HostName;

        [SerializeField]
        private TextMeshProUGUI m_HostMenuAsClient_HostID;

        [SerializeField]
        private TextMeshProUGUI m_HostMenuAsClient_ClientName;

        [SerializeField]
        private Button m_HostMenuAsClient_ReadyButton;

        [SerializeField]
        private Transform m_SelectSideMenu;

        private Transform m_CurrentDisplay;

        Stack<Transform> m_NavigationHistory = new Stack<Transform>();

        private Queue<Action> m_GoBackCallBacks = new Queue<Action>();


        private void Awake()
        {
            m_CurrentDisplay = m_MenuOptions;
           
        }

        private void Update()
        {
            if (m_NavigationHistory.Count > 0)
            {
                m_GoBackButton.gameObject.SetActive(true);
            }
            else
            {
                m_GoBackButton.gameObject.SetActive(false);
            }
        }

        public void OnPlayButtonPressed()
        {
    
            TransitionIntoDisplay(m_ChooseVsOptions);
          
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
            GameNetworkManager.Instance.StartHost(1);
            TransitionIntoDisplay(m_SelectSideMenu);
            m_GoBackCallBacks.Enqueue(() =>
            {
                  GameNetworkManager.Instance.Disconnected();

            });

        }

        public void OnPlayerVsPlayerButtonPressed()
        {

            TransitionIntoDisplay(m_ChooseLocationOptions);

        }

        public void OnOnlineButtonPressed()
        {

            TransitionIntoDisplay(m_ChooseOnlineOptions);

        }
        public void OnLocalButtonPressed()
        {


        }

        public void OnHostButtonPressed()
        {
            m_WaitScreen_Text.text = "Creating the Lobby ...";
            TransitionIntoDisplay(m_WaitScreen);
            // this point just wait
            m_GoBackButton.gameObject.SetActive(false);
            GameNetworkManager.Instance.StartHost(
            2,
            (string t_HostName, SteamId? t_SteamId) =>
            {
                OnGoBackButtonPressed();
                TransitionIntoDisplay(m_HostMenu);

                m_GoBackButton.gameObject.SetActive(true);
                m_HostMenu_StartButton.interactable = false;
                m_HostMenu_HostName.text = t_HostName;
                m_HostMenu_HostID.text = t_SteamId?.Value.ToString();

                Action<SteamClientInfo> steamClientConnectCallBack = clientInfo =>
                {
                    m_HostMenu_WaitingText.gameObject.SetActive(false);
                    m_HostMenu_ClientHoverHold.gameObject.SetActive(true);
                    m_HostMenu_ClientName.text = clientInfo.SteamName;
                };

                Action steamClientReadyCallBack = () =>
                {
                    m_HostMenu_StartButton.interactable = true; 
                };

                Action steamClienDisconnectCallback = () =>
                {
                    m_HostMenu_WaitingText.gameObject.SetActive(true);
                    m_HostMenu_ClientName.text = "Place Holder";
                    m_HostMenu_ClientHoverHold.gameObject.SetActive(false);
                };
                NetworkConnectionRPCS.Instance.OnSteamPlayerConnected += steamClientConnectCallBack;
                NetworkConnectionRPCS.Instance.OnSteamPlayerDisconnected += steamClienDisconnectCallback;
                NetworkConnectionRPCS.Instance.OnSteamPlayerReadyAction += steamClientReadyCallBack;
                m_GoBackCallBacks.Enqueue(() =>
                {
                    NetworkConnectionRPCS.Instance.OnSteamPlayerConnected -= steamClientConnectCallBack;
                    NetworkConnectionRPCS.Instance.OnSteamPlayerDisconnected -= steamClienDisconnectCallback;
                     NetworkConnectionRPCS.Instance.OnSteamPlayerReadyAction -= steamClientReadyCallBack;
                    GameNetworkManager.Instance.Disconnected();

                });
            },
            () =>
            {
                OnGoBackButtonPressed();
            }
            );
        }



        public void OnConnectButtonPressed()
        {
            TransitionIntoDisplay(m_JoinMenu);
        }

        public void OnHostStartButtonPressed()
        {
            FootballSim.GameManager.Instance.InitiateGame(new()
            {
                HomePlayerIndex = 0,
                AwayPlayerIndex = -1,
                GameMode = GameMode.PVP_ONLINE
            });
        }

        public void OnClientJoinButtonPressed()
        {
            var refId = m_JoinMenu_HostID.text;


            if (ulong.TryParse(refId, out ulong result))
            {
                SteamId steamId = result;
                Debug.Log($"Trying to connect to Steam ID {steamId}");
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientJoinHappened;
               
                GameNetworkManager.Instance.StartClient(steamId);
                m_WaitScreen_Text.text = "Trying to connect to Host ...";
                TransitionIntoDisplay(m_WaitScreen);
           
                m_GoBackButton.gameObject.SetActive(false);
            }

        }

        private void OnClientJoinHappened(ulong t_CliendId)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientJoinHappened;
            OnGoBackButtonPressed();
            TransitionIntoDisplay(m_HostMenuAsClient);
            m_HostMenuAsClient_ClientName.text = SteamClient.Name;
            Action<SteamClientInfo> hostInfoRecievedCallback = info =>
            {
                m_HostMenuAsClient_HostName.text = info.SteamName;
                m_HostMenuAsClient_HostID.text = info.SteamId.ToString(); 

            };

            NetworkConnectionRPCS.Instance.OnSteamHostInfoTransmission += hostInfoRecievedCallback;
            m_GoBackCallBacks.Enqueue(
            () =>
            {
                GameNetworkManager.Instance.Disconnected();
                NetworkConnectionRPCS.Instance.OnSteamHostInfoTransmission -= hostInfoRecievedCallback;
            });
            m_GoBackButton.gameObject.SetActive(true);

        }

        public void OnReadyButtonPressed()
        {
            NetworkConnectionRPCS.Instance.NotifyClientReadyRpc();
            m_HostMenuAsClient_ReadyButton.enabled = false;
        }



        public void OnGoBackButtonPressed()
        {

            m_CurrentDisplay.gameObject.SetActive(false);
            m_CurrentDisplay = m_NavigationHistory.Pop();
            m_CurrentDisplay.gameObject.SetActive(true);

            while (m_GoBackCallBacks.Count > 0)
            {
                var callback = m_GoBackCallBacks.Dequeue();
                callback();
            }
        }


        private void TransitionIntoDisplay(Transform t_TargetDisplay)
        {
            m_NavigationHistory.Push(m_CurrentDisplay);
            m_CurrentDisplay.gameObject.SetActive(false);
            m_CurrentDisplay = t_TargetDisplay;
            m_CurrentDisplay.gameObject.SetActive(true);

        }
    

    }


}