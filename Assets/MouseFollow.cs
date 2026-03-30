using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust this in the Inspector to change follow speed

    private Vector3 targetPosition;

    void Update()
    {
        // Convert mouse position from screen space to world space
        targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = transform.position.z; // Keep the camera's original Z position for 2D

    }

    void FixedUpdate()
    {
        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
    }
}
