using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerSpawning : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView playerPrefab;
    [SerializeField] private Transform spawnPoint1;
    [SerializeField] private Transform spawnPoint2;
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private TMP_Text roundResultText;
    [SerializeField] private int playersToStart = 2;
    [SerializeField] private float roundResetDelay = 1f;

    private readonly List<PlayerController> activePlayers = new List<PlayerController>();
    private bool localPlayerSpawned;
    private bool roundEnding;

    public static PlayerSpawning Instance { get; private set; }

    void Awake()
    {
        Instance = this;
        AutoAssignSceneReferences();
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
    }

    public override void OnJoinedRoom()
    {
        localPlayerSpawned = false;
        roundEnding = false;
        UpdateWaitingText();
        TrySpawnLocalPlayer();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateWaitingText();
        TrySpawnLocalPlayer();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        roundEnding = false;
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
            int playerSlot = GetPlayerSlot(playerController.Owner);
            Transform spawnPoint = GetSpawnPoint(playerSlot);

            if (spawnPoint != null)
            {
                playerController.SetSpawnData(spawnPoint.position, spawnPoint.rotation, playerSlot);
            }
        }
    }

    public void UnregisterPlayer(PlayerController playerController)
    {
        activePlayers.Remove(playerController);
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
        ShowRoundResult($"{winner.GetRoundLabel()} wins the round!");
        StartCoroutine(RespawnRoundAfterDelay());
    }

    private IEnumerator RespawnRoundAfterDelay()
    {
        yield return new WaitForSeconds(roundResetDelay);

        foreach (PlayerController player in activePlayers.ToArray())
        {
            if (player == null || !player.IsMine)
            {
                continue;
            }

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
        return playerSlot == 0 ? spawnPoint1 : spawnPoint2;
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
