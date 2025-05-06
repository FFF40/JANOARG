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
        Load();
    } 

    public void Load()
    {
        Scores.Load();
    }

    public void Save() 
    {
        Scores.Save();
        Common.main.Storage.Save();
    }
}