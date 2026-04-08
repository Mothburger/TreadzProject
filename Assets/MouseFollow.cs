using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class MouseFollow : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust this in the Inspector to change follow speed
    GameObject PlayerObj;
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    [SerializeField] private GameObject CameraBoundsObj;
    private BoxCollider2D boundaryCollider;
    PhotonView photonView;

    void Start()
    {
       // PlayerObj = GameObject.FindWithTag("Player");
       // photonView = PlayerObj.transform.parent.gameObject.GetComponent<PhotonView>();
        boundaryCollider = CameraBoundsObj.GetComponent<BoxCollider2D>();
    }
    void Update()
    {
        ///Convert mouse position from screen space to world space
        //if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
        //{
        //    return;
        //}
        targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = transform.position.z; // Keep the camera's original Z position for 2D

        
    }

    void FixedUpdate()
    {
        
        // Smoothly move the camera towards the target position
        //{
          //  return;
        //}
       if (boundaryCollider.bounds.Contains(targetPosition))
        {
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        lastPosition = targetPosition;
        }
        else
        {
        transform.position = Vector3.Lerp(transform.position, lastPosition, moveSpeed * Time.deltaTime);
        }
    }
}
