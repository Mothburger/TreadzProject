using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class FindPlayer : MonoBehaviour
{
    
      private bool isTaskDone = false;
        CinemachineVirtualCamera vcam;
      

    void OnAwake()
    {
        vcam = gameObject.GetComponent<CinemachineVirtualCamera>();
        StartCoroutine(FindPlayerNow());
    }

    IEnumerator FindPlayerNow()
    {
        vcam.Follow = GameObject.FindWithTag("Player").transform;
        if (vcam.Follow != null)
        {
            isTaskDone = true;
        }
        Debug.Log("Task Failing!");
        // Keep waiting while isTaskDone is false
        yield return new WaitUntil(() => isTaskDone == true);
        
        Debug.Log("Task Succeeded!");
    }

    // Call this method when the action actually succeeds
    public void CompleteTask()
    {
        isTaskDone = true;
    }
}
