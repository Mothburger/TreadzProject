using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FollowPlayer : MonoBehaviour
{
    Transform target;
    PhotonView photonView;
    [SerializeField]
    float speed = 2.0f;
    void Start()
    {
        target = this.gameObject.transform.GetChild(0).gameObject.transform;
        photonView = gameObject.GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
    {
        return;
    }
        if (target != null)
        {
            // Move the current object towards the target's position
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }
}
