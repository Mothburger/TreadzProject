using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviour
{
    GameObject PlayerObj;
    ///Transform GunTransform;
    GameObject PlayerGun;
    PhotonView photonView;
    [SerializeField]
    float MovementSpeed = 20.0f;
    [SerializeField]
    float TankRotationSpeed = 20.0f;
    [SerializeField]
    float GunRotationSpeed = 20.0f;
    void Start()
    {
        PlayerObj = gameObject;
        PlayerGun = this.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
    {
        return;
    }
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += (transform.up * MovementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0,0, TankRotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0,0, -TankRotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.position -= (transform.up * MovementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.Q))
        {
            PlayerGun.transform.Rotate(0,0, GunRotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKey(KeyCode.E))
        {
            PlayerGun.transform.Rotate(0,0, -GunRotationSpeed * Time.deltaTime); 
        }
    
    }
}
