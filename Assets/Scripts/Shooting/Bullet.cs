using UnityEngine;
using Photon.Pun;

public class Bullet : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    private Rigidbody2D rb;
    [SerializeField] private float lifetime = 3f;

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
}