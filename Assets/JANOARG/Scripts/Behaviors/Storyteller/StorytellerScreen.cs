using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorytellerScreen : MonoBehaviour
{
    public static StorytellerScreen main;

    public StoryScript ScriptToPlay;

    public void Awake()
    {
        main = this;
        CommonScene.Load();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ScriptToPlay) 
        {
            Storyteller.main.PlayScript(ScriptToPlay);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
