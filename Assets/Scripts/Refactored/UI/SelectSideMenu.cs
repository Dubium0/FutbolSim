using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text;
using DG.Tweening;
using TMPro;

public class SelectSideMenu : MonoBehaviour
{
    // UI Elements
    [SerializeField] private GameObject player1Side;
    [SerializeField] private GameObject player2Side;
    [SerializeField] private TextMeshProUGUI player1ControlsText;
    [SerializeField] private TextMeshProUGUI player2ControlsText;
    [SerializeField] private GameObject leftReady;
    [SerializeField] private GameObject rightReady;

    // Player Positions
    [SerializeField] private Transform player1MTarget, player2MTarget;
    [SerializeField] private Transform player1RTarget, player2RTarget;
    [SerializeField] private Transform player1LTarget, player2LTarget;

    // State
    private bool leftSideReadyState, rightSideReadyState;
    private bool isInputMapSwapped;
    private float moveAnimationDuration = 0.5f;
    private float analogThreshold = 0.5f;
    private bool[] analogLeftState = new bool[2];
    private bool[] analogRightState = new bool[2];

    // Player tracking
    private class PlayerInfo
    {
        public InputDevice Device;
        public string DeviceType;
        public int PlayerIndex;
        public bool IsReady;
        public Transform CurrentPosition;
        
        public PlayerInfo(InputDevice device, string deviceType, int playerIndex)
        {
            Device = device;
            DeviceType = deviceType;
            PlayerIndex = playerIndex;
            IsReady = false;
        }
    }
    
    private List<PlayerInfo> connectedPlayers = new List<PlayerInfo>(2);
    private List<PlayerInfo> readyPlayers = new List<PlayerInfo>(2);

    private void OnEnable() => InputSystem.onDeviceChange += OnInputDeviceChange;
    private void OnDisable() => InputSystem.onDeviceChange -= OnInputDeviceChange;
    
    void Start()
    {
        InitializeUI();
        DetectInputDevices();
        UpdateControlSchemeTexts();
    }

    private void InitializeUI()
    {
        if (player1Side) player1Side.SetActive(false);
        if (player2Side) player2Side.SetActive(false);
        if (leftReady) leftReady.SetActive(false);
        if (rightReady) rightReady.SetActive(false);
        
        for (int i = 0; i < 2; i++)
        {
            analogLeftState[i] = false;
            analogRightState[i] = false;
        }
    }

    void Update()
    {
        ProcessPlayerInput();
    }

