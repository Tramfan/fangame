using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreText : MonoBehaviour
{
    private TextMeshProUGUI scoretext;
    public static int Score ;

    void Start () 
    {
        scoretext = FindObjectOfType<TextMeshProUGUI> ();
    }

    void Update ()

    {
        scoretext.text = Score.ToString();
    }
}
