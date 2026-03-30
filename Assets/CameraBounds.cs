using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBounds : MonoBehaviour
{
    void Start()
    {
        FitColliderToScreen();
    }

    void FitColliderToScreen()
    {
        PolygonCollider2D pc2d = GetComponent<PolygonCollider2D>();
        Camera cam = Camera.main;
        
        // Calculate screen boundaries in world space
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        // Define 4 points for the rectangle (local space)
        Vector2[] newPoints = new Vector2[4];
        newPoints[0] = new Vector2(-width / 2f, -height / 2f);
        newPoints[1] = new Vector2(-width / 2f, height / 2f);
        newPoints[2] = new Vector2(width / 2f, height / 2f);
        newPoints[3] = new Vector2(width / 2f, -height / 2f);

        // Update the collider
        pc2d.points = newPoints;
        transform.position = cam.transform.position; 
    }
       
}