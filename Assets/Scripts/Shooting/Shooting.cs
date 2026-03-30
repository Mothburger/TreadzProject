using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private Transform firePoint;

    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponentInParent<PhotonView>();
    }

    void Update()
    {
        // Only let the local player shoot from this tank
        if (photonView != null && !photonView.IsMine && PhotonNetwork.IsConnected)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missing bulletPrefab or firePoint on Shooting script.");
            return;
        }

        GameObject bullet = PhotonNetwork.Instantiate(
            bulletPrefab.name,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = firePoint.up * bulletSpeed;
        }

        // Optional cleanup after a few seconds
        DestroyBulletAfterTime destroyScript = bullet.GetComponent<DestroyBulletAfterTime>();
        if (destroyScript != null)
        {
            destroyScript.BeginCountdown();
        }
    }
}
