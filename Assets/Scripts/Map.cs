using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class Map : MonoBehaviour
{
    public Transform[] spawnPoints;

    public (Transform spawn1,  Transform spawn2) GetSpawnPoints()
    {
        Transform chosen1 = null;
        Transform chosen2 = null;

        List<Transform> availablePoints = spawnPoints.ToList();
        
        int randIndex = Random.Range(0, availablePoints.Count);

        chosen1 = availablePoints[randIndex];
        availablePoints.RemoveAt(randIndex);

        randIndex = Random.Range(0, availablePoints.Count);
        chosen2 = availablePoints[randIndex];

        return (chosen1, chosen2);
    }
}
