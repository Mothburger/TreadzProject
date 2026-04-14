using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviour
{
    private const string VirtualCameraTag = "VCam";

    GameObject PlayerObj;
    ///Transform GunTransform;
    GameObject PlayerGun;
    PhotonView photonView;
    public GameObject AimReticle;
    private bool isDestroyed;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private int playerSlot = -1;
    CinemachineVirtualCamera vcam;
    private GameObject localAimReticle;
    private GameObject localCameraBounds;
    private CinemachineTargetGroup localCameraTargetGroup;
    [SerializeField] private string mouseObjectResourceName = "MouseObject";
    [SerializeField] private Vector2 mouseBoundsSize = new Vector2(40f, 40f);
    [SerializeField]
    float StartingMovementSpeed = 0.0f;
    [SerializeField]
    float MaxMovementSpeed = 5.0f;
    float MovementSpeed = 0.0f;
    [SerializeField]
    float TankRotationSpeed = 20.0f;
    [SerializeField]
    float GunRotationSpeed = 20.0f;
    FollowPlayer FollowPlayerScript; 
    void Start()
    {
        PlayerObj = gameObject;
        photonView = PlayerObj.GetComponent<PhotonView>();
        PlayerGun = transform.childCount > 0 ? transform.GetChild(0).gameObject : null;

        if (IsLocalOwner())
        {
            SetupLocalOwnerFollowTargets();
        }

        PlayerSpawning.Instance?.RegisterPlayer(this);
    }

    void OnDestroy()
    {
        if (PlayerSpawning.Instance != null)
        {
            PlayerSpawning.Instance.UnregisterPlayer(this);
        }

        if (localAimReticle != null)
        {
            Destroy(localAimReticle);
        }

        if (localCameraBounds != null)
        {
            Destroy(localCameraBounds);
        }

        if (localCameraTargetGroup != null)
        {
            Destroy(localCameraTargetGroup.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDestroyed)
        {
            return;
        }

        if (!IsLocalOwner())
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
       
        if (AimReticle != null && PlayerGun != null)
        {
            Vector2 direction = AimReticle.transform.position - PlayerGun.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
            PlayerGun.transform.rotation = Quaternion.Slerp(PlayerGun.transform.rotation, targetRotation, Time.deltaTime * GunRotationSpeed);
        }
    
    }

    private bool IsLocalOwner()
    {
        return photonView == null || photonView.IsMine || !PhotonNetwork.IsConnected;
    }

    private void SetupLocalOwnerFollowTargets()
    {
        CreateCameraBoundsFollower();
        CreateAimReticle();
        HookCameraToLocalTargets();
    }

    private void CreateCameraBoundsFollower()
    {
        localCameraBounds = new GameObject("Local Mouse BoundingBox");
        localCameraBounds.transform.position = transform.position;

        BoxCollider2D boundsCollider = localCameraBounds.AddComponent<BoxCollider2D>();
        boundsCollider.isTrigger = true;
        boundsCollider.size = mouseBoundsSize;

        FollowPlayerScript = localCameraBounds.AddComponent<FollowPlayer>();
        FollowPlayerScript.SetPlayer(PlayerObj);
    }

    private void CreateAimReticle()
    {
        if (AimReticle == null)
        {
            GameObject mousePrefab = Resources.Load<GameObject>(mouseObjectResourceName);
            AimReticle = mousePrefab != null
                ? Instantiate(mousePrefab, transform.position, Quaternion.identity)
                : new GameObject("MouseObject");
        }

        AimReticle.name = "MouseObject";
        localAimReticle = AimReticle;

        MouseFollow mouseFollow = AimReticle.GetComponent<MouseFollow>();
        if (mouseFollow != null)
        {
            mouseFollow.SetCameraBounds(localCameraBounds);
        }
    }

    private void HookCameraToLocalTargets()
    {
        if (vcam == null)
        {
            GameObject virtualCameraObject = GameObject.FindWithTag(VirtualCameraTag);
            vcam = virtualCameraObject != null
                ? virtualCameraObject.GetComponent<CinemachineVirtualCamera>()
                : FindObjectOfType<CinemachineVirtualCamera>();
        }

        if (vcam == null)
        {
            return;
        }

        GameObject targetGroupObject = new GameObject("Local Camera Target Group");
        localCameraTargetGroup = targetGroupObject.AddComponent<CinemachineTargetGroup>();
        localCameraTargetGroup.m_Targets = new[]
        {
            new CinemachineTargetGroup.Target { target = transform, weight = 1f, radius = 4f },
            new CinemachineTargetGroup.Target { target = AimReticle.transform, weight = 1f, radius = 0f }
        };

        vcam.Follow = localCameraTargetGroup.transform;
        vcam.LookAt = localCameraTargetGroup.transform;
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
