using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
    GameObject PlayerObj;
    ///Transform GunTransform;
    GameObject PlayerGun;
    PhotonView photonView;
    private bool isDestroyed;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private int playerSlot = -1;
    CinemachineVirtualCamera vcam;
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
        PlayerSpawning.Instance?.RegisterPlayer(this);
        if (photonView.IsMine == true && PhotonNetwork.IsConnected == true)
        {
        vcam = GameObject.FindWithTag("VCam").GetComponent<CinemachineVirtualCamera>();
        vcam.Follow = gameObject.transform;
        }
    }

    void OnDestroy()
    {
        if (PlayerSpawning.Instance != null)
        {
            PlayerSpawning.Instance.UnregisterPlayer(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDestroyed)
        {
            return;
        }

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
            
            MovementSpeed +=0.005f;
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
            //Debug.Log("Movement Speed:" + MovementSpeed);
            
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

    [PunRPC]
    public void DisappearTank()
    {
        if (isDestroyed)
        {
            return;
        }

        isDestroyed = true;
        MovementSpeed = 0f;

        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = false;
        }

        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = false;
        }

        foreach (var rigidbody in GetComponentsInChildren<Rigidbody2D>(true))
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;
            rigidbody.simulated = false;
        }

        PlayerSpawning.Instance?.HandleTankDestroyed(this);
    }

    [PunRPC]
    public void RespawnTank(Vector3 spawnPosition, Quaternion spawnRotation)
    {
        startingPosition = spawnPosition;
        startingRotation = spawnRotation;
        isDestroyed = false;
        MovementSpeed = StartingMovementSpeed;

        transform.SetPositionAndRotation(spawnPosition, spawnRotation);

        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>(true))
        {
            renderer.enabled = true;
        }

        foreach (var collider in GetComponentsInChildren<Collider2D>(true))
        {
            collider.enabled = true;
        }

        foreach (var rigidbody in GetComponentsInChildren<Rigidbody2D>(true))
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;
            rigidbody.simulated = true;
        }
    }

    public void SetSpawnData(Vector3 spawnPosition, Quaternion spawnRotation, int slotIndex)
    {
        startingPosition = spawnPosition;
        startingRotation = spawnRotation;
        playerSlot = slotIndex;
    }

    public void BroadcastRespawn()
    {
        photonView.RPC(nameof(RespawnTank), RpcTarget.AllBuffered, startingPosition, startingRotation);
    }

    public bool IsDestroyed => isDestroyed;

    public bool IsMine => photonView != null && photonView.IsMine;

    public int OwnerActorNumber => photonView != null && photonView.Owner != null ? photonView.OwnerActorNr : -1;

    public Player Owner => photonView != null ? photonView.Owner : null;

    public string GetRoundLabel()
    {
        if (playerSlot >= 0)
        {
            return $"Player {playerSlot + 1}";
        }

        return photonView != null && photonView.Owner != null
            ? $"Player {photonView.OwnerActorNr}"
            : "Player";
    }
}
