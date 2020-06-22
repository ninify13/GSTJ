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
    public Vector2 LevelSpeedMinMax;
    public int LevelTime;

    public int BossTime;
    public int BossFires;

    public int Fires;
    public int Coins;
}
