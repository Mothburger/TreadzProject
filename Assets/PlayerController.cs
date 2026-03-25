using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    GameObject PlayerObj;
    ///Transform GunTransform;
    GameObject PlayerGun;
    [SerializeField]
    float MovementSpeed = 20.0f;
    [SerializeField]
    float RotationSpeed = 20.0f;
    void Start()
    {
        PlayerObj = gameObject;
        PlayerGun = this.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            transform.position += (transform.up * MovementSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(0,0, RotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(0,0, -RotationSpeed * Time.deltaTime); 
        }
        if (Input.GetKey(KeyCode.S))
        {
            
        }
        if (Input.GetKey(KeyCode.Q))
        {
            
        }
        if (Input.GetKey(KeyCode.E))
        {
            
        }
    
    }
}