    private void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            string deviceType = GetDeviceType(device);
            Debug.Log($"[Device] Connected: {deviceType} (\"{device.displayName}\")");
            AssignDeviceToPlayer(device, deviceType);
        }
        else if (change == InputDeviceChange.Removed)
        {
            Debug.Log($"[Device] Disconnected: (\"{device.displayName}\")");
            RemoveDevice(device);
        }
    }

    private string GetDeviceType(InputDevice device)
    {
        string displayName = device.displayName?.ToLower() ?? "";
        
        if (device is Keyboard) return "Keyboard";
        if (device is Gamepad)
        {
            if (displayName.Contains("xbox") || displayName.Contains("microsoft"))
                return "Xbox Controller";
            if (displayName.Contains("dualsense") || (displayName.Contains("playstation") && displayName.Contains("5")))
                return "PS5 Controller";
            return "Generic Controller";
        }
        return "Unknown";
    }

    private void DetectInputDevices()
    {
        connectedPlayers.Clear();
        foreach (var device in InputSystem.devices)
        {
            string deviceType = GetDeviceType(device);
            if (deviceType != "Unknown")
            {
                AssignDeviceToPlayer(device, deviceType);
            }
        }
        LogInputSystemDevices();
    }
    
    private void AssignDeviceToPlayer(InputDevice device, string deviceType)
    {
        if (connectedPlayers.Exists(p => p.Device == device)) return;
        if (connectedPlayers.Count >= 2)
        {
            Debug.Log($"[Player] Maximum players (2) reached. Ignoring: {deviceType}");
            return;
        }
        
        int playerIndex = connectedPlayers.Count;
        Debug.Log($"[Player] Assigning {deviceType} to Player {playerIndex + 1}");
        connectedPlayers.Add(new PlayerInfo(device, deviceType, playerIndex));
        UpdateControllerVisuals();
    }
    
    private void RemoveDevice(InputDevice device)
    {
        PlayerInfo player = connectedPlayers.Find(p => p.Device == device);
        if (player != null)
        {
            if (player.IsReady)
            {
                string position = GetPositionName(player.CurrentPosition);
                if (position == "Left")
                {
                    leftSideReadyState = false;
                    if (leftReady) leftReady.SetActive(false);
                }
                else if (position == "Right")
                {
                    rightSideReadyState = false;
                    if (rightReady) rightReady.SetActive(false);
                }
            }
            
            connectedPlayers.Remove(player);
            for (int i = 0; i < connectedPlayers.Count; i++)
                connectedPlayers[i].PlayerIndex = i;
            
            UpdateControllerVisuals();
        }
    }
    
    private void UpdateControllerVisuals()
    {
        if (player1Side) player1Side.SetActive(false);
        if (player2Side) player2Side.SetActive(false);
        
        foreach (var player in connectedPlayers)
        {
            GameObject playerSide = player.PlayerIndex == 0 ? player1Side : player2Side;
            Transform middleTarget = player.PlayerIndex == 0 ? player1MTarget : player2MTarget;
            
            if (playerSide != null && middleTarget != null)
            {
                playerSide.SetActive(true);
                playerSide.transform.position = middleTarget.position;
                player.CurrentPosition = middleTarget;
                ShowCorrectControllerImage(playerSide, player.DeviceType);
            }
        }
    }
    
    private void ShowCorrectControllerImage(GameObject playerSide, string deviceType)
    {
        if (playerSide.transform.childCount < 3)
        {
            Debug.LogError("[UI] Player side object missing controller images");
            return;
        }
        
        for (int i = 0; i < 3; i++)
            playerSide.transform.GetChild(i).gameObject.SetActive(false);
        
        int childIndex = deviceType switch
        {
            "PS5 Controller" => 0,
            "Xbox Controller" => 1,
            _ => 2 // Keyboard
        };
        playerSide.transform.GetChild(childIndex).gameObject.SetActive(true);
    }
    
    private void ProcessPlayerInput()
    {
        var playersToProcess = new List<PlayerInfo>(connectedPlayers);
        
        foreach (var player in playersToProcess)
        {
            if (player.Device is Gamepad gamepad)
                ProcessGamepadInput(player, gamepad);
            else if (player.Device is Keyboard keyboard)
                ProcessKeyboardInput(player, keyboard);
        }
    }
    
    private void ProcessGamepadInput(PlayerInfo player, Gamepad gamepad)
    {
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        
        if (leftStick.x < -analogThreshold && !analogLeftState[player.PlayerIndex])
        {
            analogLeftState[player.PlayerIndex] = true;
            if (IsAtMiddlePosition(player.CurrentPosition))
                MovePlayerLeft(player);
            else if (IsAtRightPosition(player.CurrentPosition))
                MovePlayerMiddle(player);
        }
        else if (leftStick.x > analogThreshold && !analogRightState[player.PlayerIndex])
        {
            analogRightState[player.PlayerIndex] = true;
            if (IsAtMiddlePosition(player.CurrentPosition))
                MovePlayerRight(player);
            else if (IsAtLeftPosition(player.CurrentPosition))
                MovePlayerMiddle(player);
        }
        else if (Mathf.Abs(leftStick.x) < 0.2f)
        {
            analogLeftState[player.PlayerIndex] = false;
            analogRightState[player.PlayerIndex] = false;
        }
        
        if (gamepad.aButton.wasPressedThisFrame || gamepad.crossButton.wasPressedThisFrame)
            TogglePlayerReady(player);
    }
    
    private void ProcessKeyboardInput(PlayerInfo player, Keyboard keyboard)
    {
        // Check for second keyboard player addition
        if (keyboard.digit1Key.wasPressedThisFrame && connectedPlayers.Count < 2)
        {
            var existingKeyboardPlayer = connectedPlayers.Find(p => p.DeviceType == "Keyboard");
            if (existingKeyboardPlayer != null && existingKeyboardPlayer.Device == keyboard)
            {
                Debug.Log("[Player] Adding second keyboard player");
                connectedPlayers.Add(new PlayerInfo(keyboard, "Keyboard", connectedPlayers.Count));
                UpdateControllerVisuals();
                return;
            }
        }

        // Handle input scheme toggle
        if (keyboard.spaceKey.wasPressedThisFrame && player.PlayerIndex == 0)
        {
            isInputMapSwapped = !isInputMapSwapped;
            UpdateControlSchemeTexts();
            return;
        }

        // Process movement
        if (player.PlayerIndex == 0)
        {
            if (keyboard.aKey.wasPressedThisFrame && IsAtMiddlePosition(player.CurrentPosition))
                MovePlayerLeft(player);
            else if (keyboard.dKey.wasPressedThisFrame && IsAtMiddlePosition(player.CurrentPosition))
                MovePlayerRight(player);
            else if (keyboard.aKey.wasPressedThisFrame && IsAtRightPosition(player.CurrentPosition))
                MovePlayerMiddle(player);
            else if (keyboard.dKey.wasPressedThisFrame && IsAtLeftPosition(player.CurrentPosition))
                MovePlayerMiddle(player);
        }
        else // Player 2
        {
            if (keyboard.leftArrowKey.wasPressedThisFrame && IsAtMiddlePosition(player.CurrentPosition))
                MovePlayerLeft(player);
            else if (keyboard.rightArrowKey.wasPressedThisFrame && IsAtMiddlePosition(player.CurrentPosition))
                MovePlayerRight(player);
            else if (keyboard.leftArrowKey.wasPressedThisFrame && IsAtRightPosition(player.CurrentPosition))
                MovePlayerMiddle(player);
            else if (keyboard.rightArrowKey.wasPressedThisFrame && IsAtLeftPosition(player.CurrentPosition))
                MovePlayerMiddle(player);
        }
        
        if (keyboard.enterKey.wasPressedThisFrame)
            TogglePlayerReady(player);
    }

    private void UpdateControlSchemeTexts()
    {
        if (player1ControlsText != null && player2ControlsText != null)
        {
            player1ControlsText.text = isInputMapSwapped ? "WASD" : "ARROWS";
            player2ControlsText.text = isInputMapSwapped ? "ARROWS" : "WASD";
        }
    }

    private void MovePlayerMiddle(PlayerInfo player)
    {
        GameObject playerSide = player.PlayerIndex == 0 ? player1Side : player2Side;
        Transform middleTarget = player.PlayerIndex == 0 ? player1MTarget : player2MTarget;
        
        if (playerSide != null && middleTarget != null && player.CurrentPosition != middleTarget)
        {
            playerSide.transform.DOMove(middleTarget.position, moveAnimationDuration);
            player.CurrentPosition = middleTarget;
        }
    }
    
    private void MovePlayerLeft(PlayerInfo player)
    {
        GameObject playerSide = player.PlayerIndex == 0 ? player1Side : player2Side;
        Transform leftTarget = player.PlayerIndex == 0 ? player1LTarget : player2LTarget;
        
        if (playerSide != null && leftTarget != null && player.CurrentPosition != leftTarget)
        {
            if (IsSideOccupiedByReadyPlayer("Left"))
            {
                Debug.Log($"[Movement] Player {player.PlayerIndex + 1} cannot move left - side occupied");
                return;
            }
            
            playerSide.transform.DOMove(leftTarget.position, moveAnimationDuration);
            player.CurrentPosition = leftTarget;
        }
    }
    
    private void MovePlayerRight(PlayerInfo player)
    {
        GameObject playerSide = player.PlayerIndex == 0 ? player1Side : player2Side;
        Transform rightTarget = player.PlayerIndex == 0 ? player1RTarget : player2RTarget;
        
        if (playerSide != null && rightTarget != null && player.CurrentPosition != rightTarget)
        {
            if (IsSideOccupiedByReadyPlayer("Right"))
            {
                Debug.Log($"[Movement] Player {player.PlayerIndex + 1} cannot move right - side occupied");
                return;
            }
            
            playerSide.transform.DOMove(rightTarget.position, moveAnimationDuration);
            player.CurrentPosition = rightTarget;
        }
    }
    
    private void TogglePlayerReady(PlayerInfo player)
    {
        if (!IsAtSidePosition(player.CurrentPosition))
        {
            Debug.Log($"[Ready] Player {player.PlayerIndex + 1} must be at side position");
            return;
        }
        
        string position = GetPositionName(player.CurrentPosition);
        Debug.Log($"[Ready] Player {player.PlayerIndex + 1} ({player.DeviceType}) at {position} toggling ready");
        
        if (position == "Left")
        {
            leftSideReadyState = !leftSideReadyState;
            player.IsReady = leftSideReadyState;
            if (leftReady) leftReady.SetActive(leftSideReadyState);
        }
        else if (position == "Right")
        {
            rightSideReadyState = !rightSideReadyState;
            player.IsReady = rightSideReadyState;
            if (rightReady) rightReady.SetActive(rightSideReadyState);
        }
        
        bool bothSidesReady = leftSideReadyState && rightSideReadyState;
        TeamFlag controlledTeamFlag = leftSideReadyState ? TeamFlag.Home : TeamFlag.Away;
    
        if (!readyPlayers.Contains(player))
        {
            readyPlayers.Add(player);
            Debug.Log($"[Ready] Added Player {player.PlayerIndex + 1} to ready list");
        }
        else
        {
            readyPlayers.Remove(player);
            Debug.Log($"[Ready] Removed Player {player.PlayerIndex + 1} from ready list");
        }

        if (readyPlayers.Count == connectedPlayers.Count)
        {
            Debug.Log("[Game] All players ready, starting game...");
            Dictionary<TeamFlag, List<int>> teamPlayerIndices = new Dictionary<TeamFlag, List<int>>();
            teamPlayerIndices[TeamFlag.Home] = new List<int>();
            teamPlayerIndices[TeamFlag.Away] = new List<int>();

            foreach (var readyPlayer in readyPlayers)
            {
                string pos = GetPositionName(readyPlayer.CurrentPosition);
                TeamFlag teamFlag = pos == "Left" ? TeamFlag.Home : TeamFlag.Away;
                teamPlayerIndices[teamFlag].Add(readyPlayer.PlayerIndex);
                Debug.Log($"[Team] Player {readyPlayer.PlayerIndex + 1} assigned to {teamFlag}");
            }
            //GameStartConfig config = new ();
            //config.gameMode = GameMode.LocalPVP;
            //
            //config.teamPlayerIndices = teamPlayerIndices;
            //GameManager.Instance.StartGame(config);
            //gameObject.SetActive(false);
            FootballSim.GameStartConfig config = new();

            config.HomePlayerIndex = teamPlayerIndices[TeamFlag.Home].Count > 0 ? teamPlayerIndices[TeamFlag.Home][0] : -1;
            config.AwayPlayerIndex = teamPlayerIndices[TeamFlag.Away].Count > 0 ? teamPlayerIndices[TeamFlag.Away][0] : -1;
            config.GameMode = FootballSim.GameMode.PVP_LOCAL;
            FootballSim.GameManager.Instance.InitiateGame(config);
            

        }
    }

    // Helper methods for position checks
    private bool IsAtMiddlePosition(Transform position) => 
        position == player1MTarget || position == player2MTarget;
    private bool IsAtLeftPosition(Transform position) => 
        position == player1LTarget || position == player2LTarget;
    private bool IsAtRightPosition(Transform position) => 
        position == player1RTarget || position == player2RTarget;
    private bool IsAtSidePosition(Transform position) => 
        IsAtLeftPosition(position) || IsAtRightPosition(position);
    
    private string GetPositionName(Transform position)
    {
        if (IsAtLeftPosition(position)) return "Left";
        if (IsAtRightPosition(position)) return "Right";
        return "Middle";
    }
    
    private bool IsSideOccupiedByReadyPlayer(string side)
    {
        foreach (var p in connectedPlayers)
        {
            if (p.IsReady && GetPositionName(p.CurrentPosition) == side)
                return true;
        }
        return false;
    }
    
    private void LogInputSystemDevices()
    {
        Debug.Log($"[Devices] Total detected: {InputSystem.devices.Count}");
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Available input devices:");
        foreach (var device in InputSystem.devices)
            sb.AppendLine($"- {GetDeviceType(device)}: {device.displayName}");
        Debug.Log(sb.ToString());
    }

    public List<string> GetPlayerControllerTypes()
    {
        List<string> controllerTypes = new List<string>(2);
        foreach (var player in readyPlayers)
        {
            controllerTypes.Add(player.DeviceType);
            Debug.Log($"[Controller] Player {player.PlayerIndex + 1} using {player.DeviceType}");
        }
        return controllerTypes;
    }

    public bool IsInputMapSwapped() => isInputMapSwapped;
}
