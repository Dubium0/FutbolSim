using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Text;
using DG.Tweening; // Make sure DOTween is imported in your project

public class SelectSideManager : MonoBehaviour
{
    [SerializeField] private GameObject player1Side;
    [SerializeField] private GameObject player2Side;
    [SerializeField] private GameObject player3Side;
    
    [SerializeField] private Transform player1MTarget;
    [SerializeField] private Transform player2MTarget;
    [SerializeField] private Transform player3MTarget;
    
    [SerializeField] private Transform player1RTarget;
    [SerializeField] private Transform player2RTarget;
    [SerializeField] private Transform player3RTarget;
    
    [SerializeField] private Transform player1LTarget;
    [SerializeField] private Transform player2LTarget;
    [SerializeField] private Transform player3LTarget;
    
    [SerializeField] private GameObject player1Ready;
    [SerializeField] private GameObject player2Ready;
    
    private bool player1ReadyState = false;
    private bool player2ReadyState = false;
    
    // Class to track players and their devices
    private class PlayerInfo
    {
        public InputDevice Device;
        public string DeviceType;
        public int PlayerIndex;
        public bool IsReady;
        public GameObject ReadyIndicator;
        public Transform CurrentPosition;
        
        public PlayerInfo(InputDevice device, string deviceType, int playerIndex, GameObject readyIndicator)
        {
            Device = device;
            DeviceType = deviceType;
            PlayerIndex = playerIndex;
            IsReady = false;
            ReadyIndicator = readyIndicator;
        }
    }
    
    private List<PlayerInfo> connectedPlayers = new List<PlayerInfo>();
    private float moveAnimationDuration = 0.5f;
    private float analogThreshold = 0.5f;
    private bool[] analogLeftState = new bool[3];
    private bool[] analogRightState = new bool[3];
    
    private void OnEnable()
    {
        // Subscribe to device change events
        InputSystem.onDeviceChange += OnInputDeviceChange;
    }

    private void OnDisable()
    {
        // Unsubscribe when disabled
        InputSystem.onDeviceChange -= OnInputDeviceChange;
    }
    
    void Start()
    {
        // Hide all player sides initially
        if (player1Side) player1Side.SetActive(false);
        if (player2Side) player2Side.SetActive(false);
        if (player3Side) player3Side.SetActive(false);
        
        // Hide ready indicators
        if (player1Ready) player1Ready.SetActive(false);
        if (player2Ready) player2Ready.SetActive(false);
        
        // Initialize analog states
        for (int i = 0; i < 3; i++)
        {
            analogLeftState[i] = false;
            analogRightState[i] = false;
        }
        
        // Detect initial devices
        DetectInputDevices();
    }

    void Update()
    {
        // Process input for all connected players
        ProcessPlayerInput();
    }

