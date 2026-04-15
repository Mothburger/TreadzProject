using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Scoring : MonoBehaviourPunCallbacks
{
    private const int MainMenuSceneIndex = 0;
    private const int LevelSceneIndex = 1;

    [SerializeField] private GameObject winOverlay;
    [SerializeField] private Transform pointsGroup;
    [SerializeField] private TMP_Text playerPointsTextTemplate;
    [SerializeField] private TMP_Text pointsToWinText;
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private AudioSource music;
    [SerializeField] private AudioSource matchResultAudioSource;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;
    [SerializeField] private int pointsToWin = 10;
    [SerializeField] private float returnToMenuDelay = 3f;

    private readonly Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private readonly Dictionary<int, TMP_Text> playerScoreTexts = new Dictionary<int, TMP_Text>();
    private bool gameEnded;
    private bool matchResultSoundPlayed;

    public static Scoring Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapCurrentScene()
    {
        SceneManager.sceneLoaded += BootstrapScene;
        BootstrapScene(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void BootstrapScene(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != LevelSceneIndex && scene.name != "Level1")
        {
            return;
        }

        if (FindObjectOfType<Scoring>() != null)
        {
            return;
        }

        new GameObject("Scoring").AddComponent<Scoring>();
    }

    private void Awake()
    {
        Instance = this;
        AutoAssignSceneReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Start()
    {
        RefreshPlayers();
        UpdatePointsToWinText();
        HidePopup();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshPlayers();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshPlayers();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(MainMenuSceneIndex);
    }

    public bool AwardPoint(Player winningPlayer)
    {
        if (winningPlayer == null || gameEnded)
        {
            return false;
        }

        RefreshPlayers();

        int actorNumber = winningPlayer.ActorNumber;
        if (!playerScores.ContainsKey(actorNumber))
        {
            playerScores[actorNumber] = 0;
        }

        playerScores[actorNumber]++;
        string playerName = GetPlayerDisplayName(winningPlayer);

        RefreshScoreTexts();

        if (playerScores[actorNumber] >= pointsToWin)
        {
            gameEnded = true;
            music.Stop();
            winOverlay.SetActive(true);
            ShowPopup($"{playerName} wins!");
            PlayMatchResultSound(winningPlayer);
            StartCoroutine(ReturnToMainMenuAfterDelay());
            return true;
        }

        ShowPopup($"{playerName} scored! ({playerScores[actorNumber]}/{pointsToWin})");
        return false;
    }

    public string GetPlayerDisplayName(Player player)
    {
        if (player == null)
        {
            return "Player";
        }

        return string.IsNullOrWhiteSpace(player.NickName)
            ? $"guest{player.ActorNumber}"
            : player.NickName;
    }

    private IEnumerator ReturnToMainMenuAfterDelay()
    {
        yield return new WaitForSeconds(returnToMenuDelay);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            yield break;
        }

        SceneManager.LoadScene(MainMenuSceneIndex);
    }

    private void RefreshPlayers()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == null)
            {
                continue;
            }

            if (!playerScores.ContainsKey(player.ActorNumber))
            {
                playerScores[player.ActorNumber] = 0;
            }
        }

        RemoveScoresForMissingPlayers();
        RebuildScoreTexts();
        RefreshScoreTexts();
    }

    private void RebuildScoreTexts()
    {
        if (playerPointsTextTemplate == null)
        {
            return;
        }

        if (pointsGroup == null)
        {
            pointsGroup = playerPointsTextTemplate.transform.parent;
        }

        foreach (TMP_Text scoreText in playerScoreTexts.Values)
        {
            if (scoreText != null && scoreText != playerPointsTextTemplate)
            {
                Destroy(scoreText.gameObject);
            }
        }

        playerScoreTexts.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; i++)
        {
            Player player = players[i];
            if (player == null)
            {
                continue;
            }

            TMP_Text scoreText = i == 0
                ? playerPointsTextTemplate
                : Instantiate(playerPointsTextTemplate, pointsGroup);

            scoreText.name = $"PlayerPointsText_{player.ActorNumber}";
            scoreText.gameObject.SetActive(true);
            playerScoreTexts[player.ActorNumber] = scoreText;
        }

        if (players.Length == 0)
        {
            playerPointsTextTemplate.gameObject.SetActive(false);
        }
    }

    private void RemoveScoresForMissingPlayers()
    {
        List<int> actorNumbersToRemove = new List<int>();

        foreach (int actorNumber in playerScores.Keys)
        {
            if (GetPlayerByActorNumber(actorNumber) != null)
            {
                continue;
            }

            actorNumbersToRemove.Add(actorNumber);
        }

        foreach (int actorNumber in actorNumbersToRemove)
        {
            playerScores.Remove(actorNumber);
        }
    }

    private void RefreshScoreTexts()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player == null || !playerScoreTexts.TryGetValue(player.ActorNumber, out TMP_Text scoreText) || scoreText == null)
            {
                continue;
            }

            int score = playerScores.TryGetValue(player.ActorNumber, out int storedScore) ? storedScore : 0;
            scoreText.text = $"{GetPlayerDisplayName(player)}: {score}";
        }
    }

    private Player GetPlayerByActorNumber(int actorNumber)
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (player != null && player.ActorNumber == actorNumber)
            {
                return player;
            }
        }

        return null;
    }

    private void ShowPopup(string message)
    {
        if (popupText == null)
        {
            return;
        }

        popupText.text = message;
        popupText.gameObject.SetActive(true);
    }

    private void HidePopup()
    {
        if (popupText != null)
        {
            popupText.gameObject.SetActive(false);
        }
    }

    private void PlayMatchResultSound(Player winningPlayer)
    {
        if (matchResultSoundPlayed)
        {
            return;
        }

        matchResultSoundPlayed = true;

        bool localPlayerWon = PhotonNetwork.LocalPlayer != null &&
            winningPlayer != null &&
            PhotonNetwork.LocalPlayer.ActorNumber == winningPlayer.ActorNumber;

        AudioClip resultClip = localPlayerWon ? winSound : loseSound;
        if (resultClip == null)
        {
            return;
        }

        if (matchResultAudioSource == null)
        {
            matchResultAudioSource = GetComponent<AudioSource>();
        }

        if (matchResultAudioSource == null)
        {
            matchResultAudioSource = gameObject.AddComponent<AudioSource>();
        }

        matchResultAudioSource.playOnAwake = false;
        matchResultAudioSource.spatialBlend = 0f;
        matchResultAudioSource.PlayOneShot(resultClip);
    }

    private void UpdatePointsToWinText()
    {
        if (pointsToWinText != null)
        {
            pointsToWinText.text = $"Points To Win: {pointsToWin}";
        }
    }

    private void AutoAssignSceneReferences()
    {
        if (pointsGroup == null)
        {
            GameObject pointsGroupObject = GameObject.Find("PointsGroup");
            if (pointsGroupObject != null)
            {
                pointsGroup = pointsGroupObject.transform;
            }
        }

        if (playerPointsTextTemplate == null)
        {
            GameObject playerPointsTextObject = GameObject.Find("PlayerPointsText");
            if (playerPointsTextObject != null)
            {
                playerPointsTextTemplate = playerPointsTextObject.GetComponent<TMP_Text>();
            }
        }

        if (pointsToWinText == null)
        {
            GameObject pointsToWinTextObject = GameObject.Find("PointsToWinText");
            if (pointsToWinTextObject != null)
            {
                pointsToWinText = pointsToWinTextObject.GetComponent<TMP_Text>();
            }
        }

        if (popupText == null)
        {
            GameObject popupTextObject = GameObject.Find("RoundResultText");
            if (popupTextObject != null)
            {
                popupText = popupTextObject.GetComponent<TMP_Text>();
            }
        }

        if (matchResultAudioSource == null)
        {
            matchResultAudioSource = GetComponent<AudioSource>();
        }
    }
}
