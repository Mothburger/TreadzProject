using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowPlayer : MonoBehaviour
{
    public GameObject PlayerObj;
    PhotonView photonView;
    [SerializeField]
    void Start()
    {
        photonView = gameObject.GetComponent<PhotonView>();
    }

    void Update()
    {
      //  if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
    //{
       /// return;
    //}
        //if (target != null)
       // {
            // Move the current object towards the target's position
            transform.position = PlayerObj.transform.position;
       /// }
    }
}
