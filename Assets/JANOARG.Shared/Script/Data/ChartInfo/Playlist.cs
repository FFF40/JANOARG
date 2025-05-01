using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Playlist", menuName = "JANOARG/Playlist", order = 100)]
public class Playlist : ScriptableObject
{
    public List<string> ItemPaths;
}


