using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBulletAfterTime : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    public void BeginCountdown()
    {
        StartCoroutine(DestroyAfterSeconds());
    }

    private IEnumerator DestroyAfterSeconds()
    {
        yield return new WaitForSeconds(lifetime);

        PhotonView pv = GetComponent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }
}