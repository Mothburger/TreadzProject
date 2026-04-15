using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SimpleLauncher : MonoBehaviourPunCallbacks
{

    public PhotonView playerPrefab;
    private const byte MaxPlayers = 2;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        EnsurePlayerName();

        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");

        if (!PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: new Photon.Realtime.RoomOptions
            {
                MaxPlayers = MaxPlayers,
                IsOpen = true,
                IsVisible = true
            });
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room.");
    }

    private void EnsurePlayerName()
    {
        if (!string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            return;
        }

        PhotonNetwork.NickName = $"guest{Random.Range(1000, 9999)}";
    }

}
