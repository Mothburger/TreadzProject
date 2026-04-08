using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Scoring : MonoBehaviour
{
    public Map[] maps;
    public TMP_Text score1Text;
    public TMP_Text score2Text;
    public int p1Score;
    public int p2Score;
    public int p1RoundsWon;
    public int p2RoundsWon;
    public int pointsPerRound;
    public int numRounds;

    private void Start()
    {
        p1Score = 0;
        p2Score = 0;
        p1RoundsWon = 0;
        p2RoundsWon = 0;
    }


    public void P1GetPoint()
    {
        p1Score++;
        score1Text.text = p1Score.ToString();
    }

    public void P2GetPoint()
    {
        p2Score++;
        score2Text.text = p2Score.ToString();
    }

    public void SwitchMaps()
    {
        foreach (Map map in maps)
        {
            map.gameObject.SetActive(false);
        }

        maps[Random.Range(0, maps.Length)].gameObject.SetActive(true);


    }

    public void CheckPoints()
    {
        if (p1Score > pointsPerRound) 
        {
            p1RoundsWon++;

            p1Score = 0; p2Score = 0;
        }

        if (p2Score > pointsPerRound)
        {
            p2RoundsWon++;

            p1Score = 0; p2Score = 0;
        }


    }
}