    private void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Added)
        {
            // A new device was connected
            string deviceType = GetDeviceType(device);
            Debug.Log($"Device connected: {deviceType} (\"{device.displayName}\")");
            
            // Assign the device to a player
            AssignDeviceToPlayer(device, deviceType);
        }
        else if (change == InputDeviceChange.Removed)
        {
            // A device was disconnected
            Debug.Log($"Device disconnected: (\"{device.displayName}\")");
            
            // Remove the device from players
            RemoveDevice(device);
        }
    }

    private string GetDeviceType(InputDevice device)
    {
        string deviceType = "Unknown";
        string displayName = device.displayName?.ToLower() ?? "";
        
        if (device is Keyboard)
        {
            deviceType = "Keyboard";
        }
        else if (device is Gamepad)
        {
            // Default to generic controller
            deviceType = "Generic Controller";
            
            // Detect Xbox controllers
            if (displayName.Contains("xbox") || displayName.Contains("microsoft"))
            {
                deviceType = "Xbox Controller";
            }
            // Detect PS5 controllers
            else if (displayName.Contains("dualsense") || 
                    (displayName.Contains("playstation") && displayName.Contains("5")))
            {
                deviceType = "PS5 Controller";
            }
        }
        
        return deviceType;
    }

    private void DetectInputDevices()
    {
        // Clear existing players
        connectedPlayers.Clear();
        
        // Detect and assign all current devices
        foreach (var device in InputSystem.devices)
        {
            string deviceType = GetDeviceType(device);
            if (deviceType == "Keyboard" || deviceType == "Xbox Controller" || 
                deviceType == "PS5 Controller" || deviceType == "Generic Controller")
            {
                AssignDeviceToPlayer(device, deviceType);
            }
        }
        
        LogInputSystemDevices();
    }
    
    private void AssignDeviceToPlayer(InputDevice device, string deviceType)
    {
        // Don't assign if already assigned
        if (connectedPlayers.Exists(p => p.Device == device))
            return;
            
        // Check if we already have 3 players
        if (connectedPlayers.Count >= 3)
        {
            Debug.Log("Maximum number of players reached. Ignoring new device.");
            return;
        }
        
        // Assign to next available player slot
        int playerIndex = connectedPlayers.Count;
        GameObject readyIndicator = null;
        
        if (playerIndex == 0) readyIndicator = player1Ready;
        else if (playerIndex == 1) readyIndicator = player2Ready;
        
        connectedPlayers.Add(new PlayerInfo(device, deviceType, playerIndex, readyIndicator));
        
        // Show the appropriate controller image
        UpdateControllerVisuals();
    }
    
    private void RemoveDevice(InputDevice device)
    {
        PlayerInfo player = connectedPlayers.Find(p => p.Device == device);
        if (player != null)
        {
            int removedIndex = player.PlayerIndex;
            connectedPlayers.Remove(player);
            
            // Reset ready state if it was one of the first two players
            if (removedIndex == 0) player1ReadyState = false;
            else if (removedIndex == 1) player2ReadyState = false;
            
            // Reassign player indices
            for (int i = 0; i < connectedPlayers.Count; i++)
            {
                connectedPlayers[i].PlayerIndex = i;
            }
            
            // Update visuals
            UpdateControllerVisuals();
        }
    }
    
    private void UpdateControllerVisuals()
    {
        // Hide all player sides initially
        if (player1Side) player1Side.SetActive(false);
        if (player2Side) player2Side.SetActive(false);
        if (player3Side) player3Side.SetActive(false);
        
        // Loop through connected players and show appropriate visuals
        foreach (var player in connectedPlayers)
        {
            GameObject playerSide = null;
            Transform middleTarget = null;
            
            switch (player.PlayerIndex)
            {
                case 0:
                    playerSide = player1Side;
                    middleTarget = player1MTarget;
                    break;
                case 1:
                    playerSide = player2Side;
                    middleTarget = player2MTarget;
                    break;
                case 2:
                    playerSide = player3Side;
                    middleTarget = player3MTarget;
                    break;
            }
            
            if (playerSide != null && middleTarget != null)
            {
                playerSide.SetActive(true);
                
                // Set initial position
                playerSide.transform.position = middleTarget.position;
                player.CurrentPosition = middleTarget;
                
                // Show the correct controller image (child)
                ShowCorrectControllerImage(playerSide, player.DeviceType);
            }
        }
    }
    
    private void ShowCorrectControllerImage(GameObject playerSide, string deviceType)
    {
        // Make sure the playerSide has at least 3 children
        if (playerSide.transform.childCount < 3)
        {
            Debug.LogError("Player side object does not have enough children for controller images");
            return;
        }
        
        // Disable all child images first
        for (int i = 0; i < 3; i++)
        {
            playerSide.transform.GetChild(i).gameObject.SetActive(false);
        }
        
        // Enable the correct image based on device type
        int childIndex;
        if (deviceType == "PS5 Controller")
            childIndex = 0;  // 1st child - PS controller image
        else if (deviceType == "Xbox Controller")
            childIndex = 1;  // 2nd child - Xbox controller image
        else
            childIndex = 2;  // 3rd child - Keyboard image
            
        playerSide.transform.GetChild(childIndex).gameObject.SetActive(true);
    }
    
    private void ProcessPlayerInput()
    {
        foreach (var player in connectedPlayers)
        {
            if (player.Device is Gamepad gamepad)
            {
                // Process gamepad input
                ProcessGamepadInput(player, gamepad);
            }
            else if (player.Device is Keyboard keyboard)
            {
                // Process keyboard input
                ProcessKeyboardInput(player, keyboard);
            }
        }
    }
    
    private void ProcessGamepadInput(PlayerInfo player, Gamepad gamepad)
    {
        // Get left stick value
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        
        // Check for left/right movement
        if (leftStick.x < -analogThreshold && !analogLeftState[player.PlayerIndex])
        {
            analogLeftState[player.PlayerIndex] = true;
            MovePlayerLeft(player);
        }
        else if (leftStick.x > analogThreshold && !analogRightState[player.PlayerIndex])
        {
            analogRightState[player.PlayerIndex] = true;
            MovePlayerRight(player);
        }
        else if (Mathf.Abs(leftStick.x) < 0.2f)
        {
            // Reset analog state when stick is back to neutral
            analogLeftState[player.PlayerIndex] = false;
            analogRightState[player.PlayerIndex] = false;
        }
        
        // Check for ready button (using South button / A on Xbox / X on PlayStation)
        if (gamepad.aButton.wasPressedThisFrame || gamepad.crossButton.wasPressedThisFrame)
        {
            TogglePlayerReady(player);
        }
    }
    
    private void ProcessKeyboardInput(PlayerInfo player, Keyboard keyboard)
    {
        // Left/right movement with arrow keys
        if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            MovePlayerLeft(player);
        }
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            MovePlayerRight(player);
        }
        
        // Ready with spacebar or enter
        if (keyboard.spaceKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame)
        {
            TogglePlayerReady(player);
        }
    }
    
    private void MovePlayerLeft(PlayerInfo player)
    {
        GameObject playerSide = null;
        Transform leftTarget = null;
        
        switch (player.PlayerIndex)
        {
            case 0:
                playerSide = player1Side;
                leftTarget = player1LTarget;
                break;
            case 1:
                playerSide = player2Side;
                leftTarget = player2LTarget;
                break;
            case 2:
                playerSide = player3Side;
                leftTarget = player3LTarget;
                break;
        }
        
        if (playerSide != null && leftTarget != null && player.CurrentPosition != leftTarget)
        {
            // Move to left position
            playerSide.transform.DOMove(leftTarget.position, moveAnimationDuration);
            player.CurrentPosition = leftTarget;
        }
    }
    
    private void MovePlayerRight(PlayerInfo player)
    {
        GameObject playerSide = null;
        Transform rightTarget = null;
        
        switch (player.PlayerIndex)
        {
            case 0:
                playerSide = player1Side;
                rightTarget = player1RTarget;
                break;
            case 1:
                playerSide = player2Side;
                rightTarget = player2RTarget;
                break;
            case 2:
                playerSide = player3Side;
                rightTarget = player3RTarget;
                break;
        }
        
        if (playerSide != null && rightTarget != null && player.CurrentPosition != rightTarget)
        {
            // Move to right position
            playerSide.transform.DOMove(rightTarget.position, moveAnimationDuration);
            player.CurrentPosition = rightTarget;
        }
    }
    
    private void TogglePlayerReady(PlayerInfo player)
    {
        // Toggle ready state
        player.IsReady = !player.IsReady;
        
        // Update ready indicator and ready state variables
        if (player.PlayerIndex == 0)
        {
            player1ReadyState = player.IsReady;
            if (player1Ready) player1Ready.SetActive(player.IsReady);
        }
        else if (player.PlayerIndex == 1)
        {
            player2ReadyState = player.IsReady;
            if (player2Ready) player2Ready.SetActive(player.IsReady);
        }
        
        // Log player ready state
        Debug.Log($"Player {player.PlayerIndex + 1} ready state: {player.IsReady}");
    }
    
    private void LogInputSystemDevices()
    {
        Debug.Log($"Total input devices detected: {InputSystem.devices.Count}");
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Available input devices:");
        
        foreach (var device in InputSystem.devices)
        {
            string deviceType = GetDeviceType(device);
            sb.AppendLine($"- {deviceType}: {device.displayName}");
        }
        
        Debug.Log(sb.ToString());
    }
}
