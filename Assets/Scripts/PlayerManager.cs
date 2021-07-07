using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;


public class PlayerManager : MonoBehaviour
{
    #region vars
    public GameState CurrentGameState
    {
        get { return m_CurrentGameState; }
        set
        {
            if (m_CurrentGameState == value) return;
            m_CurrentGameState = value;
            SetState(value);
        }
    }
    private GameState m_CurrentGameState = GameState.None;

    public PlayerInput playerInput;
    [SerializeField] private PlayerController playerPrefab;
    [SerializeField] private GameObject engagementView, disconnectView;
    
    private PlayerController m_CurrentPlayer;
    private GameState m_GameStateOnDisconnect;
    private InputDevice m_CurrentDevice;
    #endregion

    #region Unity Messages
    private void Start()
    {
        disconnectView.SetActive(false);
        engagementView.SetActive(true);

        // Subscribe to events
        InputSystem.onDeviceChange += OnDeviceChange;
        playerInput.actions.FindAction("Engage").performed += OnEngagement;
        
        // Start player engagement
        CurrentGameState = GameState.Engagement;
    }
    #endregion


    #region Input Callbacks
    private void OnEngagement(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        Debug.Log("Engaged!");
        playerInput.actions.FindAction("Engage").performed -= OnEngagement;

        // Set device
        m_CurrentDevice = ctx.control.device;

        // Unpair other devices
        PairCurrentDevice();

        // Spawn player        
        Debug.Log("Spawning player");
        SpawnPlayer();

        // Set game state
        CurrentGameState = GameState.Movement;

        engagementView.SetActive(false);
        return;        
    }

    void OnJoinPressed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        Debug.Log("Joined!");

        // Set device
        m_CurrentDevice = ctx.control.device;

        // Unpair existing devices
        PairCurrentDevice();

        // Set game state
        Debug.Log("Setting gamestate back to pre-disconnect state");
        CurrentGameState = m_GameStateOnDisconnect;
        disconnectView.SetActive(false);

        // Unsubscribe to join action
        playerInput.actions.FindAction("Engage").performed -= OnJoinPressed;
    }

    void OnDeviceChange(InputDevice device, InputDeviceChange deviceChange)
    {
        switch (deviceChange)
        {
                
            case InputDeviceChange.Added:
                OnDeviceAdded(device);
                break;
            case InputDeviceChange.Removed:
                break;
            case InputDeviceChange.Disconnected:
                OnDeviceDisconnected(device);
                break;
            case InputDeviceChange.Reconnected:
                OnDeviceReconnected(device);                
                break;
            case InputDeviceChange.Enabled:
                break;
            case InputDeviceChange.Disabled:
                break;
            case InputDeviceChange.UsageChanged:
                break;
            case InputDeviceChange.ConfigurationChanged:
                break;
            case InputDeviceChange.Destroyed:
                break;
        }
    }

    private void OnDeviceDisconnected(InputDevice device)
    {
        // If we're already disconnected - ignore event
        if (CurrentGameState == GameState.Disconnected || CurrentGameState == GameState.Engagement)
            return;

        // Check if this lost device is the active device
        if (device == m_CurrentDevice)
        {
            Debug.LogWarning("Device Lost!");
            m_GameStateOnDisconnect = CurrentGameState;
            disconnectView.SetActive(true);

            // Wait for restoration or engagement - add available devices to listen for input
            foreach (var d in InputUser.GetUnpairedInputDevices())
            {
                InputUser.PerformPairingWithDevice(d, playerInput.user, InputUserPairingOptions.None);
            }

            // Subscribe to join event
            CurrentGameState = GameState.Disconnected;
            playerInput.actions.FindAction("Engage").performed += OnJoinPressed;
        }
    }

    private void OnDeviceAdded(InputDevice device)
    {
        // If we're not in engagement or reconect state - unpair device
        if (CurrentGameState == GameState.Engagement || CurrentGameState == GameState.Disconnected)
            return;

        playerInput.user.UnpairDevice(device);
    }

    private void OnDeviceReconnected(InputDevice device)
    {
        // Ignore if we're on the engagement screen
        if (CurrentGameState == GameState.Engagement)
            return;

        // Are we using a different device?
        if (device == m_CurrentDevice)
        {
            // This device has been reconnected
            Debug.LogWarning("Device restored!");
            
            // Clear other devices
            PairCurrentDevice();

            disconnectView.SetActive(false);
            CurrentGameState = m_GameStateOnDisconnect;
        }
        else
        {
            // Using a new device, unpair this one
            Debug.LogWarning("Another gamepad is associated to the user - ignoring this device");
            playerInput.user.UnpairDevice(device);
        }

        disconnectView.SetActive(false);
    }
    #endregion


    private void PairCurrentDevice()
    {
        if(m_CurrentDevice == null)
        {
            Debug.LogError("Unable to pair device - device does not exist!");
            return;
        }

        InputUser.PerformPairingWithDevice(m_CurrentDevice,
            playerInput.user,
            InputUserPairingOptions.UnpairCurrentDevicesFromUser);
    }

    private void SpawnPlayer()
    {
        m_CurrentPlayer = Instantiate(playerPrefab);
        playerInput.actions.FindAction("Move").performed    += m_CurrentPlayer.OnMove;
        playerInput.actions.FindAction("Move").canceled     += m_CurrentPlayer.OnMove;
    }


    private void SetState(GameState state)
    {
        switch (state)
        {
            case GameState.Engagement:                              
            case GameState.Disconnected:
                playerInput.SwitchCurrentActionMap("Engagement");                
                break;
            case GameState.Movement:
                playerInput.SwitchCurrentActionMap("Movement");
                break;
            case GameState.UI:
                playerInput.SwitchCurrentActionMap("UI");
                break;
            case GameState.None:
            default:
                playerInput.currentActionMap.Disable();
                break;
        }
        Debug.LogFormat("Switching game state: {0}", state.ToString());
    }
}


    public enum GameState
{
    None,
    Engagement,
    Disconnected,
    Movement,
    UI
}
