using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;

public class PlayerInfo
{
    public InputDevice Device;
    public string DeviceType;
    public int PlayerIndex;
    public bool IsReady;
    public Transform CurrentPosition;
    public TeamFlag Team;

    public PlayerInfo(InputDevice device, string deviceType, int playerIndex)
    {
        Device = device;
        DeviceType = deviceType;
        PlayerIndex = playerIndex;
        IsReady = false;
        Team = TeamFlag.None;
    }
}

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private InputActionAsset inputActionsAsset;
    private InputActionMap[] inputMaps;
    [SerializeField] Dictionary<TeamFlag, InputActionAsset> teamActionAssets = new();
    public List<PlayerInfo> ConnectedPlayers = new List<PlayerInfo>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        inputMaps = inputActionsAsset.actionMaps.ToArray();
    }
    
    private InputDevice GetDeviceByTeam(TeamFlag team)
    {
        foreach (var player in ConnectedPlayers)
        {
            if (player.Team == team)
            {
                return player.Device;
            }
        }
        return null;
    }
    
    public InputAction GetAction(TeamFlag team, string actionName)
    {
        InputDevice device = GetDeviceByTeam(team);
        if (device == null)
        {
            Debug.LogError($"No device found for team {team}");
            return null;
        }

        if (device is Keyboard)
        {
            foreach (var map in inputMaps)
            {
                if (map.name == "Keyboard+Mouse")
                {
                    return map.FindAction(actionName);
                }
            }
            print("keyboard");
        }
        
        else if (device is Gamepad)
        {
            foreach (var map in inputMaps)
            {
                if (map.name == "Controller")
                {
                    return map.FindAction(actionName);
                }
            }
            print("gamepad");
        }
        Debug.LogError($"Unsupported device type: {device.GetType()}");
        return null;
    }
}