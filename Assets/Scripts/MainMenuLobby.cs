using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuLobby : MonoBehaviourPunCallbacks
{
    private const int MainMenuSceneIndex = 0;
    private const int LevelSceneIndex = 1;
    private const byte MaxPlayers = 2;
    private const string HostNameProperty = "HostName";
    private const string RoomButtonName = "Room Button";
    private const string RoomButtonCompactName = "RoomButton";

    [SerializeField] private GameObject startGroup;
    [SerializeField] private GameObject lobbyGroup;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private RectTransform roomContent;
    [SerializeField] private Button roomButtonTemplate;

    private readonly Dictionary<string, RoomInfo> cachedRooms = new Dictionary<string, RoomInfo>();
    private readonly List<GameObject> spawnedRoomButtons = new List<GameObject>();
    private bool loadRequested;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapCurrentScene()
    {
        SceneManager.sceneLoaded += BootstrapScene;
        BootstrapScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void BootstrapScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != MainMenuSceneIndex && scene.name != "Main Menu")
        {
            return;
        }

        if (FindObjectOfType<MainMenuLobby>() != null)
        {
            return;
        }

        new GameObject("Main Menu Lobby").AddComponent<MainMenuLobby>();
    }

    private void Awake()
    {
        AutoAssignSceneReferences();
        WireButtonEvents();

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        ShowStartGroup();

        if (PhotonNetwork.IsConnected)
        {
            JoinLobbyIfReady();
            return;
        }

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        JoinLobbyIfReady();
    }

    public override void OnJoinedLobby()
    {
        RefreshRoomButtons();
    }

    public override void OnLeftLobby()
    {
        JoinLobbyIfReady();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room == null)
            {
                continue;
            }

            if (room.RemovedFromList || room.PlayerCount <= 0 || !room.IsVisible)
            {
                cachedRooms.Remove(room.Name);
                continue;
            }

            cachedRooms[room.Name] = room;
        }

        RefreshRoomButtons();
    }

    public override void OnCreatedRoom()
    {
        PhotonNetwork.CurrentRoom.IsOpen = PhotonNetwork.CurrentRoom.PlayerCount < MaxPlayers;
        PhotonNetwork.CurrentRoom.IsVisible = true;
    }

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.CurrentRoom.IsOpen = PhotonNetwork.CurrentRoom.PlayerCount < MaxPlayers;
            PhotonNetwork.CurrentRoom.IsVisible = true;
        }

        LoadLevelOne();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        cachedRooms.Clear();
        RefreshRoomButtons();
        JoinLobbyIfReady();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        JoinLobbyIfReady();
    }

    public void ShowStartGroup()
    {
        SetGroupActive(startGroup, true);
        SetGroupActive(lobbyGroup, false);
    }

    public void ShowLobbyGroup()
    {
        SetGroupActive(startGroup, false);
        SetGroupActive(lobbyGroup, true);
        JoinLobbyIfReady();
        RefreshRoomButtons();
    }

    public void CreateRoom()
    {
        string playerName = GetPlayerName();
        PhotonNetwork.NickName = playerName;

        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.ConnectUsingSettings();
            return;
        }

        string roomName = $"{playerName}'s Room {Random.Range(1000, 9999)}";
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = MaxPlayers,
            IsOpen = true,
            IsVisible = true,
            CustomRoomProperties = new Hashtable { { HostNameProperty, playerName } },
            CustomRoomPropertiesForLobby = new[] { HostNameProperty },
            CleanupCacheOnLeave = true
        };

        PhotonNetwork.CreateRoom(roomName, roomOptions, TypedLobby.Default);
    }

    public void RequestRoomRefresh()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            RefreshRoomButtons();
            return;
        }

        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
            RefreshRoomButtons();
            return;
        }

        JoinLobbyIfReady();
        RefreshRoomButtons();
    }

    private void JoinRoom(RoomInfo room)
    {
        if (room == null)
        {
            return;
        }

        PhotonNetwork.NickName = GetPlayerName();
        PhotonNetwork.JoinRoom(room.Name);
    }

    private void RefreshRoomButtons()
    {
        ClearSpawnedRoomButtons();

        if (roomButtonTemplate == null || roomContent == null)
        {
            return;
        }

        roomButtonTemplate.gameObject.SetActive(false);

        foreach (RoomInfo room in cachedRooms.Values)
        {
            if (room == null || room.PlayerCount <= 0 || !room.IsOpen || !room.IsVisible)
            {
                continue;
            }

            Button roomButton = Instantiate(roomButtonTemplate, roomContent);
            roomButton.name = $"{RoomButtonName} - {room.Name}";
            roomButton.gameObject.SetActive(true);
            SetButtonLabel(roomButton.gameObject, GetRoomDisplayName(room));
            roomButton.onClick.RemoveAllListeners();
            RoomInfo selectedRoom = room;
            roomButton.onClick.AddListener(() => JoinRoom(selectedRoom));
            spawnedRoomButtons.Add(roomButton.gameObject);
        }
    }

    private void ClearSpawnedRoomButtons()
    {
        foreach (GameObject roomButton in spawnedRoomButtons)
        {
            if (roomButton != null)
            {
                Destroy(roomButton);
            }
        }

        spawnedRoomButtons.Clear();
    }

    private void JoinLobbyIfReady()
    {
        if (!PhotonNetwork.IsConnectedAndReady || PhotonNetwork.InRoom || PhotonNetwork.InLobby)
        {
            return;
        }

        PhotonNetwork.JoinLobby();
    }

    private void LoadLevelOne()
    {
        if (loadRequested)
        {
            return;
        }

        loadRequested = true;
        PhotonNetwork.LoadLevel(LevelSceneIndex);
    }

    private string GetPlayerName()
    {
        string playerName = nameInput != null ? nameInput.text.Trim() : string.Empty;

        if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = $"guest{Random.Range(1000, 9999)}";
            if (nameInput != null)
            {
                nameInput.text = playerName;
            }
        }

        return playerName;
    }

    private string GetRoomDisplayName(RoomInfo room)
    {
        if (room.CustomProperties != null && room.CustomProperties.TryGetValue(HostNameProperty, out object hostName))
        {
            return $"{hostName}'s Room ({room.PlayerCount}/{room.MaxPlayers})";
        }

        return $"{room.Name} ({room.PlayerCount}/{room.MaxPlayers})";
    }

    private void AutoAssignSceneReferences()
    {
        if (startGroup == null)
        {
            startGroup = FindSceneObject("StartGroup");
        }

        if (lobbyGroup == null)
        {
            lobbyGroup = FindSceneObject("LobbyGroup");
        }

        if (nameInput == null)
        {
            nameInput = FindNamedComponent<TMP_InputField>("NameInput");
        }

        if (playButton == null)
        {
            playButton = FindNamedComponent<Button>("PlayButton");
        }

        if (quitButton == null)
        {
            quitButton = FindNamedComponent<Button>("QuitButton");
        }

        if (backButton == null)
        {
            backButton = FindNamedComponent<Button>("BackButton");
        }

        if (refreshButton == null)
        {
            refreshButton = FindNamedComponent<Button>("RefreshButton");
        }

        if (createRoomButton == null)
        {
            createRoomButton = FindNamedComponent<Button>("CreateRoomButton");
        }

        GameObject roomSelectorPanel = FindSceneObject("RoomSelectorPanel");
        if (roomContent == null && roomSelectorPanel != null)
        {
            Transform content = FindChildRecursive(roomSelectorPanel.transform, "Content");
            roomContent = content != null ? content as RectTransform : null;
        }

        if (roomButtonTemplate == null)
        {
            roomButtonTemplate = FindNamedComponent<Button>(RoomButtonName);
            if (roomButtonTemplate == null)
            {
                roomButtonTemplate = FindNamedComponent<Button>(RoomButtonCompactName);
            }
        }

        if (roomContent == null && roomButtonTemplate != null)
        {
            roomContent = roomButtonTemplate.transform.parent as RectTransform;
        }
    }

    private void WireButtonEvents()
    {
        AddButtonListener(playButton, ShowLobbyGroup);
        AddButtonListener(backButton, ShowStartGroup);
        AddButtonListener(refreshButton, RequestRoomRefresh);
        AddButtonListener(createRoomButton, CreateRoom);
        AddButtonListener(quitButton, QuitGame);
    }

    private void AddButtonListener(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveAllListeners();

        if (HasPersistentListener(button, action.Method.Name))
        {
            return;
        }

        button.onClick.AddListener(action);
    }

    private bool HasPersistentListener(Button button, string methodName)
    {
        for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
        {
            if (button.onClick.GetPersistentTarget(i) == this && button.onClick.GetPersistentMethodName(i) == methodName)
            {
                return true;
            }
        }

        return false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void SetGroupActive(GameObject group, bool isActive)
    {
        if (group != null)
        {
            group.SetActive(isActive);
        }
    }

    private void SetButtonLabel(GameObject buttonObject, string label)
    {
        TMP_Text tmpText = buttonObject.GetComponentInChildren<TMP_Text>(true);
        if (tmpText != null)
        {
            tmpText.text = label;
            return;
        }

        Text text = buttonObject.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.text = label;
        }
    }

    private T FindNamedComponent<T>(string objectName) where T : Component
    {
        GameObject target = FindSceneObject(objectName);
        return target != null ? target.GetComponent<T>() : null;
    }

    private GameObject FindSceneObject(string objectName)
    {
        foreach (GameObject sceneObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (sceneObject.name == objectName && sceneObject.scene.IsValid() && sceneObject.scene == SceneManager.GetActiveScene())
            {
                return sceneObject;
            }
        }

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform match = FindChildRecursive(child, childName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
