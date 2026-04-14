using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Shooting : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float shootDelay = 1.0f; 
    private float shootTimer = 0f;
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
        shootTimer += Time.deltaTime;
        if ((Input.GetKeyDown(KeyCode.Mouse0) && shootTimer >= shootDelay))
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Missing bulletPrefab or firePoint on Shooting script.");
            return;
        }

        Vector2 fireDirection = firePoint.up;
        GameObject bullet = PhotonNetwork.Instantiate(
            bulletPrefab.name,
            firePoint.position,
            firePoint.rotation,
            0,
            new object[] { fireDirection.x, fireDirection.y, bulletSpeed }
        );

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = fireDirection * bulletSpeed;
        }

        // Optional cleanup after a few seconds
        DestroyBulletAfterTime destroyScript = bullet.GetComponent<DestroyBulletAfterTime>();
        if (destroyScript != null)
        {
            destroyScript.BeginCountdown();
        }
    }
}
