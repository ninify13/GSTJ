using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelMeta", menuName = "Meta/Levels", order = 0)]
public class LevelMeta : ScriptableObject
{
    public List<LevelMetaObject> Levels;
}

[Serializable]
public class LevelMetaObject
{
    public int LevelTime;
    public int Fires;
    public int Coins;
}
