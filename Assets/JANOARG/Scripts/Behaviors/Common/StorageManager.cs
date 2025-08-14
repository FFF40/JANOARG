using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Xml.Serialization;
using Unity.VisualScripting;

public class StorageManager : MonoBehaviour
{
    public static StorageManager main;

    public ScoreStore Scores = new();

    void Awake()
    {
        main = this;
        LoadScores();
    } 

    public void LoadScores()
    {
        Scores.Load();
    }

    public void SaveScores() 
    {
        Scores.Save();
        Common.main.Storage.Save();
    }
}