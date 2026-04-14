using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public GameObject PlayerObj;

    public void SetPlayer(GameObject playerObj)
    {
        PlayerObj = playerObj;
    }

    void Update()
    {
        if (PlayerObj == null)
        {
            return;
        }

        transform.position = PlayerObj.transform.position;
    }
}
