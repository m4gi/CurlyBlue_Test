using Fusion;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public string GameModeIdentifier;
    public NetworkRunner RunnerPrefab;
    public int MaxPlayerCount = 8;

    public bool ForceSinglePlayer;

    [Header("UI Setup")]
    public CanvasGroup PanelGroup;
    public TMP_InputField NicknameText;
    public TextMeshProUGUI StatusText;
    public GameObject StartGroup;
    public GameObject DisconnectGroup;

    private NetworkRunner _runnerInstance;
    private static string _shutdownStatus;

    public async void StartGame()
    {
        await Disconnect();

        PlayerPrefs.SetString("PlayerName", NicknameText.text);

        _runnerInstance = Instantiate(RunnerPrefab);

        // Add listener for shutdowns so we can handle unexpected shutdowns
        var events = _runnerInstance.GetComponent<NetworkEvents>();
        events.OnShutdown.AddListener(OnShutdown);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex));

        var startArguments = new StartGameArgs()
        {
            GameMode = Application.isEditor && ForceSinglePlayer ? GameMode.Single : GameMode.AutoHostOrClient,
            SessionName = GameModeIdentifier,
            PlayerCount = MaxPlayerCount,
            SessionProperties = new Dictionary<string, SessionProperty> { ["GameMode"] = GameModeIdentifier, ["Nickname"] = NicknameText.text },
            Scene = sceneInfo,
        };

        StatusText.text = startArguments.GameMode == GameMode.Single ? "Starting single-player..." : "Connecting...";

        var startTask = _runnerInstance.StartGame(startArguments);
        await startTask;

        if (startTask.Result.Ok)
        {
            StatusText.text = "";
            PanelGroup.gameObject.SetActive(false);
        }
        else
        {
            StatusText.text = $"Connection Failed: {startTask.Result.ShutdownReason}";
        }
    }

    public async void DisconnectClicked()
    {
        await Disconnect();
    }

    public async void BackToMenu()
    {
        await Disconnect();

        SceneManager.LoadScene(0);
    }

    public void TogglePanelVisibility()
    {
        if (PanelGroup.gameObject.activeSelf && _runnerInstance == null)
            return;

        PanelGroup.gameObject.SetActive(!PanelGroup.gameObject.activeSelf);
    }

    private void OnEnable()
    {
        var nickname = PlayerPrefs.GetString("PlayerName");
        if (string.IsNullOrEmpty(nickname))
        {
            nickname = "Player" + Random.Range(10000, 100000);
        }

        NicknameText.text = nickname;

        StatusText.text = _shutdownStatus != null ? _shutdownStatus : string.Empty;
        _shutdownStatus = null;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePanelVisibility();
        }

        if (PanelGroup.gameObject.activeSelf)
        {
            StartGroup.SetActive(_runnerInstance == null);
            DisconnectGroup.SetActive(_runnerInstance != null);
            NicknameText.interactable = _runnerInstance == null;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public async Task Disconnect()
    {
        if (_runnerInstance == null)
            return;

        StatusText.text = "Disconnecting...";
        PanelGroup.interactable = false;

        var events = _runnerInstance.GetComponent<NetworkEvents>();
        events.OnShutdown.RemoveListener(OnShutdown);

        await _runnerInstance.Shutdown();
        _runnerInstance = null;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnShutdown(NetworkRunner runner, ShutdownReason reason)
    {
        _shutdownStatus = $"Shutdown: {reason}";
        Debug.LogWarning(_shutdownStatus);

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
