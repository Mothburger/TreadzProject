using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollow : MonoBehaviour
{
    public float moveSpeed = 5f; // Adjust this in the Inspector to change follow speed
    private Vector3 targetPosition;
    private Vector3 lastPosition;
    [SerializeField] private GameObject CameraBoundsObj;
    private BoxCollider2D boundaryCollider;

    void Start()
    {
        SetCameraBounds(CameraBoundsObj);
        targetPosition = transform.position;
        lastPosition = transform.position;
    }

    public void SetCameraBounds(GameObject cameraBoundsObj)
    {
        CameraBoundsObj = cameraBoundsObj;
        boundaryCollider = CameraBoundsObj != null ? CameraBoundsObj.GetComponent<BoxCollider2D>() : null;
    }

    void Update()
    {
        ///Convert mouse position from screen space to world space
        if (Camera.main == null)
        {
            return;
        }

        targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition.z = transform.position.z; // Keep the camera's original Z position for 2D

        
    }

    void FixedUpdate()
    {
        
        // Smoothly move the camera towards the target position
        if (boundaryCollider == null)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            lastPosition = targetPosition;
            return;
        }

        if (boundaryCollider.bounds.Contains(targetPosition))
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            lastPosition = targetPosition;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, lastPosition, moveSpeed * Time.deltaTime);
        }
    }
}
