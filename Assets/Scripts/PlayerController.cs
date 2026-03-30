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
    float StartingMovementSpeed = 0.0f;
    [SerializeField]
    float MaxMovementSpeed = 5.0f;
    float MovementSpeed = 0.0f;
    [SerializeField]
    float TankRotationSpeed = 20.0f;
    [SerializeField]
    float GunRotationSpeed = 20.0f;
    void Start()
    {
        PlayerObj = gameObject;
        photonView = PlayerObj.GetComponent<PhotonView>();
        PlayerGun = this.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (photonView.IsMine == false && PhotonNetwork.IsConnected == true)
    {
        return;
    }
        if (Input.GetKeyUp(KeyCode.W))
            {
            MovementSpeed = StartingMovementSpeed;
            }
        if (Input.GetKey(KeyCode.W))
        {
            
            MovementSpeed +=0.001f;
            if (MovementSpeed >= MaxMovementSpeed)
            {
                MovementSpeed = MaxMovementSpeed;
            }
            transform.position += (transform.up * MovementSpeed * Time.deltaTime);
            Debug.Log("Movement Speed:" + MovementSpeed);
            
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0,0, TankRotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0,0, -TankRotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKeyUp(KeyCode.S))
            {
            MovementSpeed = StartingMovementSpeed;
            }
        if (Input.GetKey(KeyCode.S))
        {
            MovementSpeed +=0.001f;
            if (MovementSpeed >= MaxMovementSpeed)
            {
                MovementSpeed = MaxMovementSpeed;
            }
            transform.position += (transform.up * -MovementSpeed * Time.deltaTime);
            Debug.Log("Movement Speed:" + MovementSpeed);
            
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
