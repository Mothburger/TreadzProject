using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerSpawning : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private Transform[] randomSpawnPoints;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private TMP_Text roundResultText;
    [SerializeField] private Image connectingPanelImage;
    [SerializeField] private int playersToStart = 2;
    [SerializeField] private float roundResetDelay = 1f;

    private const string RoundSpawnSeedProperty = "RoundSpawnSeed";

    private readonly List<PlayerController> activePlayers = new List<PlayerController>();
    private bool localPlayerSpawned;
    private bool roundEnding;
    private Color connectingPanelBaseColor = Color.clear;

    public static PlayerSpawning Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        AutoAssignSceneReferences();

        if (connectingPanelImage != null)
        {
            connectingPanelBaseColor = connectingPanelImage.color;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Start()
    {
        UpdateWaitingText();
        TrySpawnLocalPlayer();
    }

    public override void OnJoinedRoom()
    {
        localPlayerSpawned = false;
        roundEnding = false;
        EnsureRoundSpawnSeed();
        UpdateWaitingText();
        TrySpawnLocalPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        EnsureRoundSpawnSeed();
        UpdateWaitingText();
        TrySpawnLocalPlayer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roundEnding = false;
        DespawnOwnedNetworkObjectsIfWaiting();
        UpdateWaitingText();
    }

    public void RegisterPlayer(PlayerController playerController)
    {
        if (playerController == null || activePlayers.Contains(playerController))
        {
            return;
        }

        activePlayers.Add(playerController);

        if (playerController.IsMine)
        {
            ApplySpawnDataToPlayer(playerController);
        }
    }

    public void UnregisterPlayer(PlayerController playerController)
    {
        activePlayers.Remove(playerController);

        if (playerController != null && playerController.IsMine)
        {
            localPlayerSpawned = false;
        }
    }

    public void HandleTankDestroyed(PlayerController destroyedPlayer)
    {
        if (destroyedPlayer == null || roundEnding || PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < playersToStart)
        {
            return;
        }

        PlayerController winner = GetWinnerAfterDestruction(destroyedPlayer);
        if (winner == null)
        {
            return;
        }

        roundEnding = true;

        bool gameEnded = Scoring.Instance != null && Scoring.Instance.AwardPoint(winner.Owner);
        if (gameEnded)
        {
            return;
        }

        ShowRoundResult($"{GetPlayerDisplayName(winner)} scored!");
        StartCoroutine(RespawnRoundAfterDelay());
    }

    private IEnumerator RespawnRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundResetDelay);

        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < playersToStart)
        {
            roundEnding = false;
            DespawnOwnedNetworkObjectsIfWaiting();
            UpdateWaitingText();
            yield break;
        }

        PublishNewRoundSpawnSeed();
        yield return null;

        HideRoundResult();

        foreach (PlayerController player in activePlayers.ToArray())
        {
            if (player == null || !player.IsMine)
            {
                continue;
            }

            ApplySpawnDataToPlayer(player);
            player.BroadcastRespawn();
        }

        roundEnding = false;
        UpdateWaitingText();
    }

    private void TrySpawnLocalPlayer()
    {
        if (!PhotonNetwork.InRoom || localPlayerSpawned)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < playersToStart)
        {
            DespawnOwnedNetworkObjectsIfWaiting();
            return;
        }

        int playerSlot = GetPlayerSlot(PhotonNetwork.LocalPlayer);
        Transform spawnPoint = GetSpawnPoint(playerSlot);
        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion spawnRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        PhotonNetwork.Instantiate(playerPrefab.name, spawnPosition, spawnRotation);
        localPlayerSpawned = true;
        HideWaitingText();
    }

    private void UpdateWaitingText()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null)
        {
            ShowWaitingText($"Waiting for players (0/{playersToStart})");
            return;
        }

        if (PhotonNetwork.CurrentRoom.PlayerCount < playersToStart)
        {
            DespawnOwnedNetworkObjectsIfWaiting();
            ShowWaitingText($"Waiting for players ({PhotonNetwork.CurrentRoom.PlayerCount}/{playersToStart})");
            return;
        }

        if (!roundEnding)
        {
            HideWaitingText();
        }
    }

    private void ShowWaitingText(string message)
    {
        SetConnectingPanelAlpha(connectingPanelBaseColor.a);

        if (waitingText == null)
        {
            return;
        }

        waitingText.text = message;
        waitingText.gameObject.SetActive(true);
        HideRoundResult();
    }

    private void HideWaitingText()
    {
        SetConnectingPanelAlpha(0f);

        if (waitingText != null)
        {
            waitingText.gameObject.SetActive(false);
        }
    }

    private void ShowRoundResult(string message)
    {
        HideWaitingText();

        if (roundResultText == null)
        {
            return;
        }

        roundResultText.text = message;
        roundResultText.gameObject.SetActive(true);
    }

    private void HideRoundResult()
    {
        if (roundResultText != null)
        {
            roundResultText.gameObject.SetActive(false);
        }
    }

    private void SetConnectingPanelAlpha(float alpha)
    {
        if (connectingPanelImage == null)
        {
            return;
        }

        Color panelColor = connectingPanelBaseColor;
        panelColor.a = alpha;
        connectingPanelImage.color = panelColor;
    }

    private PlayerController GetWinnerAfterDestruction(PlayerController destroyedPlayer)
    {
        foreach (PlayerController player in activePlayers.ToArray())
        {
            if (player == null || player == destroyedPlayer || player.IsDestroyed)
            {
                continue;
            }

            return player;
        }

        return null;
    }

    private void DespawnOwnedNetworkObjectsIfWaiting()
    {
        if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount >= playersToStart)
        {
            return;
        }

        foreach (PlayerController player in activePlayers.ToArray())
        {
            if (player == null || !player.IsMine)
            {
                continue;
            }

            PhotonNetwork.Destroy(player.gameObject);
        }

        foreach (Bullet bullet in FindObjectsOfType<Bullet>())
        {
            PhotonView bulletView = bullet.GetComponent<PhotonView>();
            if (bulletView != null && bulletView.IsMine)
            {
                PhotonNetwork.Destroy(bullet.gameObject);
            }
        }

        localPlayerSpawned = false;
    }

    private string GetPlayerDisplayName(PlayerController player)
    {
        if (player == null)
        {
            return "Player";
        }

        if (Scoring.Instance != null)
        {
            return Scoring.Instance.GetPlayerDisplayName(player.Owner);
        }

        return player.Owner != null && !string.IsNullOrWhiteSpace(player.Owner.NickName)
            ? player.Owner.NickName
            : player.GetRoundLabel();
    }

    private int GetPlayerSlot(Player player)
    {
        if (player == null)
        {
            return 0;
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].ActorNumber == player.ActorNumber)
            {
                return i;
            }
        }

        return 0;
    }

    private Transform GetSpawnPoint(int playerSlot)
    {
        Transform[] usableSpawnPoints = GetUsableRandomSpawnPoints();
        if (usableSpawnPoints.Length > 0)
        {
            int[] shuffledIndices = GetShuffledSpawnPointIndices(usableSpawnPoints.Length);
            int spawnIndex = shuffledIndices[Mathf.Abs(playerSlot) % shuffledIndices.Length];
            return usableSpawnPoints[spawnIndex];
        }

        return playerSlot == 0 ? spawnPoint1 : spawnPoint2;
    }

    private void ApplySpawnDataToPlayer(PlayerController playerController)
    {
        if (playerController == null)
        {
            return;
        }

        int playerSlot = GetPlayerSlot(playerController.Owner);
        Transform spawnPoint = GetSpawnPoint(playerSlot);

        if (spawnPoint != null)
        {
            playerController.SetSpawnData(spawnPoint.position, spawnPoint.rotation, playerSlot);
        }
    }

    private void EnsureRoundSpawnSeed()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.PlayerCount < playersToStart)
        {
            return;
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoundSpawnSeedProperty))
        {
            return;
        }

        PublishNewRoundSpawnSeed();
    }

    private void PublishNewRoundSpawnSeed()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new PhotonHashtable
        {
            { RoundSpawnSeedProperty, Random.Range(1, int.MaxValue) }
        });
    }

    private int GetRoundSpawnSeed()
    {
        if (PhotonNetwork.CurrentRoom != null &&
            PhotonNetwork.CurrentRoom.CustomProperties != null &&
            PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(RoundSpawnSeedProperty, out object seedValue) &&
            seedValue is int seed)
        {
            return seed;
        }

        return 0;
    }

    private Transform[] GetUsableRandomSpawnPoints()
    {
        if (randomSpawnPoints == null || randomSpawnPoints.Length == 0)
        {
            return new Transform[0];
        }

        List<Transform> usableSpawnPoints = new List<Transform>();
        foreach (Transform spawnPoint in randomSpawnPoints)
        {
            if (spawnPoint != null)
            {
                usableSpawnPoints.Add(spawnPoint);
            }
        }

        return usableSpawnPoints.ToArray();
    }

    private int[] GetShuffledSpawnPointIndices(int count)
    {
        int[] indices = new int[count];
        for (int i = 0; i < indices.Length; i++)
        {
            indices[i] = i;
        }

        System.Random random = new System.Random(GetRoundSpawnSeed());
        for (int i = indices.Length - 1; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            int previousValue = indices[i];
            indices[i] = indices[swapIndex];
            indices[swapIndex] = previousValue;
        }

        return indices;
    }

    private void AutoAssignSceneReferences()
    {
        if (waitingText == null)
        {
            GameObject waitingTextObject = GameObject.Find("WaitingForPlayersText");
            if (waitingTextObject != null)
            {
                waitingText = waitingTextObject.GetComponent<TMP_Text>();
            }
        }

        if (roundResultText == null)
        {
            GameObject roundResultTextObject = GameObject.Find("RoundResultText");
            if (roundResultTextObject != null)
            {
                roundResultText = roundResultTextObject.GetComponent<TMP_Text>();
            }
        }

        if (connectingPanelImage == null)
        {
            GameObject connectingPanelObject = GameObject.Find("ConnectingPanel");
            if (connectingPanelObject != null)
            {
                connectingPanelImage = connectingPanelObject.GetComponent<Image>();
            }
        }

        if (spawnPoint1 == null)
        {
            GameObject spawnOne = GameObject.Find("SpawnPoint1");
            if (spawnOne != null)
            {
                spawnPoint1 = spawnOne.transform;
            }
        }

        if (spawnPoint2 == null)
        {
            GameObject spawnTwo = GameObject.Find("SpawnPoint2");
            if (spawnTwo != null)
            {
                spawnPoint2 = spawnTwo.transform;
            }
        }
    }
}
