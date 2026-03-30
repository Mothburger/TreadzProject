using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Bullet : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private Rigidbody2D rb;
    [SerializeField] private float lifetime = 3f;
    private bool hasHit;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;
        if (data != null && data.Length == 3)
        {
            Vector2 dir = new Vector2((float)data[0], (float)data[1]);
            float speed = (float)data[2];

            rb.velocity = dir * speed;
        }

        Invoke(nameof(DestroyMe), lifetime);
    }

    void DestroyMe()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryHandleHit(other.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryHandleHit(collision.gameObject);
    }

    private void TryHandleHit(GameObject otherObject)
    {
        if (hasHit || !photonView.IsMine)
        {
            return;
        }

        Room currentRoom = PhotonNetwork.CurrentRoom;
        if (currentRoom == null || currentRoom.PlayerCount != 2)
        {
            return;
        }

        PhotonView targetPhotonView = otherObject.GetComponentInParent<PhotonView>();
        if (targetPhotonView == null || targetPhotonView.OwnerActorNr == photonView.OwnerActorNr)
        {
            return;
        }

        PlayerController targetController = targetPhotonView.GetComponent<PlayerController>();
        if (targetController == null)
        {
            return;
        }

        hasHit = true;
        targetPhotonView.RPC(nameof(PlayerController.DisappearTank), RpcTarget.AllBuffered);
        PhotonNetwork.Destroy(gameObject);
    }
}
